using SavegameToolkit;

namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class ShowGameObjectMessage {
        public GameObject GameObject { get; }
        public string Caption { get; }

        public ShowGameObjectMessage(GameObject gameObject, string caption = null) {
            GameObject = gameObject;
            Caption = caption;
        }
    }
}
