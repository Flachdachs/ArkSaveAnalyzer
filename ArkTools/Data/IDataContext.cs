using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkTools.Data {
    public interface IDataContext {
        MapData MapData { get; }

        GameObjectContainer ObjectContainer { get; }

        ArkSavegame Savegame { get; }
    }
}
