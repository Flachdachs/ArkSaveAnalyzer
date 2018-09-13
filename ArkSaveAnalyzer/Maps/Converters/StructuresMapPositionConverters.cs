using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;

namespace ArkSaveAnalyzer.Maps.Converters {
    public class StructuresMapPositionConverterX : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            try {
                double lat = (float)values[0];
                double lon = (float)values[1];
                MapData mapData = (MapData)values[2];
                Rect imageBoundaryGps = (Rect)values[3];

                lat = (lat - mapData.LatShift) * mapData.LatDiv;
                lon = (lon - mapData.LonShift) * mapData.LonDiv;

                Rect imageBoundaryUe = new Rect(
                    new Point((imageBoundaryGps.X - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Y - mapData.LatShift) * mapData.LatDiv),
                    new Point((imageBoundaryGps.Right - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Bottom - mapData.LatShift) * mapData.LatDiv));

                Point positionInImage = new Point(lon - imageBoundaryUe.X, lat - imageBoundaryUe.Y);

                return positionInImage.X * 1024 / imageBoundaryUe.Width;
            } catch (Exception e) {
                Debug.WriteLine(e);
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class StructuresMapPositionConverterY : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            try {
                double lat = (float)values[0];
                double lon = (float)values[1];
                MapData mapData = (MapData)values[2];
                Rect imageBoundaryGps = (Rect)values[3];

                lat = (lat - mapData.LatShift) * mapData.LatDiv;
                lon = (lon - mapData.LonShift) * mapData.LonDiv;

                Rect imageBoundaryUe = new Rect(
                    new Point((imageBoundaryGps.X - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Y - mapData.LatShift) * mapData.LatDiv),
                    new Point((imageBoundaryGps.Right - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Bottom - mapData.LatShift) * mapData.LatDiv));

                Point positionInImage = new Point(lon - imageBoundaryUe.X, lat - imageBoundaryUe.Y);

                return positionInImage.Y * 1024 / imageBoundaryUe.Height;
            } catch (Exception e) {
                Debug.WriteLine(e);
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}