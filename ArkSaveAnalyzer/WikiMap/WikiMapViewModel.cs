using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;

namespace ArkSaveAnalyzer.WikiMap {

    public class WikiMapViewModel : ViewModelBase {
        private string currentMapName;
        private string fileName;

        public RelayCommand<string> MapCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand<MapLine> ShowDataCommand { get; }
        public RelayCommand CopyCommand { get; }

        public ObservableCollection<MapLine> MapLinesTames { get; } = new ObservableCollection<MapLine>();
        public ObservableCollection<MapLine> MapLinesStructures { get; } = new ObservableCollection<MapLine>();

        #region UiEnabled

        private bool uiEnabled = true;

        public bool UiEnabled {
            get => uiEnabled;
            set => Set(ref uiEnabled, value);
        }

        #endregion

        public WikiMapViewModel() {
            MapCommand = new RelayCommand<string>(mapName => map(mapName), s => UiEnabled);
            OpenFileCommand = new RelayCommand(openFile, () => UiEnabled && !string.IsNullOrEmpty(currentMapName));
            ShowDataCommand = new RelayCommand<MapLine>(showData, m => UiEnabled);
            CopyCommand = new RelayCommand(copy, () => UiEnabled);
        }

        private void openFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                    FileName = fileName,
                    CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true) {
                fileName = openFileDialog.FileName;
                // todo get mapename from user input
                map(currentMapName, fileName);
            }
        }

        private void copy() {
            Clipboard.SetText(string.Join(Environment.NewLine, MapLinesTames.Select(line => line.LineContent)));
        }

        private void showData(MapLine mapLine) {
            if (mapLine.GameObject != null) {
                Messenger.Default.Send(new ShowGameObjectMessage(mapLine.GameObject));
            }
        }

        private async void map(string mapName, string filename = null) {
            UiEnabled = false;

            currentMapName = mapName;

            try {
                MapLinesTames.Clear();
                foreach (MapLine mapLine in await WikiMapService.ReadTames(mapName, filename)) {
                    MapLinesTames.Add(mapLine);
                }

                MapLinesStructures.Clear();
                foreach (MapLine mapLine in await WikiMapService.ReadStructures(mapName, filename)) {
                    MapLinesStructures.Add(mapLine);
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            UiEnabled = true;
        }
    }

}
