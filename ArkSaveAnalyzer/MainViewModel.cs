using System.Collections.Generic;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using ArkSaveAnalyzer.Savegame;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;

namespace ArkSaveAnalyzer {
    public class MainViewModel : ViewModelBase {
        public MainViewModel() {
            if (string.IsNullOrWhiteSpace(Settings.Default.ArkSavedDirectory) || string.IsNullOrWhiteSpace(Settings.Default.WorkingDirectory)) {
                MessageBox.Show("Ark Saved Folder and/or Working Directory not configured.");
            }

            Messenger.Default.Register<ShowGameObjectMessage>(this, showGameObjectMessage => showGameObject(showGameObjectMessage.Caption, showGameObjectMessage.GameObject));
            Messenger.Default.Register<ShowGameObjectListMessage>(this,
                showGameObjectListMessage => showGameObjectList(showGameObjectListMessage.Caption, showGameObjectListMessage.GameObjects, showGameObjectListMessage.MapData));
        }

        private void showGameObject(string caption, GameObject gameObject) {
            GameObjectViewModel gameObjectViewModel = new GameObjectViewModel {
                Caption = caption,
                GameObject = gameObject
            };

            GameObjectWindow gameObjectWindow = new GameObjectWindow(gameObjectViewModel);
            gameObjectWindow.Show();
        }

        private void showGameObjectList(string caption, List<GameObject> gameObjects, MapData mapData) {
            GameObjectListViewModel gameObjectListViewModel = new GameObjectListViewModel(caption, gameObjects, mapData);

            GameObjectListWindow gameObjectListWindow = new GameObjectListWindow(gameObjectListViewModel);
            gameObjectListWindow.Show();
        }
    }
}
