using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Maps {

    public static class MapService {
        public static async Task<List<GameObject>> ReadTames(string mapName, string filename) {
            GameObjectContainer gameObjectContainer = filename != null ? await SavegameService.GetGameObjectsForFile(filename) : await SavegameService.GetGameObjects(mapName);

            return extractTames(gameObjectContainer, MapData.For(mapName)).ToList();
        }

        private static IEnumerable<GameObject> extractTames(GameObjectContainer gameObjectContainer, MapData mapData) {
            return gameObjectContainer.Where(o => o.IsCreature() && o.IsTamed());
        }

        public static async Task<IEnumerable<StructuresViewModel>> ReadStructures(string mapName, string filename) {
            GameObjectContainer gameObjectContainer = filename != null ? await SavegameService.GetGameObjectsForFile(filename) : await SavegameService.GetGameObjects(mapName);

            return extractStructures(gameObjectContainer, MapData.For(mapName))
                    .OrderBy(s => s.Key)
                    .Select(structureLine => new StructuresViewModel(structureLine.Key.Item1, structureLine.Key.Item2, structureLine.Count(), structureLine.Key.Item3, structureLine.ToList()));
        }

        private static IEnumerable<IGrouping<Tuple<float, float, bool>, GameObject>> extractStructures(GameObjectContainer gameObjectContainer, MapData mapData) {
            return gameObjectContainer
                    .Where(o => !o.IsCreature() && !o.IsDeathItemCache() && o.IsTamed() && (o.HasAnyProperty("OwningPlayerID") || o.HasAnyProperty("OwnerName")))
                    .GroupBy(gameObject => {
                        double lat = 0, lon = 0;
                        if (gameObject.Location != null) {
                            lon = gameObject.Location.X / mapData.LonDiv + mapData.LonShift;
                            lat = gameObject.Location.Y / mapData.LatDiv + mapData.LatShift;
                        }

                        bool hidden = gameObject.GetPropertyValue<bool>("IsHidden");
                        return new Tuple<float, float, bool>((float)Math.Round(lat, 1), (float)Math.Round(lon, 1), hidden);
                    });
        }
    }

}
