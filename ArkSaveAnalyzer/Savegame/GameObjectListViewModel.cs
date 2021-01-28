using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Savegame {

    public class GameObjectListViewModel : ViewModelBase {
        #region Caption

        private string caption;

        public string Caption {
            get => caption;
            set => Set(ref caption, value);
        }

        #endregion

        #region SelectedGameObject

        private GameObject selectedGameObject;

        public GameObject SelectedGameObject {
            get => selectedGameObject;
            set => Set(ref selectedGameObject, value);
        }

        #endregion

        #region MapData

        private MapData mapData;

        public MapData MapData {
            get => mapData;
            set => Set(ref mapData, value);
        }

        #endregion

        public ObservableCollection<GameObject> GameObjects { get; } = new ObservableCollection<GameObject>();

        public RelayCommand ShowDataCommand { get; }
        public RelayCommand<Window> CloseCommand { get; }

        private string sortColumn;
        private readonly Dictionary<string, ListSortDirection> sortDirections = new Dictionary<string, ListSortDirection>();
        public RelayCommand<string> SortCommand { get; set; }

        public GameObjectListViewModel(string caption, IEnumerable<GameObject> gameObjects, MapData mapData) {
            this.mapData = mapData;
            this.caption = caption ?? string.Empty;

            CloseCommand = new RelayCommand<Window>(close);
            ShowDataCommand = new RelayCommand(showData);
            SortCommand = new RelayCommand<string>(changeSort);

            foreach (GameObject gameObject in gameObjects.Where(o => o != null)) {
                GameObjects.Add(gameObject);
            }

            SelectedGameObject = GameObjects.FirstOrDefault();
        }

        private void showData() {
            if (SelectedGameObject != null) {
                Messenger.Default.Send(new ShowGameObjectMessage(SelectedGameObject));
            }
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

            List<GameObject> sortedObjects = GameObjects.ToList();
            sortedObjects.Sort((a, b) => comparison(a, b));

            GameObjects.Clear();
            foreach (GameObject gameObject in sortedObjects) {
                GameObjects.Add(gameObject);
            }
        }

        private int comparison(GameObject a, GameObject b, string column = null) {
            int cmp = 0;
            switch (column ?? sortColumn) {
                case "Id":
                    cmp = a.Id - b.Id;
                    break;
                case "Location":
                    cmp = (int)(Math.Round(a.Location.Y, 2) - Math.Round(b.Location.Y, 2));
                    if (cmp == 0) {
                        cmp = (int)(Math.Round(a.Location.X, 2) - Math.Round(b.Location.X, 2));
                    }

                    break;
                case "Name":
                    string aName = a.GetNameForStructure(ArkDataService.ArkData) ?? a?.ClassString;
                    string bName = b.GetNameForStructure(ArkDataService.ArkData) ?? b?.ClassString;
                    cmp = string.Compare(aName, bName, StringComparison.InvariantCulture);
                    break;
                case "Class":
                    cmp = string.Compare(a.ClassString, b.ClassString, StringComparison.InvariantCulture);
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

        private void close(Window window) {
            window.Close();
        }
    }

}
