using System.IO;

namespace ArkTools {

    public static class GlobalOptions {

        public static ArkToolsConfiguration ArkToolsConfiguration { get; set; }
        public static bool ShowHelp { get; set; }
        public static string Language { get; set; }
        public static bool MemoryMapping { get; set; }
        public static bool Parallel { get; set; }
        public static bool PrettyPrinting { get; set; } = true;
        public static bool Quiet { get; set; }
        public static bool UseStopWatch { get; set; } = true;
        public static int ThreadCount { get; set; } = 4;
        public static bool Verbose { get; set; }
        public static TextWriter Out { get; set; } = System.Console.Out;
        public static bool Compact { get; set; }

        static GlobalOptions() {
            ArkToolsConfiguration = new ArkToolsConfiguration();
        }
    }

}
