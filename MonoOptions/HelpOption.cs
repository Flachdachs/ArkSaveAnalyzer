namespace MonoOptions {

    internal class HelpOption : Option {

        private readonly Option option;
        private readonly CommandSet commands;

        public HelpOption(CommandSet commands, Option d)
                : base(d.Prototype, d.Description, d.MaxValueCount, d.Hidden) {
            this.commands = commands;
            option = d;
        }

        protected override void OnParseComplete(OptionContext c) {
            commands.showHelp = true;

            option?.InvokeOnParseComplete(c);
        }

    }

}