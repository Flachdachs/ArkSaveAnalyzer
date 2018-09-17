/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ArkSaveAnalyzer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System.ComponentModel;
using System.Windows;
using ArkSaveAnalyzer.Configuration;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Maps;
using ArkSaveAnalyzer.Savegame;
using ArkSaveAnalyzer.Wildlife;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace ArkSaveAnalyzer {
    public class ViewModelLocator {
        public ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (!ViewModelBase.IsInDesignModeStatic) {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                ArkDataService.EnsureArkDataAvailability();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MapViewModel>();
            SimpleIoc.Default.Register<GameObjectViewModel>();
            SimpleIoc.Default.Register<GameObjectListViewModel>();
            SimpleIoc.Default.Register<SavegameViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<WildlifeViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public MapViewModel Map => ServiceLocator.Current.GetInstance<MapViewModel>();
        public SavegameViewModel Savegame => ServiceLocator.Current.GetInstance<SavegameViewModel>();
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public WildlifeViewModel Wildlife => ServiceLocator.Current.GetInstance<WildlifeViewModel>();

        public static void Cleanup() {
            // Clear the ViewModels
        }
    }
}
