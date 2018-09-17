namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class FileSystemWatchMessage {
        public string MapName { get; }

        public FileSystemWatchMessage(string mapName) {
            MapName = mapName;
        }
    }
}
