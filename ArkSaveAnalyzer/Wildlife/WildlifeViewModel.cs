using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Wildlife {
    public class WildlifeViewModel : ViewModelBase {
        private string sortColumn;
        private readonly Dictionary<string, ListSortDirection> sortDirections = new Dictionary<string, ListSortDirection>();

        public RelayCommand<string> ContentCommand { get; }
        public RelayCommand ShowDataCommand { get; }
        public RelayCommand<string> SortCommand { get; set; }
        public RelayCommand ExcludeCommand { get; }
        public RelayCommand WishListCommand { get; }

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
                    loadAndSortCurrentMap();
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
                    loadAndSortCurrentMap();
                }
            }
        }

        #endregion

        #region ApplyWishList

        private bool applyWishList;

        public bool ApplyWishList {
            get => applyWishList;
            set {
                if (Set(ref applyWishList, value)) {
                    loadAndSortCurrentMap();
                }
            }
        }

        #endregion

        #region AutoReload

        private bool autoReload;

        public bool AutoReload {
            get => autoReload;
            set {
                if (Set(ref autoReload, value)) {
                    Messenger.Default.Send(new FileSystemWatchMessage(value ? CurrentMapName : null));
                }
            }
        }

        #endregion

        #region Untameable

        private bool untameable;

        public bool Untameable {
            get => untameable;
            set {
                if (Set(ref untameable, value)) {
                    loadAndSortCurrentMap();
                }
            }
        }

        #endregion

        #region UiEnabled

        private bool uiEnabled = true;

        public bool UiEnabled {
            get => uiEnabled;
            set {
                if (Set(ref uiEnabled, value))
                    CommandManager.InvalidateRequerySuggested();
            }
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
            ContentCommand = new RelayCommand<string>(mapName => {
                loadContent(mapName, false);
                Messenger.Default.Send(new FileSystemWatchMessage(AutoReload ? mapName : null));
            }, s => UiEnabled);
            ShowDataCommand = new RelayCommand(showData, () => UiEnabled);
            SortCommand = new RelayCommand<string>(changeSort, s => UiEnabled);
            ExcludeCommand = new RelayCommand(exclude, () => UiEnabled);
            WishListCommand = new RelayCommand(addToWishList, () => UiEnabled);

            Messenger.Default.Register<InvalidateMapDataMessage>(this, message => {
                Application.Current.Dispatcher.Invoke(() => {
                    if (CurrentMapName == message.MapName) {
                        //Objects.Clear();
                    }
                });
            });

            Messenger.Default.Register<FileSystemWatchChangedMessage>(this, message => {
                if (AutoReload && message.MapName == CurrentMapName) {
                    loadContent(message.MapName, false);
                }
            });
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
                case "Name":
                    ArkData arkData = ArkDataService.ArkData;
                    cmp = string.Compare(a.GetNameForCreature(arkData), b.GetNameForCreature(arkData), StringComparison.InvariantCulture);
                    break;
                case "Level":
                    cmp = a.GetBaseLevel() - b.GetBaseLevel();
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

        private void showData() {
            if (SelectedObject != null) {
                Messenger.Default.Send(new ShowGameObjectMessage(SelectedObject));
            }
        }

        private void exclude() {
            if (SelectedObject == null)
                return;
            Messenger.Default.Send(new WildlifeExcludeMessage(SelectedObject.GetNameForCreature(ArkDataService.ArkData)));
            loadAndSortCurrentMap();
        }

        private void addToWishList() {
            if (SelectedObject == null)
                return;
            Messenger.Default.Send(new WildlifeWishListMessage(SelectedObject.GetNameForCreature(ArkDataService.ArkData)));
        }

        private void loadAndSortCurrentMap() {
            if (string.IsNullOrEmpty(CurrentMapName)) {
                return;
            }

            loadContent(CurrentMapName);
        }

        private async void loadContent(string mapName, bool getCached = true) {
            CurrentMapName = mapName;

            UiEnabled = false;

            ArkData arkData = ArkDataService.ArkData;

            try {
                Objects.Clear();

                GameObjectContainer objects = await SavegameService.GetGameObjects(mapName, getCached);

                IEnumerable<GameObject> filteredObjects = objects.Where(o => o.IsCreature() && o.IsWild());

                string[] excludedWildlife = Settings.Default.ExcludedWildlife
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                foreach (string exclude in excludedWildlife) {
                    filteredObjects = filteredObjects.Where(o => !Regex.IsMatch(o.GetNameForCreature(arkData), exclude, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filterText)) {
                    filteredObjects = filteredObjects.Where(o => o.GetNameForCreature(arkData).ToLowerInvariant().Contains(filterText.ToLowerInvariant()));
                }

                if (!Untameable) {
                    filteredObjects = filteredObjects.Where(o => !o.GetPropertyValue<bool>("bForceDisablingTaming"));
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

                if (ApplyWishList) {
                    string[] wishListWildlife = Settings.Default.WishListWildlife
                        .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                    filteredObjects = filteredObjects.Where(o => wishListWildlife.Any(wish => Regex.IsMatch(o.GetNameForCreature(arkData), wish, RegexOptions.IgnoreCase)));
                }

                List<GameObject> objectsList = filteredObjects.ToList();
                if (!string.IsNullOrEmpty(sortColumn)) {
                    objectsList.Sort((a, b) => comparison(a, b));
                }

                Objects.Clear();
                foreach (GameObject obj in objectsList) {
                    Objects.Add(obj);
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            UiEnabled = true;
        }
    }
}
