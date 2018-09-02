using System;
using System.Globalization;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.WikiMap.Converters {

    public class CreatureTooltipConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject)values[0];
            MapData mapData = (MapData)values[1];

            return $"{(gameObject.GetNameForCreature(ArkDataService.GetArkData().Result) ?? gameObject.ClassString)}: " +
                    $"{gameObject.GetPropertyValue<string>("TamedName")} " +
                    $"{(gameObject.GetPropertyValue<bool>("bIsFemale") ? "♀" : "♂")} {gameObject.GetFullLevel()} - " +
                    $"{mapData.CalculateLat(gameObject.Location.Y):F1} / {mapData.CalculateLon(gameObject.Location.X):F1}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
