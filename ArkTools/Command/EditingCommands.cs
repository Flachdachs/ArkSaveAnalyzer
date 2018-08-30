using System;
using System.Collections.Generic;

namespace ArkTools.Command {

    public abstract class EditingBaseCommand : MonoOptions.Command {

        protected EditingBaseCommand(string name, string help = null) : base(name, help) {
            Run = RunCommand;
        }

        protected virtual void RunCommand(IEnumerable<string> args) {
            throw new NotImplementedException(GetType().ToString());
            // todo implementation
        }

    }

    public class FeedCommand : EditingBaseCommand {

        private const string names = "feed";
        private const string help = "Sets food of all tamed creatures to max and brings them into the present. "
                + "Mainly useful if you left your server running with no players online.";

        public FeedCommand() : base(names, help) { }

    }

    public class ExportCommand : EditingBaseCommand {

        private const string names = "export";
        private const string help = "Export a specified object/dino and everything attached to it. "
                + "Can be used to 'revive' dinos from backups or to import bases from another save file. "
                + "Or to do whatever else you want. "
                + "Manually editing exported file might be required.";

        public ExportCommand() : base(names, help) { }

    }

    public class ImportCommand : EditingBaseCommand {

        private const string names = "import";
        private const string help = "Imports all objects from JSON into SAVE.";

        public ImportCommand() : base(names, help) { }

    }

    public class ModifyCommand : EditingBaseCommand {

        private const string names = "modify";
        private const string help = "Applies the actions defined in MODIFICATION to the specified INPUT file.";

        public ModifyCommand() : base(names, help) { }

    }

}
