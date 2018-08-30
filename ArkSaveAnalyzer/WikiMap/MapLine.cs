using SavegameToolkit;

namespace ArkSaveAnalyzer.WikiMap {

    public class MapLine {
        public string LineContent { get; }
        public GameObject GameObject { get; }

        public MapLine(string lineContent, GameObject gameObject) {
            LineContent = lineContent;
            GameObject = gameObject;
        }
    }

}
