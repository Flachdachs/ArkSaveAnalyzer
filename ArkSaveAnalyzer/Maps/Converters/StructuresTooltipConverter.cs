using System;
using System.Globalization;
using System.Windows.Data;

namespace ArkSaveAnalyzer.Maps.Converters {
    public class StructuresTooltipConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            float lat = (float) values[0];
            float lon = (float) values[1];

            return $"{lat:F1} / {lon:F1}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
