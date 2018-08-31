using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Wildlife.Converters {

    public class GameObjectToSexSymbolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject)value;
            return gameObject.GetPropertyValue<bool>("bIsFemale") ? "♀" : "♂";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
