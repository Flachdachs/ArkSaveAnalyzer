using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Maps.Converters {

    public class CreatureMapPositionConverterX : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            try {
                if (!(values[0] is GameObject creature)) {
                    return 0;
                }
                if (!(values[1] is MapData mapData)) {
                    return 0;
                }
                Rect imageBoundaryGps = (Rect)values[2];

                Rect imageBoundaryUe = new Rect(
                        new Point((imageBoundaryGps.X - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Y - mapData.LatShift) * mapData.LatDiv),
                        new Point((imageBoundaryGps.Right - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Bottom - mapData.LatShift) * mapData.LatDiv));

                Point positionInImage = new Point(creature.Location.X - imageBoundaryUe.X, creature.Location.Y - imageBoundaryUe.Y);

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

    public class CreatureMapPositionConverterY : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            try {
                if (!(values[0] is GameObject creature)) {
                    return 0;
                }
                if (!(values[1] is MapData mapData)) {
                    return 0;
                }
                Rect imageBoundaryGps = (Rect)values[2];

                Rect imageBoundaryUe = new Rect(
                        new Point((imageBoundaryGps.X - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Y - mapData.LatShift) * mapData.LatDiv),
                        new Point((imageBoundaryGps.Right - mapData.LonShift) * mapData.LonDiv, (imageBoundaryGps.Bottom - mapData.LatShift) * mapData.LatDiv));

                Point positionInImage = new Point(creature.Location.X - imageBoundaryUe.X, creature.Location.Y - imageBoundaryUe.Y);

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
