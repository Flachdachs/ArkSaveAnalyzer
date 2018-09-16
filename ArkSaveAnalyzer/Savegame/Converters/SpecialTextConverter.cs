using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Savegame.Converters {

    [ValueConversion(typeof(GameObject), typeof(string))]
    public class SpecialTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = value as GameObject;

            float? itemRating = gameObject.GetPropertyValue<float?>("ItemRating");
            if (itemRating.HasValue) {
                return $"{gameObject.GetPropertyValue<int>("ItemQualityIndex")}-{itemRating.Value}, " +
                        $"{gameObject.GetPropertyValue<int>("ItemStatValues", 2)}, {gameObject.GetPropertyValue<int>("ItemStatValues", 3)}";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}