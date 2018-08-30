using System;
using System.Linq;

namespace MonoOptions {

    internal class CommandOptionSet : OptionSet {

        private readonly CommandSet commands;

        public CommandOptionSet(CommandSet commands, Converter<string, string> localizer)
                : base(localizer) {
            this.commands = commands;
        }

        protected override void SetItem(int index, Option item) {
            if (shouldWrapOption(item)) {
                base.SetItem(index, new HelpOption(commands, item));
                return;
            }

            base.SetItem(index, item);
        }

        private static bool shouldWrapOption(Option item) {
            if (item == null)
                return false;
            if (item is HelpOption)
                return false;
            return item.Names.Any(n => n == "help");
        }

        protected override void InsertItem(int index, Option item) {
            if (shouldWrapOption(item)) {
                base.InsertItem(index, new HelpOption(commands, item));
                return;
            }

            base.InsertItem(index, item);
        }

    }

}
