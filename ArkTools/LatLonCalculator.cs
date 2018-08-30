using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SavegameToolkit;

namespace ArkTools {

    public class MapData {
        public float LatShift { get; }
        public float LatDiv { get; }
        public float LonShift { get; }
        public float LonDiv { get; }

        public MapData(float latShift, float latDiv, float lonShift, float lonDiv) {
            LatShift = latShift;
            LatDiv = latDiv;
            LonShift = lonShift;
            LonDiv = lonDiv;
        }

        public float CalculateLat(float y) {
            return LatShift + y / LatDiv;
        }

        public float CalculateLon(float x) {
            return LonShift + x / LonDiv;
        }

    }

    public class LatLonCalculator {
        public const string Filename = "LatLonCalculator.json";

        private static Dictionary<string, MapData> knownMaps { get; set; } = new Dictionary<string, MapData>();

        private static readonly MapData defaultMapData = new MapData(50f, 8000f, 50f, 8000f);

        static LatLonCalculator() {
            if (importList())
                return;
            knownMaps.Clear();
            knownMaps.Add("TheIsland", defaultMapData);
            knownMaps.Add("ScorchedEarth", defaultMapData);
            knownMaps.Add("Aberration", defaultMapData);
            knownMaps.Add("TheCenter", new MapData(30.34223747253418f, 9584f, 55.10416793823242f, 9600f));
            knownMaps.Add("Ragnarok", new MapData(50.009388f, 13100f, 50.009388f, 13100f));
            knownMaps.Add("Valhalla", new MapData(48.813560485839844f, 14750f, 48.813560485839844f, 14750f));
            knownMaps.Add("MortemTupiu", new MapData(32.479148864746094f, 20000f, 40.59893798828125f, 16000f));
            knownMaps.Add("ShigoIslands", new MapData(50f, 8128f, 50f, 8128f));
            knownMaps.Add("TheVolcano", new MapData(50f, 9181f, 50f, 9181f));
            knownMaps.Add("PGARK", new MapData(0f, 6080f, 0f, 6080f));
        }

        /// <summary>
        /// Exports current list of known maps
        /// </summary>
        /// <param name="generator"></param>
        public static void ExportList(JsonTextWriter generator, WritingOptions writingOptions) {
            JsonSerializer.CreateDefault().Serialize(generator, knownMaps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if latLonCalculator.json could be found and import was successful</returns>
        private static bool importList() {
            try {
                string filename = Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, Filename);
                knownMaps = JsonSerializer.CreateDefault().Deserialize<Dictionary<string, MapData>>(new JsonTextReader(new StreamReader(filename)));
                return true;
            } catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException) {
                return false;
            }
        }

        /// <summary>
        /// Tries to find the best match for the given {@code savegame}
        /// </summary>
        /// <param name="savegame">The savegame to find a LatLonCalculator for</param>
        /// <returns>a LatLonCalculator for the given <code>savegame</code> or <see cref="defaultMapData"/></returns>
        public static MapData ForSave(ArkSavegame savegame) {
            string mapName = savegame.DataFiles[0];
            return knownMaps.TryGetValue(mapName, out MapData map) ? map : defaultMapData;
        }
    }
}
