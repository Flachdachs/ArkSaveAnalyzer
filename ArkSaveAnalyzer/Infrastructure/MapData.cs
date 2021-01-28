using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ArkSaveAnalyzer.Infrastructure {
    public class MapData {
        public string Name { get; }
        public string Directory { get; }
        public double LatShift { get; }
        public double LonShift { get; }
        public double LatDiv { get; }
        public double LonDiv { get; }
        public Rect BoundaryArtistic { get; }
        public Rect BoundaryTopographic { get; }

        public MapData(string name, double latShift, double lonShift, double latDiv, double lonDiv, string directory = null,
            double topX = 0, double topY = 0, double bottomX = 100, double bottomY = 100,
            double topXTopographic = 0, double topYTopographic = 0, double bottomXTopographic = 100, double bottomYTopographic = 100) {
            Name = name;
            Directory = directory ?? name;
            LatShift = latShift;
            LonShift = lonShift;
            LatDiv = latDiv;
            LonDiv = lonDiv;
            BoundaryArtistic = new Rect(new Point(topX, topY), new Point(bottomX, bottomY));
            BoundaryTopographic = new Rect(new Point(topXTopographic, topYTopographic), new Point(bottomXTopographic, bottomYTopographic));
        }

        private static readonly Dictionary<string, MapData> mapData = new Dictionary<string, MapData> {
            {"TheIsland", new MapData("TheIsland", 50, 50, 8000, 8000, "", 0, 0, 100, 100, 8.9, 8.4, 90.8, 92.4)},
            {"ScorchedEarth", new MapData("ScorchedEarth_P", 50, 50, 8000, 8000, null, 0, 0, 100, 100, 6.5, 4.1, 93.9, 95.6)},
            {"Aberration", new MapData("Aberration_P", 50, 50, 8000, 8000, null, 0, 0, 100, 100, 10.8, 9.2, 90.7, 89.7)},
            {"Extinction", new MapData("Extinction", 50, 50, 8000, 8000, null, 0, 0, 100, 100, 0, 0, 100, 100)},
            {"Genesis1", new MapData("Genesis", 50, 50, 10500, 10500, null, -1.4, -1.4, 99.2, 101, 10, 10, 90, 90)},
            {"TheCenter", new MapData("TheCenter", 30.342237, 55.104168, 9584, 9600, null, 0, -2, 100, 100, 1, -2.5, 104.5, 101.0)},
            {"Ragnarok", new MapData("Ragnarok", 50.009388, 50.009388, 13100, 13100, null, 0, 0, 100, 100, 0, 0, 100, 100)},
            {"Valguero", new MapData("Valguero_P", 50, 50, 8161, 8161, null, -1.2, -2.2, 99.5, 101.2, 0, 0, 100, 100)},
            {"CrystalIsles", new MapData("CrystalIsles", 48.75, 50, 16000, 17000, null, -1.8, -1.4, 97.4, 103, 0, 0, 100, 100)}
        };

        public static MapData For(string mapName) {
            return string.IsNullOrWhiteSpace(mapName) ? mapData.First().Value : mapData[mapName];
        }

        public static string GetMapName(string filename) {
            string mapFilename = Path.GetFileNameWithoutExtension(filename);
            return mapData.FirstOrDefault(pair => pair.Value.Name == mapFilename).Key;
        }

        public double CalculateLat(float y) {
            return LatShift + y / LatDiv;
        }

        public double CalculateLon(float x) {
            return LonShift + x / LonDiv;
        }
    }
}
