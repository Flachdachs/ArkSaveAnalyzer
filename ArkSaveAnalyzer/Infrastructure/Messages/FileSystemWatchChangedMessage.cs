namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class FileSystemWatchChangedMessage {
        public string MapName { get; }

        public FileSystemWatchChangedMessage(string mapName) {
            MapName = mapName;
        }
    }
}
