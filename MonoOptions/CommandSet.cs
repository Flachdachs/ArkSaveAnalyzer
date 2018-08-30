using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace MonoOptions {

    public class CommandSet : List<Command> {

        internal List<CommandSet> NestedCommandSets;

        internal HelpCommand help;

        internal bool showHelp;

        internal OptionSet Options { get; private set; }

        public CommandSet(string suite, Converter<string, string> localizer = null)
                : this(suite, Console.Out, Console.Error, localizer) { }

        public CommandSet(string suite, TextWriter output, TextWriter error, Converter<string, string> localizer = null) {

            Suite = suite ?? throw new ArgumentNullException(nameof(suite));
            Options = new CommandOptionSet(this, localizer);
            Out = output ?? throw new ArgumentNullException(nameof(output));
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public string Suite { get; }

        public TextWriter Out { get; private set; }

        public TextWriter Error { get; private set; }

        public Converter<string, string> MessageLocalizer => Options.MessageLocalizer;

        //protected override string GetKeyForItem(Command command) {
        //    return command?.Name;
        //}

        public new CommandSet Add(Command command) {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            addCommand(command);
            Options.Add(new CommandOption(command));
            return this;
        }

        private void addCommand(Command command) {
            if (command.CommandSet != null && command.CommandSet != this) {
                throw new ArgumentException("Command instances can only be added to a single CommandSet.", nameof(command));
            }

            command.CommandSet = this;
            if (command.Options != null) {
                command.Options.MessageLocalizer = Options.MessageLocalizer;
            }

            base.Add(command);

            help = help ?? command as HelpCommand;
        }

        public CommandSet Add(string header) {
            Options.Add(header);
            return this;
        }

        public CommandSet Add(Option option) {
            Options.Add(option);
            return this;
        }

        public CommandSet Add(string prototype, Action<string> action) {
            Options.Add(prototype, action);
            return this;
        }

        public CommandSet Add(string prototype, string description, Action<string> action, bool hidden = false) {
            Options.Add(prototype, description, action, hidden);
            return this;
        }

        public CommandSet Add(string prototype, Action<string, string> action) {
            Options.Add(prototype, action);
            return this;
        }

        public CommandSet Add(string prototype, string description, Action<string, string> action, bool hidden = false) {
            Options.Add(prototype, description, action, hidden);
            return this;
        }

        public CommandSet Add<T>(string prototype, Action<T> action) {
            Options.Add(prototype, null, action);
            return this;
        }

        public CommandSet Add<T>(string prototype, string description, Action<T> action) {
            Options.Add(prototype, description, action);
            return this;
        }

        public CommandSet Add<TKey, TValue>(string prototype, Action<TKey, TValue> action) {
            Options.Add(prototype, action);
            return this;
        }

        public CommandSet Add<TKey, TValue>(string prototype, string description, Action<TKey, TValue> action) {
            Options.Add(prototype, description, action);
            return this;
        }

        public CommandSet Add(ArgumentSource source) {
            Options.Add(source);
            return this;
        }

        public CommandSet Add(CommandSet nestedCommands) {
            if (nestedCommands == null)
                throw new ArgumentNullException(nameof(nestedCommands));

            if (NestedCommandSets == null) {
                NestedCommandSets = new List<CommandSet>();
            }

            if (!alreadyAdded(nestedCommands)) {
                NestedCommandSets.Add(nestedCommands);
                foreach (Option o in nestedCommands.Options) {
                    if (o is CommandOption c) {
                        Options.Add(new CommandOption(c.Command, $"{nestedCommands.Suite} {c.CommandName}"));
                    } else {
                        Options.Add(o);
                    }
                }
            }

            nestedCommands.Options = Options;
            nestedCommands.Out = Out;
            nestedCommands.Error = Error;

            return this;
        }

        private bool alreadyAdded(CommandSet commandSet) {
            if (commandSet == this)
                return true;
            if (NestedCommandSets == null)
                return false;
            foreach (CommandSet nc in NestedCommandSets) {
                if (nc.alreadyAdded(commandSet))
                    return true;
            }

            return false;
        }

        public int Run(IEnumerable<string> arguments) {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            showHelp = false;
            if (help == null) {
                help = new HelpCommand();
                addCommand(help);
            }

            void setHelp(string v) => showHelp = v != null;
            if (!Options.Contains("help")) {
                Options.Add("help", "", setHelp, true);
            }

            if (!Options.Contains("?")) {
                Options.Add("?", "", setHelp, true);
            }

            List<string> extra = Options.Parse(arguments);
            if (!extra.Any()) {
                if (showHelp) {
                    return help.Invoke(extra);
                }

                Out.WriteLine(Options.MessageLocalizer($"Use `{Suite} help` for usage."));
                return 1;
            }

            Command command = GetCommand(extra);
            if (command == null) {
                help.WriteUnknownCommand(extra[0]);
                return 1;
            }

            if (showHelp) {
                if (command.Options?.Contains("help") ?? true) {
                    extra.Add("--help");
                    return command.Invoke(extra);
                }

                command.Options.WriteOptionDescriptions(Out);
                return 0;
            }

            return command.Invoke(extra);
        }

        internal Command GetCommand(List<string> extra) {
            return tryGetLocalCommand(extra) ?? tryGetNestedCommand(extra);
        }

        private Command tryGetLocalCommand(List<string> extra) {
            string name = extra[0];
            //if (Contains(name)) {
            //    extra.RemoveAt(0);
            //    return this[name];
            //}
            foreach (Command command in this) {
                if (command.Name.Split('|', ',').Select(n => n.Trim()).Contains(name)) {
                    extra.RemoveAt(0);
                    return command;
                }
            }

            for (int i = 1; i < extra.Count; ++i) {
                name = name + " " + extra[i];
                //if (Contains(name)) {
                //    extra.RemoveRange(0, i + 1);
                //    return this[name];
                //}
                foreach (Command command in this) {
                    if (command.Name.Split('|', ',').Select(n => n.Trim()).Contains(name)) {
                        extra.RemoveRange(0, i + 1);
                        return command;
                    }
                }
            }

            return null;
        }

        private Command tryGetNestedCommand(List<string> extra) {

            CommandSet nestedCommands = NestedCommandSets?.Find(c => c.Suite == extra[0]);
            if (nestedCommands == null)
                return null;

            List<string> extraCopy = new List<string>(extra);
            extraCopy.RemoveAt(0);
            if (extraCopy.Count == 0)
                return null;

            Command command = nestedCommands.GetCommand(extraCopy);
            if (command != null) {
                extra.Clear();
                extra.AddRange(extraCopy);
                return command;
            }

            return null;
        }

    }

}