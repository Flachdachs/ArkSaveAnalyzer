﻿using System;
using System.Globalization;
using System.Windows.Data;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Infrastructure.Converters {

    public class GameObjectToStructureNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GameObject gameObject = (GameObject)value;
            return gameObject.GetNameForStructure(ArkDataService.GetArkData().Result) ?? gameObject.ClassString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
