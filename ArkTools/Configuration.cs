namespace ArkTools {

    public class Configuration {

        public ArkToolsConfiguration ArkToolsConfiguration { get; set; }

        public Configuration() {
            ArkToolsConfiguration = new ArkToolsConfiguration();
        }

    }

    public class ArkToolsConfiguration {

        public string BasePath { get; set; }
        public string OutputJsonExtension { get; set; }
        public string CreaturesPath { get; set; }
        public string PlayersPath { get; set; }
        public string TribesPath { get; set; }

        public string ArkSavegameFilename { get; set; }
        public string ArkProfileFilename { get; set; }
        public string ArkTribeFilename { get; set; }
        public string ArkCloudFilename { get; set; }
        public string ArkLocalProfileFilename { get; set; }
        public string ArkSavFileFilename { get; set; }
        public string UntameableClasses { get; set; }
    }

}
