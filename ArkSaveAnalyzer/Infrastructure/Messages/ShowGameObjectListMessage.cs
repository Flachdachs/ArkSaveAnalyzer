using System.Collections.Generic;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class ShowGameObjectListMessage {
        public List<GameObject> GameObjects { get; }
        public string Caption { get; }
        public MapData MapData { get; }

        public ShowGameObjectListMessage(List<GameObject> gameObjects, MapData mapData, string caption = null) {
            GameObjects = gameObjects;
            MapData = mapData;
            Caption = caption;
        }
    }
}
