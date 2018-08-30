using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoOptions;
using SavegameToolkitAdditions;

namespace ArkTools.Command {

    public abstract class SettingsBaseCommand : BaseCommand {

        protected virtual string[] HelpHeaders { get; set; } = { };

        protected SettingsBaseCommand(string name, string help = null) : base(name, help) {
            Options = new OptionSet();
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (string helpHeader in HelpHeaders) {
                Options.Add(helpHeader);
            }

            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }

    }

    public class LatLonCommand : SettingsBaseCommand {

        private const string names = "latlon";
        private const string help = "Exports internal LatLonCalculator data to latLonCalculator.json in the current working directory";

        public LatLonCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string path = Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, LatLonCalculator.Filename);
            
            CommonFunctions.WriteJson(path, LatLonCalculator.ExportList, writingOptions);
        }
    }

}
