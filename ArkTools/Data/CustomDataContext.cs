using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkTools.Data {

    public class CustomDataContext : IDataContext {
        public MapData MapData { get; set; }

        public GameObjectContainer ObjectContainer { get; set; }

        public ArkSavegame Savegame { get; set; }
    }

}
