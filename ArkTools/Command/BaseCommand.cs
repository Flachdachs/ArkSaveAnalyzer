using System.Collections.Generic;
using SavegameToolkit;

namespace ArkTools.Command {

    public abstract class BaseCommand : MonoOptions.Command {
        protected bool showHelp;

        protected BaseCommand(string name, string help = null) : base(name, help) {
            Run = RunCommand;
        }

        protected static WritingOptions writingOptions => WritingOptions.Create()
                .WithThreadCount(GlobalOptions.ThreadCount)
                .Parallel(GlobalOptions.Parallel)
                .WithMemoryMapping(GlobalOptions.MemoryMapping)
                .CompactOutput(GlobalOptions.Compact);

        protected static ReadingOptions readingOptions => ReadingOptions.Create()
                .WithThreadCount(GlobalOptions.ThreadCount)
                .Parallel(GlobalOptions.Parallel)
                .WithMemoryMapping(GlobalOptions.MemoryMapping);

        protected abstract void RunCommand(IEnumerable<string> args);

        protected virtual bool showCommandHelp(IEnumerable<string> args) {
            if (!GlobalOptions.ShowHelp && !showHelp)
                return false;
            //todo real help text output
            GlobalOptions.Out.WriteLine($"Help for {Name}:");

            return true;
        }
    }

}
