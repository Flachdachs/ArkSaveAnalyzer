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

using ArkSaveAnalyzer.Configuration;
using ArkSaveAnalyzer.Maps;
using ArkSaveAnalyzer.Savegame;
using ArkSaveAnalyzer.Wildlife;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace ArkSaveAnalyzer {
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator {
        public ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            //SimpleIoc.Default.Register<SavegameService>();

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
