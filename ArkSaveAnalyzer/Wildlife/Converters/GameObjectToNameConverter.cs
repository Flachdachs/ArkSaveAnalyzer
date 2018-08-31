using System;
using System.Globalization;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Wildlife.Converters {

    public class GameObjectToNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject)value;
            return gameObject.GetNameForCreature(ArkDataService.GetArkData().Result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
