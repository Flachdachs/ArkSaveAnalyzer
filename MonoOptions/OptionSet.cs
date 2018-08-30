using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoOptions {

    public class OptionSet : KeyedCollection<string, Option> {

        public OptionSet()
                : this(null) { }

        public OptionSet(Converter<string, string> localizer) {
            ArgumentSources = new ReadOnlyCollection<ArgumentSource>(sources);
            MessageLocalizer = localizer ?? (f => f);
        }

        public Converter<string, string> MessageLocalizer { get; internal set; }

        private readonly List<ArgumentSource> sources = new List<ArgumentSource>();

        public ReadOnlyCollection<ArgumentSource> ArgumentSources { get; }

        protected override string GetKeyForItem(Option item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        protected override void InsertItem(int index, Option item) {
            base.InsertItem(index, item);
            addImpl(item);
        }

        protected override void RemoveItem(int index) {
            Option p = Items[index];
            base.RemoveItem(index);
            // KeyedCollection.RemoveItem() handles the 0th item
            for (int i = 1; i < p.Names.Length; ++i) {
                Dictionary.Remove(p.Names[i]);
            }
        }

        protected override void SetItem(int index, Option item) {
            base.SetItem(index, item);
            addImpl(item);
        }

        private void addImpl(Option option) {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
            List<string> added = new List<string>(option.Names.Length);
            try {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (int i = 1; i < option.Names.Length; ++i) {
                    Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            } catch (Exception) {
                foreach (string name in added)
                    Dictionary.Remove(name);
                throw;
            }
        }

        public OptionSet Add(string header) {
            if (header == null)
                throw new ArgumentNullException(nameof(header));
            Add(new Category(header));
            return this;
        }

        internal sealed class Category : Option {

            // Prototype starts with '=' because this is an invalid prototype
            // (see Option.ParsePrototype(), and thus it'll prevent Category
            // instances from being accidentally used as normal options.
            public Category(string description)
                    : base("=:Category:= " + description, description) { }

            protected override void OnParseComplete(OptionContext c) {
                throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
            }

        }

        public new OptionSet Add(Option option) {
            base.Add(option);
            return this;
        }

        private sealed class ActionOption : Option {

            private readonly Action<OptionValueCollection> action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action, bool hidden = false)
                    : base(prototype, description, count, hidden) {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c) {
                action(c.OptionValues);
            }

        }

        public OptionSet Add(string prototype, Action<string> action) {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, Action<string> action, bool hidden = false) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Option p = new ActionOption(prototype, description, 1,
                    v => action(v[0]), hidden);
            base.Add(p);
            return this;
        }

        public OptionSet Add(string prototype, Action<string, string> action) {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, Action<string, string> action, bool hidden = false) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Option p = new ActionOption(prototype, description, 2,
                    delegate(OptionValueCollection v) {
                        action(v[0], v[1]);
                    }, hidden);
            base.Add(p);
            return this;
        }

        private sealed class ActionOption<T> : Option {

            private readonly Action<T> action;

            public ActionOption(string prototype, string description, Action<T> action)
                    : base(prototype, description, 1) {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c) {
                action(Parse<T>(c.OptionValues[0], c));
            }

        }

        private sealed class ActionOption<TKey, TValue> : Option {

            private readonly Action<TKey, TValue> action;

            public ActionOption(string prototype, string description, Action<TKey, TValue> action)
                    : base(prototype, description, 2) {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c) {
                action(
                        Parse<TKey>(c.OptionValues[0], c),
                        Parse<TValue>(c.OptionValues[1], c));
            }

        }

        public OptionSet Add<T>(string prototype, Action<T> action) {
            return Add(prototype, null, action);
        }

        public OptionSet Add<T>(string prototype, string description, Action<T> action) {
            return Add(new ActionOption<T>(prototype, description, action));
        }

        public OptionSet Add<TKey, TValue>(string prototype, Action<TKey, TValue> action) {
            return Add(prototype, null, action);
        }

        public OptionSet Add<TKey, TValue>(string prototype, string description, Action<TKey, TValue> action) {
            return Add(new ActionOption<TKey, TValue>(prototype, description, action));
        }

        public OptionSet Add(ArgumentSource source) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            sources.Add(source);
            return this;
        }

        protected virtual OptionContext CreateOptionContext() {
            return new OptionContext(this);
        }

        public List<string> Parse(IEnumerable<string> arguments) {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));
            OptionContext c = CreateOptionContext();
            c.OptionIndex = -1;
            bool process = true;
            List<string> unprocessed = new List<string>();
            Option def = Contains("<>") ? this["<>"] : null;
            ArgumentEnumerator ae = new ArgumentEnumerator(arguments);
            foreach (string argument in ae) {
                ++c.OptionIndex;
                if (argument == "--") {
                    process = false;
                    continue;
                }

                if (!process) {
                    OptionSet.unprocessed(unprocessed, def, c, argument);
                    continue;
                }

                if (addSource(ae, argument))
                    continue;
                if (!Parse(argument, c))
                    OptionSet.unprocessed(unprocessed, def, c, argument);
            }

            c.Option?.Invoke(c);
            return unprocessed;
        }

        private class ArgumentEnumerator : IEnumerable<string> {

            private readonly List<IEnumerator<string>> sources = new List<IEnumerator<string>>();

            public ArgumentEnumerator(IEnumerable<string> arguments) {
                sources.Add(arguments.GetEnumerator());
            }

            public void Add(IEnumerable<string> arguments) {
                sources.Add(arguments.GetEnumerator());
            }

            public IEnumerator<string> GetEnumerator() {
                do {
                    IEnumerator<string> c = sources[sources.Count - 1];
                    if (c.MoveNext())
                        yield return c.Current;
                    else {
                        c.Dispose();
                        sources.RemoveAt(sources.Count - 1);
                    }
                } while (sources.Count > 0);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

        }

        private bool addSource(ArgumentEnumerator ae, string argument) {
            foreach (ArgumentSource source in sources) {
                if (!source.GetArguments(argument, out IEnumerable<string> replacement))
                    continue;
                ae.Add(replacement);
                return true;
            }

            return false;
        }

        private static bool unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument) {
            if (def == null) {
                extra.Add(argument);
                return false;
            }

            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
            return false;
        }

        private readonly Regex ValueOption = new Regex(
                @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value) {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            flag = name = sep = value = null;
            Match m = ValueOption.Match(argument);
            if (!m.Success) {
                return false;
            }

            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;
            if (m.Groups["sep"].Success && m.Groups["value"].Success) {
                sep = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }

            return true;
        }

        protected virtual bool Parse(string argument, OptionContext c) {
            if (c.Option != null) {
                parseValue(argument, c);
                return true;
            }

            if (!GetOptionParts(argument, out string f, out string n, out string s, out string v))
                return false;

            if (Contains(n)) {
                Option p = this[n];
                c.OptionName = f + n;
                c.Option = p;
                switch (p.OptionValueType) {
                    case OptionValueType.None:
                        c.OptionValues.Add(n);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        parseValue(v, c);
                        break;
                }

                return true;
            }

            // no match; is it a bool option?
            return parseBool(argument, n, c) || parseBundledValue(f, string.Concat(n + s + v), c);
            // is it a bundled option?

        }

        private void parseValue(string option, OptionContext c) {
            if (option != null)
                foreach (string o in c.Option.ValueSeparators != null
                        ? option.Split(c.Option.ValueSeparators, c.Option.MaxValueCount - c.OptionValues.Count, StringSplitOptions.None)
                        : new[] { option }) {
                    c.OptionValues.Add(o);
                }

            if (c.OptionValues.Count == c.Option.MaxValueCount ||
                    c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount) {
                throw new OptionException(MessageLocalizer(string.Format(
                                "Error: Found {0} option values when expecting {1}.",
                                c.OptionValues.Count, c.Option.MaxValueCount)),
                        c.OptionName);
            }
        }

        private bool parseBool(string option, string n, OptionContext c) {
            string rn;
            if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') &&
                    Contains(rn = n.Substring(0, n.Length - 1))) {
                Option p = this[rn];
                string v = n[n.Length - 1] == '+' ? option : null;
                c.OptionName = option;
                c.Option = p;
                c.OptionValues.Add(v);
                p.Invoke(c);
                return true;
            }

            return false;
        }

        private bool parseBundledValue(string f, string n, OptionContext c) {
            if (f != "-")
                return false;
            for (int i = 0; i < n.Length; ++i) {
                Option p;
                string opt = f + n[i];
                string rn = n[i].ToString();
                if (!Contains(rn)) {
                    if (i == 0)
                        return false;
                    throw new OptionException(string.Format(MessageLocalizer(
                            "Cannot use unregistered option '{0}' in bundle '{1}'."), rn, f + n), null);
                }

                p = this[rn];
                switch (p.OptionValueType) {
                    case OptionValueType.None:
                        invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required: {
                        string v = n.Substring(i + 1);
                        c.Option = p;
                        c.OptionName = opt;
                        parseValue(v.Length != 0 ? v : null, c);
                        return true;
                    }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }

            return true;
        }

        private static void invoke(OptionContext c, string name, string value, Option option) {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        private const int OptionWidth = 29;
        private const int Description_FirstWidth = 80 - OptionWidth;
        private const int Description_RemWidth = 80 - OptionWidth - 2;

        private static readonly string CommandHelpIndentStart = new string(' ', OptionWidth);
        private static readonly string CommandHelpIndentRemaining = new string(' ', OptionWidth + 2);

        public void WriteOptionDescriptions(TextWriter o) {
            foreach (Option p in this) {
                int written = 0;

                if (p.Hidden)
                    continue;

                if (p is Category c) {
                    writeDescription(o, p.Description, "", 80, 80);
                    continue;
                }

                if (p is CommandOption co) {
                    WriteCommandDescription(o, co.Command, co.CommandName);
                    continue;
                }

                if (!writeOptionPrototype(o, p, ref written))
                    continue;

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                writeDescription(o, p.Description, new string(' ', OptionWidth + 2),
                        Description_FirstWidth, Description_RemWidth);
            }

            foreach (ArgumentSource s in sources) {
                string[] names = s.GetNames();
                if (names == null || names.Length == 0)
                    continue;

                int written = 0;

                write(o, ref written, "  ");
                write(o, ref written, names[0]);
                for (int i = 1; i < names.Length; ++i) {
                    write(o, ref written, ", ");
                    write(o, ref written, names[i]);
                }

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                writeDescription(o, s.Description, new string(' ', OptionWidth + 2),
                        Description_FirstWidth, Description_RemWidth);
            }
        }

        internal void WriteCommandDescription(TextWriter o, Command c, string commandName) {
            string name = new string(' ', 8) + (commandName ?? c.Name);
            if (name.Length < OptionWidth - 1) {
                writeDescription(o, name + new string(' ', OptionWidth - name.Length) + c.Help, CommandHelpIndentRemaining, 80, Description_RemWidth);
            } else {
                writeDescription(o, name, "", 80, 80);
                writeDescription(o, CommandHelpIndentStart + c.Help, CommandHelpIndentRemaining, 80, Description_RemWidth);
            }
        }

        private void writeDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth) {
            bool indent = false;
            foreach (string line in getLines(MessageLocalizer(getDescription(value)), firstWidth, remWidth)) {
                if (indent)
                    o.Write(prefix);
                o.WriteLine(line);
                indent = true;
            }
        }

        private bool writeOptionPrototype(TextWriter o, Option p, ref int written) {
            string[] names = p.Names;

            int i = getNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1) {
                write(o, ref written, "  -");
                write(o, ref written, names[0]);
            } else {
                write(o, ref written, "      --");
                write(o, ref written, names[0]);
            }

            for (i = getNextOptionIndex(names, i + 1);
                    i < names.Length;
                    i = getNextOptionIndex(names, i + 1)) {
                write(o, ref written, ", ");
                write(o, ref written, names[i].Length == 1 ? "-" : "--");
                write(o, ref written, names[i]);
            }

            if (p.OptionValueType == OptionValueType.Optional ||
                    p.OptionValueType == OptionValueType.Required) {
                if (p.OptionValueType == OptionValueType.Optional) {
                    write(o, ref written, MessageLocalizer("["));
                }

                write(o, ref written, MessageLocalizer("=" + getArgumentName(0, p.MaxValueCount, p.Description)));
                string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                        ? p.ValueSeparators[0]
                        : " ";
                for (int c = 1; c < p.MaxValueCount; ++c) {
                    write(o, ref written, MessageLocalizer(sep + getArgumentName(c, p.MaxValueCount, p.Description)));
                }

                if (p.OptionValueType == OptionValueType.Optional) {
                    write(o, ref written, MessageLocalizer("]"));
                }
            }

            return true;
        }

        private static int getNextOptionIndex(string[] names, int i) {
            while (i < names.Length && names[i] == "<>") {
                ++i;
            }

            return i;
        }

        private static void write(TextWriter o, ref int n, string s) {
            n += s.Length;
            o.Write(s);
        }

        private static string getArgumentName(int index, int maxIndex, string description) {
            MatchCollection matches = Regex.Matches(description ?? "", @"(?<=(?<!\{)\{)[^{}]*(?=\}(?!\}))"); // ignore double braces 
            string argName = "";
            foreach (Match match in matches) {
                string[] parts = match.Value.Split(':');
                // for maxIndex=1 it can be {foo} or {0:foo}
                if (maxIndex == 1) {
                    argName = parts[parts.Length - 1];
                }

                // look for {i:foo} if maxIndex > 1
                if (maxIndex > 1 && parts.Length == 2 &&
                        parts[0] == index.ToString(CultureInfo.InvariantCulture)) {
                    argName = parts[1];
                }
            }

            if (string.IsNullOrEmpty(argName)) {
                argName = maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
            }

            return argName;
        }

        private static string getDescription(string description) {
            if (description == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder(description.Length);
            int start = -1;
            for (int i = 0; i < description.Length; ++i) {
                switch (description[i]) {
                    case '{':
                        if (i == start) {
                            sb.Append('{');
                            start = -1;
                        } else if (start < 0)
                            start = i + 1;

                        break;
                    case '}':
                        if (start < 0) {
                            if (i + 1 == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        } else {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }

                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }

            return sb.ToString();
        }

        private static IEnumerable<string> getLines(string description, int firstWidth, int remWidth) {
            return StringCoda.WrappedLines(description, firstWidth, remWidth);
        }

    }

}