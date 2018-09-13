using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Maps.Converters {
    public class CreatureToTamerConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject) value;
            return $"{gameObject.GetPropertyValue<string>("TamerString")}, {gameObject.GetPropertyValue<string>("ImprinterName")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
