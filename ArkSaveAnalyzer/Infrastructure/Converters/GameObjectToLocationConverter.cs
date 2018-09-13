using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Infrastructure.Converters {
    public class GameObjectToLocationConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject) values[0];

            MapData mapData;
            if (values[1] is string mapName) {
                if (string.IsNullOrEmpty(mapName)) {
                    throw new ArgumentException("Map name missing.");
                }

                mapData = MapData.For(mapName);
            }
            else {
                mapData = (MapData) values[1];
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:F2} {1:F2}", 
                gameObject.Location.Y / mapData.LatDiv + mapData.LatShift, 
                gameObject.Location.X / mapData.LonDiv + mapData.LonShift);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
