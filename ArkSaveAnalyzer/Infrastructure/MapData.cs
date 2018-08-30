using System.Collections.Generic;

namespace ArkSaveAnalyzer.Infrastructure {

    public class MapData {

        public string Name { get; }
        public string Directory { get; }
        public double LatShift { get; }
        public double LonShift { get; }
        public double LatDiv { get; }
        public double LonDiv { get; }

        public MapData(string name, double latShift, double lonShift, double latDiv, double lonDiv, string directory = null) {
            Name = name;
            Directory = directory ?? name;
            LatShift = latShift;
            LonShift = lonShift;
            LatDiv = latDiv;
            LonDiv = lonDiv;
        }

        private static readonly Dictionary<string, MapData> mapData = new Dictionary<string, MapData> {
            {"TheIsland", new MapData("TheIsland", 50, 50, 8000, 8000, "")},
            {"ScorchedEarth", new MapData("ScorchedEarth_P", 50, 50, 8000, 8000)},
            {"Aberration", new MapData("Aberration_P", 50, 50, 8000, 8000)},
            {"TheCenter", new MapData("TheCenter", 30.342237, 55.104168, 9584, 9600)},
            {"Ragnarok", new MapData("Ragnarok", 50.009388, 50.009388, 13100, 13100)}
        };

        public static MapData For(string mapName) {
            return mapData[mapName];
        }

    }
}
