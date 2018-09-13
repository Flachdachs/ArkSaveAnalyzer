using System;
using System.Globalization;
using System.Windows.Data;

namespace ArkSaveAnalyzer.Infrastructure.Converters {

    public class BooleanToTrueStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is bool b && b ? "true" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
