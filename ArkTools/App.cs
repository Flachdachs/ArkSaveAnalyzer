using System;
using System.Diagnostics;
using System.IO;
using ArkTools.Command;
using Microsoft.Extensions.Configuration;
using MonoOptions;
using Newtonsoft.Json;

namespace ArkTools {

    public class App {
        public static void Main(string[] args) {
            initConfiguration();

            try {
                CommandSet commandSet = initCommands();
                commandSet.Run(args);
            } catch (Exception e) when (!Debugger.IsAttached) {
                Console.Error.WriteLine(e);
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static CommandSet initCommands() {
            return new CommandSet("arktools") {
                    "Usage: arktools command [OPTIONS]",
                    "",
                    "Converting",
                    new MapToJsonCommand(),
                    new ProfileToJsonCommand(),
                    new TribeToJsonCommand(),
                    new CloudToJsonCommand(),
                    new LocalProfileToJsonCommand(),
                    new SavToJsonCommand(),
                    "",
                    //new JsonToMapCommand(),
                    //new JsonToProfileCommand(),
                    //new JsonToTribeCommand(),
                    //new JsonToCloudCommand(),
                    //new JsonToLocalProfileCommand(),
                    //new JsonToSavCommand(),
                    //"",
                    "Creatures",
                    new CreaturesCommand(),
                    new TamedCommand(),
                    new WildCommand(),
                    "",
                    "Players",
                    new PlayersCommand(),
                    new TribesCommand(),
                    new ClusterCommand(),
                    "",
                    "Settings",
                    new LatLonCommand(),
                    "",
                    "Update",
                    new UpdateDataCommand(),
                    new VersionCommand(),
                    "",
                    //"DB",
                    //new DbCommand(),
                    //new DbDriversCommand(),
                    //"",
                    //"Editing",
                    //new FeedCommand(),
                    //new ExportCommand(),
                    //new ImportCommand(),
                    //new ModifyCommand(),
                    //"",
                    "Debug",
                    new ClassesCommand(),
                    new DumpCommand(),
                    new SizesCommand(),
                    "",
                    "",
                    "Options",
                    { "h|help", "Displays this help screen, use with a command to get contextual help.", s => GlobalOptions.ShowHelp = s != null },
                    { "lang=", "Load data for specified language, needs appropriate ark_data_lang.json.", s => GlobalOptions.Language = s },
                    { "m|mmap", "If set memory mapping will be used. Efficency depends on available RAM and OS.", s => GlobalOptions.MemoryMapping = s != null },
                    { "p|parallel", "If set files will be processed by multiple threads.", s => GlobalOptions.Parallel = s != null },
                    { "thread-count=", "Amount of threads to use for parallel tasks.", (int? n) => GlobalOptions.ThreadCount = n ?? 4 },
                    { "pretty-printing", "If set all JSON output will use pretty printing.", s => GlobalOptions.PrettyPrinting = s != null },
                    { "c|compact", "Compact output.", s => GlobalOptions.Compact = s != null },
                    { "q|quiet", "Suppresses output to stdout.", s => GlobalOptions.Quiet = s != null },
                    { "s|stopwatch", "Measure time spent.", s => GlobalOptions.UseStopWatch = s != null },
                    { "v|verbose", "Prints stack traces for potentially corrupt files.", s => GlobalOptions.Verbose = s != null }
            };
        }

        private static void initConfiguration() {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true);
            IConfigurationRoot configuration = builder.Build();

            GlobalOptions.ArkToolsConfiguration = configuration.GetSection("arktools")
                    .Get<ArkToolsConfiguration>();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                    //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Include
            };
        }
    }

}
