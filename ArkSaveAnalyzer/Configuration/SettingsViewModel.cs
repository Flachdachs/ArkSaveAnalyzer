using System.Windows.Forms;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace ArkSaveAnalyzer.Configuration {

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsViewModel : ViewModelBase {
        private string arkSavedFolder;

        public string ArkSavedFolder {
            get => arkSavedFolder;
            set {
                Set(ref arkSavedFolder, value);
                Settings.Default.ArkSavedDirectory = value;
                Settings.Default.Save();
            }
        }

        private string workingDirectory;

        public string WorkingDirectory {
            get => workingDirectory;
            set {
                Set(ref workingDirectory, value);
                Settings.Default.WorkingDirectory = value;
                Settings.Default.Save();
            }
        }

        private string excludedWildlife;

        public string ExcludedWildlife {
            get => excludedWildlife;
            set {
                if (Set(ref excludedWildlife, value)) {
                    Settings.Default.ExcludedWildlife = excludedWildlife;
                    Settings.Default.Save();
                }
            }
        }

        public RelayCommand ChooseSavedFolder { get; }
        public RelayCommand ChooseWorkingDirectory { get; }
        public RelayCommand UpdateCommand { get; }

        public SettingsViewModel() {
            ChooseSavedFolder = new RelayCommand(chooseSavedFolder);
            ChooseWorkingDirectory = new RelayCommand(chooseWorkingDirectory);
            UpdateCommand = new RelayCommand(update);

            ArkSavedFolder = Settings.Default.ArkSavedDirectory;
            WorkingDirectory = Settings.Default.WorkingDirectory;
            ExcludedWildlife = Settings.Default.ExcludedWildlife;
        }

        private void chooseSavedFolder() {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.ShowNewFolderButton = false;
                dialog.SelectedPath = ArkSavedFolder;

                if (dialog.ShowDialog() == DialogResult.OK) {
                    ArkSavedFolder = dialog.SelectedPath;
                }
            }
        }

        private void chooseWorkingDirectory() {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.ShowNewFolderButton = false;
                dialog.SelectedPath = WorkingDirectory;

                if (dialog.ShowDialog() == DialogResult.OK) {
                    WorkingDirectory = dialog.SelectedPath;
                }
            }
        }

        private async void update() {
            await ArkDataService.GetArkData(true);
        }
    }

}
