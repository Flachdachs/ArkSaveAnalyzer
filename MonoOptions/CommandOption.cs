using System;

namespace MonoOptions {

    internal class CommandOption : Option {

        public Command Command { get; }
        public string CommandName { get; }

        // Prototype starts with '=' because this is an invalid prototype
        // (see Option.ParsePrototype(), and thus it'll prevent Category
        // instances from being accidentally used as normal options.
        public CommandOption(Command command, string commandName = null, bool hidden = false)
                : base("=:Command:= " + (commandName ?? command?.Name), commandName ?? command?.Name, 0, hidden) {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            CommandName = commandName ?? command.Name;
        }

        protected override void OnParseComplete(OptionContext c) {
            throw new NotSupportedException("CommandOption.OnParseComplete should not be invoked.");
        }

    }

}