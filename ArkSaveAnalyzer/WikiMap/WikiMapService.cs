using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.WikiMap {

    public static class WikiMapService {
        public static async Task<List<MapLine>> ReadTames(string mapName, string filename) {
            GameObjectContainer gameObjectContainer = filename != null ? await SavegameService.GetGameObjectsForFile(filename) : await SavegameService.GetGameObjects(mapName);

            return extractTames(gameObjectContainer, MapData.For(mapName)).ToList();
        }

        private static IEnumerable<MapLine> extractTames(GameObjectContainer gameObjectContainer, MapData mapData) {
            return gameObjectContainer.Where(o => o.IsCreature() && o.IsTamed()).Select(gameObject => {
                double lat = 0, lon = 0;
                if (gameObject.Location != null) {
                    lon = gameObject.Location.X / mapData.LonDiv + mapData.LonShift;
                    lat = gameObject.Location.Y / mapData.LatDiv + mapData.LatShift;
                }

                string output = string.Format(CultureInfo.InvariantCulture,
                        "| {0:0.##}, {1:0.##}, {2}, ({3}) {4}: {5} - {6}/{7}",
                        lat, lon,
                        "tame",
                        gameObject.Id,
                        gameObject.ClassString.Replace("_Character_BP_C", string.Empty),
                        gameObject.GetPropertyValue<string>("TamedName"),
                        gameObject.GetPropertyValue<string>("TamerString"),
                        gameObject.GetPropertyValue<string>("ImprinterName"));

                return new MapLine(output, gameObject);
            });
        }

        public static async Task<IEnumerable<MapLine>> ReadStructures(string mapName, string filename) {
            GameObjectContainer gameObjectContainer = filename != null ? await SavegameService.GetGameObjectsForFile(filename) : await SavegameService.GetGameObjects(mapName);

            return extractStructures(gameObjectContainer, MapData.For(mapName))
                    .OrderBy(s => s)
                    .Select(structureLine => new MapLine(structureLine, null));
        }

        private static IEnumerable<string> extractStructures(GameObjectContainer gameObjectContainer, MapData mapData) {
            return gameObjectContainer
                    .Where(o => !o.IsCreature() && o.IsTamed() && (o.HasAnyProperty("OwningPlayerID") || o.HasAnyProperty("OwnerName")))
                    .Select(gameObject => {
                        double lat = 0, lon = 0;
                        if (gameObject.Location != null) {
                            lon = gameObject.Location.X / mapData.LonDiv + mapData.LonShift;
                            lat = gameObject.Location.Y / mapData.LatDiv + mapData.LatShift;
                        }

                        string type = gameObject.GetPropertyValue<bool>("IsHidden") ? "structure-hidden" : "structure";
                        return FormattableString.Invariant($"| {lat:0.#}, {lon:0.#}, {type}");
                    })
                    .ToHashSet();
        }
    }

}
