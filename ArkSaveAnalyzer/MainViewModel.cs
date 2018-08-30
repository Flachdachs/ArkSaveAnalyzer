using System.Windows;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight;

namespace ArkSaveAnalyzer {

    public class MainViewModel : ViewModelBase {

        public MainViewModel() {
            if (string.IsNullOrWhiteSpace(Settings.Default.ArkSavedDirectory) || string.IsNullOrWhiteSpace(Settings.Default.WorkingDirectory)) {
                MessageBox.Show("Ark Saved Folder and/or Working Directory not configured.");
            }
        }

    }

}
