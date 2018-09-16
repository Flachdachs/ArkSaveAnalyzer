using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Maps {

    public class MapViewModel : ViewModelBase {
        private string currentMapName;
        private bool mapStyleTopographic;
        private string fileName;

        private string sortColumn;
        private readonly Dictionary<string, ListSortDirection> sortDirections = new Dictionary<string, ListSortDirection>();

        public RelayCommand<string> MapCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand<GameObject> ShowDataTameCommand { get; }
        public RelayCommand<StructuresViewModel> ShowDataStructuresCommand { get; }
        public RelayCommand MapStyleCommand { get; }
        public RelayCommand<string> SortCommand { get; set; }

        public ObservableCollection<GameObject> Tames { get; } = new ObservableCollection<GameObject>();
        public ObservableCollection<CreatureViewModel> Creatures { get; } = new ObservableCollection<CreatureViewModel>();
        public ObservableCollection<StructuresViewModel> Structures { get; } = new ObservableCollection<StructuresViewModel>();

        #region UiEnabled

        private bool uiEnabled = true;

        public bool UiEnabled {
            get => uiEnabled;
            set => Set(ref uiEnabled, value, true);
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

        #region SelectedTame

        private GameObject selectedTame;

        public GameObject SelectedTame {
            get => selectedTame;
            set => Set(ref selectedTame, value);
        }

        #endregion

        #region SelectedStructures

        private StructuresViewModel selectedStructures;

        public StructuresViewModel SelectedStructures {
            get => selectedStructures;
            set => Set(ref selectedStructures, value);
        }

        #endregion

        public MapViewModel() {
            mapData = MapData.For(null);
            mapBoundary = MapData.BoundaryArtistic;

            MapCommand = new RelayCommand<string>(mapName => map(mapName), s => UiEnabled);
            OpenFileCommand = new RelayCommand(openFile, () => UiEnabled && !string.IsNullOrEmpty(currentMapName));
            ShowDataTameCommand = new RelayCommand<GameObject>(showDataTame, m => UiEnabled);
            ShowDataStructuresCommand = new RelayCommand<StructuresViewModel>(showDataStructures, m => UiEnabled);
            MapStyleCommand = new RelayCommand(changeMapStyle, () => UiEnabled);
            SortCommand = new RelayCommand<string>(changeSort, s => UiEnabled);
        }

        private void changeSort(string column) {
            if (column == sortColumn) {
                if (sortDirections.TryGetValue(column, out ListSortDirection dir)) {
                    if (dir == ListSortDirection.Ascending) {
                        sortDirections[column] = ListSortDirection.Descending;
                    } else if (dir == ListSortDirection.Descending) {
                        sortDirections.Remove(column);
                    }
                } else {
                    sortDirections[column] = ListSortDirection.Ascending;
                }
            } else {
                sortColumn = column;
                sortDirections[column] = ListSortDirection.Ascending;
            }

            List<GameObject> sortedObjects = Tames.ToList();
            sortedObjects.Sort((a, b) => comparison(a, b));

            Tames.Clear();
            foreach (GameObject gameObject in sortedObjects) {
                Tames.Add(gameObject);
            }
        }

        private int comparison(GameObject a, GameObject b, string column = null) {
            ArkData arkData = ArkDataService.ArkData;
            int cmp = 0;
            switch (column ?? sortColumn) {
                case "Location":
                    cmp = (int)(Math.Round(a.Location.Y, 2) - Math.Round(b.Location.Y, 2));
                    if (cmp == 0) {
                        cmp = (int)(Math.Round(a.Location.X, 2) - Math.Round(b.Location.X, 2));
                    }
                    break;
                case "Id":
                    cmp = a.Id - b.Id;
                    break;
                case "Creature":
                    cmp = string.Compare(a.GetNameForCreature(arkData), b.GetNameForCreature(arkData), StringComparison.InvariantCulture);
                    break;
                case "Name":
                    cmp = string.Compare(a.GetPropertyValue<string>("TamedName"), b.GetPropertyValue<string>("TamedName"), StringComparison.InvariantCulture);
                    break;
                case "Level":
                    cmp = a.GetBaseLevel() - b.GetBaseLevel();
                    break;
                case "Sex":
                    cmp = (a.IsFemale() ? 1 : 0) - (b.IsFemale() ? 1 : 0);
                    break;
                case "Tamer":
                    cmp = string.Compare(a.GetPropertyValue<string>("TamerString"), b.GetPropertyValue<string>("TamerString"), StringComparison.InvariantCulture);
                    if (cmp == 0) {
                        cmp = string.Compare(a.GetPropertyValue<string>("ImprinterName"), b.GetPropertyValue<string>("ImprinterName"), StringComparison.InvariantCulture);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(column)) {
                return cmp;
            }

            if (sortDirections.TryGetValue(sortColumn, out ListSortDirection dir)) {
                if (cmp != 0) {
                    return dir == ListSortDirection.Ascending ? cmp : -cmp;
                }
            }

            foreach (KeyValuePair<string, ListSortDirection> sortDirection in sortDirections) {
                if (sortDirection.Key == sortColumn)
                    continue;
                if (sortDirections.TryGetValue(sortDirection.Key, out dir)) {
                    cmp = comparison(a, b, sortDirection.Key);
                    if (cmp != 0) {
                        return dir == ListSortDirection.Ascending ? cmp : -cmp;
                    }
                }
            }

            return 0;
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

        private void changeMapStyle() {
            mapStyleTopographic = !mapStyleTopographic;
            setMap(currentMapName);
        }

        private void showDataTame(GameObject gameObject) {
            if (gameObject != null) {
                Messenger.Default.Send(new ShowGameObjectMessage(gameObject));
            }
        }

        private void showDataStructures(StructuresViewModel structuresViewModel) {
            if (structuresViewModel.Structures != null) {
                Messenger.Default.Send(new ShowGameObjectListMessage(structuresViewModel.Structures, MapData,
                        string.Format(CultureInfo.InvariantCulture, "{0:0.#} / {1:0.#}", structuresViewModel.Lat, structuresViewModel.Lon)));
            }
        }

        private async void map(string mapName, string filename = null) {
            UiEnabled = false;

            currentMapName = mapName;

            try {
                Tames.Clear();
                foreach (GameObject gameObject in await MapService.ReadTames(mapName, filename)) {
                    Tames.Add(gameObject);
                }

                SelectedTame = Tames.FirstOrDefault();

                setMap(mapName);
                Creatures.Clear();

                foreach (GameObject gameObject in Tames) {
                    Creatures.Add(new CreatureViewModel(gameObject));
                }

                Structures.Clear();

                foreach (StructuresViewModel structuresViewModel in await MapService.ReadStructures(mapName, filename)) {
                    GameObject gameObject = structuresViewModel.Structures.FirstOrDefault();
                    if (gameObject == null)
                        continue;
                    Structures.Add(structuresViewModel);
                }

                SelectedStructures = Structures.FirstOrDefault();
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            UiEnabled = true;
        }

        private void setMap(string mapName) {
            if (string.IsNullOrEmpty(mapName))
                return;

            string imgFilename = $"pack://application:,,,/assets/{(mapStyleTopographic ? $"{mapName}_Topographic.jpg" : $"{mapName}.jpg")}";
            MapImage = new ImageSourceConverter().ConvertFromString(imgFilename) as ImageSource;

            MapData = MapData.For(mapName);
            MapBoundary = mapStyleTopographic ? MapData.BoundaryTopographic : MapData.BoundaryArtistic;
        }
    }

}
