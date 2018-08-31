using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Wildlife {

    public class WildlifeViewModel : ViewModelBase {
        private string sortColumn;
        public Dictionary<string, ListSortDirection> SortDirections { get; } = new Dictionary<string, ListSortDirection>();

        public RelayCommand<string> ContentCommand { get; }
        public RelayCommand ShowDataCommand { get; }
        public RelayCommand<string> SortCommand { get; set; }

        public ObservableCollection<GameObject> Objects { get; } = new ObservableCollection<GameObject>();

        #region SelectedObject

        private GameObject selectedObject;

        public GameObject SelectedObject {
            get => selectedObject;
            set => Set(ref selectedObject, value);
        }

        #endregion

        #region FilterLevel

        private string filterLevel;

        public string FilterLevel {
            get => filterLevel;
            set {
                if (Set(ref filterLevel, value)) {
                    loadContent(CurrentMapName);
                }
            }
        }

        #endregion

        #region FilterText

        private string filterText;

        public string FilterText {
            get => filterText;
            set {
                if (Set(ref filterText, value)) {
                    loadContent(CurrentMapName);
                }
            }
        }

        #endregion

        #region UiEnabled

        private bool uiEnabled = true;

        public bool UiEnabled {
            get => uiEnabled;
            set => Set(ref uiEnabled, value);
        }

        #endregion

        #region CurrentMapName

        private string currentMapName;

        public string CurrentMapName {
            get => currentMapName;
            set => Set(ref currentMapName, value);
        }

        #endregion

        public WildlifeViewModel() {
            ContentCommand = new RelayCommand<string>(mapName => loadContent(mapName, false), s => UiEnabled);
            ShowDataCommand = new RelayCommand(showData, () => UiEnabled);
            SortCommand = new RelayCommand<string>(sort, s => UiEnabled);

            Messenger.Default.Register<InvalidateMapDataMessage>(this, message => {
                Application.Current.Dispatcher.Invoke(() => {
                    if (CurrentMapName == message.MapName) {
                        //Objects.Clear();
                    }
                });
            });
        }

        private void sort(string column) {
            if (column == sortColumn) {
                if (SortDirections.TryGetValue(column, out ListSortDirection dir)) {
                    if (dir == ListSortDirection.Ascending) {
                        SortDirections[column] = ListSortDirection.Descending;
                    } else if (dir == ListSortDirection.Descending) {
                        SortDirections.Remove(column);
                    }
                } else {
                    SortDirections[column] = ListSortDirection.Ascending;
                }
            } else {
                sortColumn = column;
                SortDirections[column] = ListSortDirection.Ascending;
            }

            List<GameObject> sortedObjects = Objects.ToList();
            sortedObjects.Sort((a, b) => comparison(a, b));

            Objects.Clear();
            foreach (GameObject obj in sortedObjects) {
                Objects.Add(obj);
            }
        }

        private int comparison(GameObject a, GameObject b, string column = null) {
            int cmp = 0;
            switch (column ?? sortColumn) {
                case "Id":
                    cmp = a.Id - b.Id;
                    break;
                case "Class":
                    cmp = string.Compare(a.ClassString, b.ClassString, StringComparison.InvariantCulture);
                    break;
                case "Level":
                    cmp = a.GetBaseLevel() - b.GetBaseLevel();
                    break;
            }

            if (!string.IsNullOrEmpty(column)) {
                return cmp;
            }

            if (SortDirections.TryGetValue(sortColumn, out ListSortDirection dir)) {
                if (cmp != 0) {
                    return dir == ListSortDirection.Ascending ? cmp : -cmp;
                }
            }

            foreach (KeyValuePair<string, ListSortDirection> sortDirection in SortDirections) {
                if (sortDirection.Key == sortColumn)
                    continue;
                if (SortDirections.TryGetValue(sortDirection.Key, out dir)) {
                    cmp = comparison(a, b, sortDirection.Key);
                    if (cmp != 0) {
                        return dir == ListSortDirection.Ascending ? cmp : -cmp;
                    }
                }
            }

            return 0;
        }

        private void showData() {
            if (SelectedObject != null) {
                Messenger.Default.Send(new ShowGameObjectMessage(SelectedObject));
            }
        }

        private async void loadContent(string mapName, bool getCached = true) {
            CurrentMapName = mapName;

            UiEnabled = false;

            try {
                Objects.Clear();

                ArkData arkData = await ArkDataService.GetArkData();

                GameObjectContainer objects = await SavegameService.GetGameObjects(mapName, getCached);

                IEnumerable<GameObject> filteredObjects = objects.Where(o => o.IsCreature() && o.IsWild());

                string[] excludedWildlife = Settings.Default.ExcludedWildlife
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                foreach (string exclude in excludedWildlife) {
                    filteredObjects = filteredObjects.Where(o => !Regex.IsMatch(o.GetNameForCreature(arkData), exclude, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filterText)) {
                    filteredObjects = filteredObjects.Where(o => o.GetNameForCreature(arkData).ToLowerInvariant().Contains(filterText.ToLowerInvariant()));
                }

                if (!string.IsNullOrEmpty(filterLevel)) {
                    filterLevel = filterLevel.Trim();
                    {
                        if (filterLevel.StartsWith("<") && int.TryParse(filterLevel.Substring(1).Trim(), out int level)) {
                            filteredObjects = filteredObjects.Where(o => o.GetBaseLevel() < level);
                        }
                    }

                    {
                        if (filterLevel.StartsWith(">") && int.TryParse(filterLevel.Substring(1).Trim(), out int level)) {
                            filteredObjects = filteredObjects.Where(o => o.GetBaseLevel() > level);
                        }
                    }

                    {
                        if (int.TryParse(filterLevel, out int level)) {
                            filteredObjects = filteredObjects.Where(o => o.GetBaseLevel() == level);
                        }
                    }
                }

                foreach (GameObject obj in filteredObjects) {
                    Objects.Add(obj);
                }

                sortColumn = null;
                SortDirections.Clear();
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            UiEnabled = true;
        }
    }

}
