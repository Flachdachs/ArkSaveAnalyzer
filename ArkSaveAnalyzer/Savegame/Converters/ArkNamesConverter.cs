using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Savegame.Converters {

    [ValueConversion(typeof(GameObject), typeof(string))]
    public class ArkNamesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return string.Join(", ", ((GameObject)value)?.Names?.Select(name => name.Name) ?? Enumerable.Empty<string>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}