namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class InvalidateMapDataMessage {
        public string MapName { get; }

        public InvalidateMapDataMessage(string mapName) {
            MapName = mapName;
        }
    }
}
