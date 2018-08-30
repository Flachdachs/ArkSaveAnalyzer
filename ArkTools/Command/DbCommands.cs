using System;
using System.Collections.Generic;

namespace ArkTools.Command {

    public abstract class DbBaseCommand : MonoOptions.Command {

        protected DbBaseCommand(string name, string help = null) : base(name, help) {
            Run = RunCommand;
        }

        protected virtual void RunCommand(IEnumerable<string> args) {
            throw new NotImplementedException(GetType().ToString());
            // todo implementation
        }

    }

    public class DbCommand : DbBaseCommand {

        private const string names = "db";
        private const string help = "Reads the SAVE, all players, all tribes and cluster data and outputs the result to URI_OR_PATH";

        public DbCommand() : base(names, help) { }

    }

    public class DbDriversCommand : DbBaseCommand {

        private const string names = "db-drivers, dbDrivers";
        private const string help = "Lists all installed drivers with their configuration parameters";

        public DbDriversCommand() : base(names, help) { }

    }

}
