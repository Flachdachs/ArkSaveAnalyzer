using System;
using System.Globalization;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkSaveAnalyzer.Wildlife.Converters {
    public class GameObjectToLocationConverter:IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject)values[0];

            string mapName = (string)values[1];
            if (string.IsNullOrEmpty(mapName)) {
                throw new ArgumentException("Map name missing.");
            }

            MapData mapData = MapData.For(mapName);

            LocationData location = gameObject.Location;

            return $"{location.X / mapData.LatDiv + mapData.LatShift:F2} {location.Y / mapData.LonDiv + mapData.LonShift:F2}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
