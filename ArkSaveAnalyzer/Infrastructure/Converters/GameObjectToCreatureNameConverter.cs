using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Infrastructure.Converters {
    public class GameObjectToCreatureNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = value as GameObject;
            return gameObject.GetNameForCreature(ArkDataService.ArkData) ?? gameObject?.ClassString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
