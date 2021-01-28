using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MessageBox = System.Windows.MessageBox;

namespace ArkSaveAnalyzer.Configuration {

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsViewModel : ViewModelBase {
        #region ArkSavedFolder

        private string arkSavedFolder;

        public string ArkSavedFolder {
            get => arkSavedFolder;
            set {
                Set(ref arkSavedFolder, value);
                Settings.Default.ArkSavedDirectory = value;
                Settings.Default.Save();
            }
        }

        #endregion

        #region WorkingDirectory

        private string workingDirectory;

        public string WorkingDirectory {
            get => workingDirectory;
            set {
                Set(ref workingDirectory, value);
                Settings.Default.WorkingDirectory = value;
                Settings.Default.Save();
            }
        }

        #endregion

        #region ExcludedWildlife

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

        #endregion

        #region WishListWildlife

        private string wishListWildlife;

        public string WishListWildlife {
            get => wishListWildlife;
            set {
                if (Set(ref wishListWildlife, value)) {
                    Settings.Default.WishListWildlife = wishListWildlife;
                    Settings.Default.Save();
                }
            }
        }

        #endregion

        public RelayCommand ChooseSavedFolder { get; }
        public RelayCommand ChooseWorkingDirectory { get; }
        public RelayCommand UpdateCommand { get; }

        public SettingsViewModel() {
            ChooseSavedFolder = new RelayCommand(chooseSavedFolder);
            ChooseWorkingDirectory = new RelayCommand(chooseWorkingDirectory);
            UpdateCommand = new RelayCommand(update);

            if (!IsInDesignMode) {
                ArkSavedFolder = Settings.Default.ArkSavedDirectory;
                WorkingDirectory = Settings.Default.WorkingDirectory;
                ExcludedWildlife = Settings.Default.ExcludedWildlife;
                WishListWildlife = Settings.Default.WishListWildlife;
            }

            Messenger.Default.Register<WildlifeExcludeMessage>(this, message => handleWildlifeExclude(message.Name));
            Messenger.Default.Register<WildlifeWishListMessage>(this, message => handleWildlifeWishList(message.Name));
        }

        private void handleWildlifeExclude(string name) {
            List<string> excluded = ExcludedWildlife
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (excluded.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                return;
            excluded.Add(name);
            excluded.Sort(StringComparer.InvariantCulture);
            ExcludedWildlife = string.Join(Environment.NewLine, excluded);
        }

        private void handleWildlifeWishList(string name) {
            List<string> wishList = WishListWildlife
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (wishList.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                return;
            wishList.Add(name);
            wishList.Sort(StringComparer.InvariantCulture);
            WishListWildlife = string.Join(Environment.NewLine, wishList);
        }

        private void chooseSavedFolder() {
            using FolderBrowserDialog dialog = new FolderBrowserDialog {
                    ShowNewFolderButton = false,
                    SelectedPath = ArkSavedFolder
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                ArkSavedFolder = dialog.SelectedPath;
            }
        }

        private void chooseWorkingDirectory() {
            using FolderBrowserDialog dialog = new FolderBrowserDialog {
                    ShowNewFolderButton = false,
                    SelectedPath = WorkingDirectory
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                WorkingDirectory = dialog.SelectedPath;
            }
        }

        private async void update() {
            await ArkDataService.DownloadArkData();
            MessageBox.Show("Done.");
        }
    }

}
