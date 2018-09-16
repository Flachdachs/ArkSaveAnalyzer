using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;

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

        public GameObjectListViewModel(string caption, IEnumerable<GameObject> gameObjects, MapData mapData) {
            this.mapData = mapData;
            this.caption = caption ?? string.Empty;

            CloseCommand = new RelayCommand<Window>(close);
            ShowDataCommand = new RelayCommand(showData);

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

        private void close(Window window) {
            window.Close();
        }
    }

}
