using System;
using System.Globalization;
using System.Windows.Data;
using ArkSaveAnalyzer.Infrastructure;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Maps.Converters {
    public class CreatureTooltipConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is GameObject gameObject && values[1] is MapData mapData) {
                return $"{gameObject.GetNameForCreature(ArkDataService.ArkData) ?? gameObject.ClassString}: " +
                       $"{gameObject.GetPropertyValue<string>("TamedName")} " +
                       $"{(gameObject.IsFemale() ? "♀" : "♂")} {gameObject.GetFullLevel()} - " +
                       $"{mapData?.CalculateLat(gameObject.Location.Y):F1} / {mapData?.CalculateLon(gameObject.Location.X):F1}";
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
