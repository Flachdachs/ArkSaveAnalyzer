using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;

namespace ArkSaveAnalyzer.WikiMap {

    public class WikiMapViewModel : ViewModelBase {
        private string currentMapName;
        private bool mapStyleTopographic;
        private string fileName;

        public RelayCommand<string> MapCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand<MapLine> ShowDataCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand MapStyleCommand { get; }
        public RelayCommand<MouseButtonEventArgs> PinCommand { get; }

        public ObservableCollection<MapLine> MapLinesTames { get; } = new ObservableCollection<MapLine>();
        public ObservableCollection<MapLine> MapLinesStructures { get; } = new ObservableCollection<MapLine>();
        public ObservableCollection<CreatureViewModel> Creatures { get; } = new ObservableCollection<CreatureViewModel>();

        #region UiEnabled

        private bool uiEnabled = true;

        public bool UiEnabled {
            get => uiEnabled;
            set => Set(ref uiEnabled, value);
        }

        #endregion

        #region MapImage

        private ImageSource mapImage;

        public ImageSource MapImage {
            get => mapImage;
            set => Set(ref mapImage, value);
        }

        #endregion

        #region MapData

        private MapData mapData;

        public MapData MapData {
            get => mapData;
            set => Set(ref mapData, value);
        }

        #endregion

        #region MapBoundary

        private Rect mapBoundary;

        public Rect MapBoundary {
            get => mapBoundary;
            set => Set(ref mapBoundary, value);
        }

        #endregion

        public WikiMapViewModel() {
            mapData = MapData.For(null);
            mapBoundary = MapData.BoundaryArtistic;

            MapCommand = new RelayCommand<string>(mapName => map(mapName), s => UiEnabled);
            OpenFileCommand = new RelayCommand(openFile, () => UiEnabled && !string.IsNullOrEmpty(currentMapName));
            ShowDataCommand = new RelayCommand<MapLine>(showData, m => UiEnabled);
            CopyCommand = new RelayCommand(copy, () => UiEnabled);
            MapStyleCommand = new RelayCommand(changeMapStyle, () => UiEnabled);
            PinCommand = new RelayCommand<MouseButtonEventArgs>(args => pin(((FrameworkElement)args.Source).DataContext as CreatureViewModel));
        }

        private void pin(CreatureViewModel creatureViewModel) {
            foreach (CreatureViewModel creature in Creatures) {
                creature.Marked = false;
            }
            creatureViewModel.Marked = true;
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

        private void changeMapStyle() {
            mapStyleTopographic = !mapStyleTopographic;
            setMap(currentMapName);
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

                setMap(mapName);
                Creatures.Clear();

                foreach (MapLine mapLinesTame in MapLinesTames) {
                    Creatures.Add(new CreatureViewModel(mapLinesTame.GameObject));
                }

            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            UiEnabled = true;
        }

        private void setMap(string mapName) {
            string imgFilename = $"pack://application:,,,/assets/{(mapStyleTopographic ? $"{mapName}_Topographic.jpg" : $"{mapName}.jpg")}";
            MapImage = new ImageSourceConverter().ConvertFromString(imgFilename) as ImageSource;

            MapData = MapData.For(mapName);
            MapBoundary = mapStyleTopographic ? MapData.BoundaryTopographic : MapData.BoundaryArtistic;
        }
    }

}
