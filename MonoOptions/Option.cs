using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MonoOptions {

    public abstract class Option {

        protected Option(string prototype, string description, int maxValueCount = 1, bool hidden = false) {
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", nameof(prototype));
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValueCount));

            Prototype = prototype;
            Description = description;
            MaxValueCount = maxValueCount;
            Names = this is OptionSet.Category
                    // append GetHashCode() so that "duplicate" categories have distinct
                    // names, e.g. adding multiple "" categories should be valid.
                    ? new[] { prototype + GetHashCode() }
                    : prototype.Split('|');

            if (this is OptionSet.Category || this is CommandOption)
                return;

            OptionValueType = parsePrototype();
            Hidden = hidden;

            if (MaxValueCount == 0 && OptionValueType != OptionValueType.None)
                throw new ArgumentException(
                        "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                        "OptionValueType.Optional.",
                        nameof(maxValueCount));
            if (OptionValueType == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                        string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
                        nameof(maxValueCount));
            if (Array.IndexOf(Names, "<>") >= 0 &&
                    (Names.Length == 1 && OptionValueType != OptionValueType.None ||
                            Names.Length > 1 && MaxValueCount > 1))
                throw new ArgumentException(
                        "The default option handler '<>' cannot require values.",
                        nameof(prototype));
        }

        public string Prototype { get; }
        public string Description { get; }
        public OptionValueType OptionValueType { get; }
        public int MaxValueCount { get; }
        public bool Hidden { get; }

        public string[] GetNames() {
            return (string[])Names.Clone();
        }

        public string[] GetValueSeparators() {
            if (ValueSeparators == null)
                return new string[0];
            return (string[])ValueSeparators.Clone();
        }

        protected static T Parse<T>(string value, OptionContext c) {
            Type tt = typeof(T);
            Type ti = tt;
            bool nullable =
                    ti.IsValueType &&
                    ti.IsGenericType &&
                    !ti.IsGenericTypeDefinition &&
                    ti.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type targetType = nullable ? tt.GetGenericArguments()[0] : tt;
            T t = default(T);
            try {
                if (value != null) {
                    TypeConverter conv = TypeDescriptor.GetConverter(targetType);
                    t = (T)conv.ConvertFromString(value);
                }
            } catch (Exception e) {
                throw new OptionException(
                        string.Format(
                                c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                                value, targetType.Name, c.OptionName),
                        c.OptionName, e);
            }

            return t;
        }

        internal string[] Names { get; }
        internal string[] ValueSeparators { get; private set; }

        private static readonly char[] NameTerminator = { '=', ':' };

        private OptionValueType parsePrototype() {
            char type = '\0';
            List<string> seps = new List<string>();
            for (int i = 0; i < Names.Length; ++i) {
                string name = Names[i];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", "prototype");

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                Names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                            $"Conflicting option types: '{type}' vs. '{name[end]}'.",
                            "prototype");
                addSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (MaxValueCount <= 1 && seps.Count != 0)
                throw new ArgumentException(
                        $"Cannot provide key/value separators for Options taking {MaxValueCount} value(s).",
                        "prototype");
            if (MaxValueCount > 1) {
                if (seps.Count == 0)
                    ValueSeparators = new[] { ":", "=" };
                else if (seps.Count == 1 && seps[0].Length == 0)
                    ValueSeparators = null;
                else
                    ValueSeparators = seps.ToArray();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void addSeparators(string name, int end, ICollection<string> seps) {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i) {
                switch (name[i]) {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                    $"Ill-formed name/value separator found in \"{name}\".",
                                    "prototype");
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                    $"Ill-formed name/value separator found in \"{name}\".",
                                    "prototype");
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }

            if (start != -1)
                throw new ArgumentException(
                        $"Ill-formed name/value separator found in \"{name}\".",
                        "prototype");
        }

        public void Invoke(OptionContext c) {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);

        internal void InvokeOnParseComplete(OptionContext c) {
            OnParseComplete(c);
        }

        public override string ToString() {
            return Prototype;
        }

    }

}