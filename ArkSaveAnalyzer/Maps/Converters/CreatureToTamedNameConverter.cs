﻿using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Maps.Converters {
    public class CreatureToTamedNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = value as GameObject;
            return gameObject.GetPropertyValue<string>("TamedName");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
