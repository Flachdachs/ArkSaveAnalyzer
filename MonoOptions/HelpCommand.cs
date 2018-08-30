using System;
using System.Collections.Generic;
using System.Linq;
using MessageLocalizerConverter = System.Converter<string, string>;

namespace MonoOptions {

    public class HelpCommand : Command {

        public HelpCommand()
                : base("help", "Show this message and exit") { }

        public override int Invoke(IEnumerable<string> arguments) {
            List<string> extra = arguments.ToList();
            Console.WriteLine($"# Help requested for: {string.Join(" ", extra)}");
            MessageLocalizerConverter _ = CommandSet.Options.MessageLocalizer;
            if (extra.Count == 0) {
                CommandSet.Options.WriteOptionDescriptions(CommandSet.Out);
                return 0;
            }

            Command command = CommandSet.GetCommand(extra);
            if (command == this || extra.Contains("--help")) {
                CommandSet.Out.WriteLine(_($"Usage: {CommandSet.Suite} COMMAND [OPTIONS]"));
                CommandSet.Out.WriteLine(_($"Use `{CommandSet.Suite} help COMMAND` for help on a specific command."));
                CommandSet.Out.WriteLine();
                CommandSet.Out.WriteLine(_("Available commands:"));
                CommandSet.Out.WriteLine();
                List<KeyValuePair<string, Command>> commands = getCommands();
                commands.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
                foreach (KeyValuePair<string, Command> c in commands) {
                    if (c.Key == "help") {
                        continue;
                    }

                    CommandSet.Options.WriteCommandDescription(CommandSet.Out, c.Value, c.Key);
                }

                CommandSet.Options.WriteCommandDescription(CommandSet.Out, CommandSet.help, "help");
                return 0;
            }

            if (command == null) {
                WriteUnknownCommand(extra[0]);
                return 1;
            }

            if (command.Options != null) {
                command.Options.WriteOptionDescriptions(CommandSet.Out);
                return 0;
            }

            return command.Invoke(new[] { "--help" });
        }

        private List<KeyValuePair<string, Command>> getCommands() {
            List<KeyValuePair<string, Command>> commands = new List<KeyValuePair<string, Command>>();

            foreach (Command c in CommandSet) {
                commands.Add(new KeyValuePair<string, Command>(c.Name, c));
            }

            if (CommandSet.NestedCommandSets == null)
                return commands;

            foreach (CommandSet nc in CommandSet.NestedCommandSets) {
                addNestedCommands(commands, "", nc);
            }

            return commands;
        }

        private static void addNestedCommands(ICollection<KeyValuePair<string, Command>> commands, string outer, CommandSet value) {
            foreach (Command v in value) {
                commands.Add(new KeyValuePair<string, Command>($"{outer}{value.Suite} {v.Name}", v));
            }

            if (value.NestedCommandSets == null)
                return;
            foreach (CommandSet nc in value.NestedCommandSets) {
                addNestedCommands(commands, $"{outer}{value.Suite} ", nc);
            }
        }

        internal void WriteUnknownCommand(string unknownCommand) {
            CommandSet.Error.WriteLine(CommandSet.Options.MessageLocalizer($"{CommandSet.Suite}: Unknown command: {unknownCommand}"));
            CommandSet.Error.WriteLine(CommandSet.Options.MessageLocalizer($"{CommandSet.Suite}: Use `{CommandSet.Suite} help` for usage."));
        }

    }

}
