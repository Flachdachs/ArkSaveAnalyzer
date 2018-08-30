using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoOptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavegameToolkit;

namespace ArkTools.Command {

    public abstract class ConvertingBaseCommand : BaseCommand {

        private bool allowBrokenFile;

        protected ConvertingBaseCommand(string name, string help, IEnumerable<string> helpHeaders) : base(name, help) {
            Options = new OptionSet();
            foreach (string helpHeader in helpHeaders) {
                Options.Add(helpHeader);
            }

            Options.Add("allow-broken-file", "Tries to read as much of broken/truncated files as possible", s => allowBrokenFile = s != null);
            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }

        protected void convertToJson<T>(IEnumerable<string> args, string defaultFilename) where T : IConversionSupport, new() {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string inFilename = argsList.Count > 0 ? argsList[0] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, defaultFilename);

            string outFilename = argsList.Count > 1 ? argsList[1] : Path.ChangeExtension(inFilename, GlobalOptions.ArkToolsConfiguration.OutputJsonExtension);

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);
            T objectToConvert = new T();
            try {
                objectToConvert.ReadBinary<T>(inFilename, readingOptions);
            } catch (Exception) when (!System.Diagnostics.Debugger.IsAttached) {
                if (!allowBrokenFile) {
                    throw;
                }
            }

            stopwatch.Stop("Reading");
            CommonFunctions.WriteJson(outFilename, objectToConvert.WriteJson, writingOptions);
            stopwatch.Stop("Dumping");

            stopwatch.Print();
        }

        protected void convertFromJson<T>(IEnumerable<string> args, string defaultFilename) where T : IConversionSupport, new() {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string inFilename = argsList.Count > 0
                    ? argsList[0]
                    : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath,
                            Path.ChangeExtension(defaultFilename, GlobalOptions.ArkToolsConfiguration.OutputJsonExtension));

            string outFilename = argsList.Count > 1 ? argsList[1] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, defaultFilename);

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);
            T objectToConvert = new T();

            JToken jToken = JToken.ReadFrom(new JsonTextReader(File.OpenText(inFilename)));
            stopwatch.Stop("Parsing");
            objectToConvert.ReadJson(jToken, readingOptions);
            stopwatch.Stop("Loading");
            objectToConvert.WriteBinary(outFilename, writingOptions);
            stopwatch.Stop("Writing");

            stopwatch.Print();
        }
    }

    public class MapToJsonCommand : ConvertingBaseCommand {

        private const string names = "m2j, mapToJson";
        private const string help = "Converts from .ark to .json";
        private static readonly string[] helpHeaders = {
                "",
                "m2j ARK JSON [OPTIONS]",
                ""
        };

        public MapToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkSavegame>(args, GlobalOptions.ArkToolsConfiguration.ArkSavegameFilename);
        }
    }

    public class ProfileToJsonCommand : ConvertingBaseCommand {

        private const string names = "p2j, profileToJson";
        private const string help = "Converts from .arkprofile to .json";
        private static readonly string[] helpHeaders = {
                "",
                "p2j PROFILE JSON [OPTIONS]",
                ""
        };

        public ProfileToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkProfile>(args, GlobalOptions.ArkToolsConfiguration.ArkProfileFilename);
        }
    }

    public class TribeToJsonCommand : ConvertingBaseCommand {

        private const string names = "t2j, tribeToJson";
        private const string help = "Converts from .arktribe to .json";
        private static readonly string[] helpHeaders = {
                "",
                "t2j TRIBE JSON [OPTIONS]",
                ""
        };

        public TribeToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkTribe>(args, GlobalOptions.ArkToolsConfiguration.ArkTribeFilename);
        }
    }

    public class CloudToJsonCommand : ConvertingBaseCommand {

        private const string names = "c2j, cloudToJson";
        private const string help = "Converts cloud data to .json";
        private static readonly string[] helpHeaders = {
                "",
                "c2j CLOUD JSON [OPTIONS]",
                ""
        };

        public CloudToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkCloudInventory>(args, GlobalOptions.ArkToolsConfiguration.ArkCloudFilename);
        }
    }

    public class LocalProfileToJsonCommand : ConvertingBaseCommand {

        private const string names = "l2j, localProfileToJson";
        private const string help = "Converts local profile data to .json";
        private static readonly string[] helpHeaders = {
                "",
                "l2j LOCALPROFILE JSON [OPTIONS]",
                ""
        };

        public LocalProfileToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkLocalProfile>(args, GlobalOptions.ArkToolsConfiguration.ArkLocalProfileFilename);
        }
    }

    public class SavToJsonCommand : ConvertingBaseCommand {

        private const string names = "s2j, savToJson";
        private const string help = "Converts .sav to .json";
        private static readonly string[] helpHeaders = {
                "",
                "s2j SAV JSON [OPTIONS]",
                ""
        };

        public SavToJsonCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertToJson<ArkSavFile>(args, GlobalOptions.ArkToolsConfiguration.ArkSavFileFilename);
        }
    }

    public class JsonToMapCommand : ConvertingBaseCommand {

        private const string names = "j2m, jsonToMap";
        private const string help = "Converts from .json to .ark";
        private static readonly string[] helpHeaders = {
                "",
                "j2m JSON ARK [OPTIONS]",
                ""
        };

        public JsonToMapCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkSavegame>(args, GlobalOptions.ArkToolsConfiguration.ArkSavegameFilename);
        }
    }

    public class JsonToProfileCommand : ConvertingBaseCommand {

        private const string names = "j2p, jsonToProfile";
        private const string help = "Converts from .json to .arkprofile";
        private static readonly string[] helpHeaders = { };

        public JsonToProfileCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkProfile>(args, GlobalOptions.ArkToolsConfiguration.ArkProfileFilename);
        }
    }

    public class JsonToTribeCommand : ConvertingBaseCommand {

        private const string names = "j2t, jsonToTribe";
        private const string help = "Converts from .json to .arktribe";
        private static readonly string[] helpHeaders = { };

        public JsonToTribeCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkTribe>(args, GlobalOptions.ArkToolsConfiguration.ArkTribeFilename);
        }
    }

    public class JsonToCloudCommand : ConvertingBaseCommand {

        private const string names = "j2c, jsonToCloud";
        private const string help = "Converts from .json to cloud data";
        private static readonly string[] helpHeaders = { };

        public JsonToCloudCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkCloudInventory>(args, GlobalOptions.ArkToolsConfiguration.ArkCloudFilename);
        }
    }

    public class JsonToLocalProfileCommand : ConvertingBaseCommand {

        private const string names = "j2l, jsonToLocalProfile";
        private const string help = "Converts from .json to local profile data";
        private static readonly string[] helpHeaders = { };

        public JsonToLocalProfileCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkLocalProfile>(args, GlobalOptions.ArkToolsConfiguration.ArkLocalProfileFilename);
        }
    }

    public class JsonToSavCommand : ConvertingBaseCommand {

        private const string names = "j2s, jsonToSav";
        private const string help = "Converts from .json to .sav";
        private static readonly string[] helpHeaders = { };

        public JsonToSavCommand() : base(names, help, helpHeaders) { }

        protected override void RunCommand(IEnumerable<string> args) {
            convertFromJson<ArkSavFile>(args, GlobalOptions.ArkToolsConfiguration.ArkSavFileFilename);
        }
    }

}
