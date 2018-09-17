using System.Collections.Generic;
using System.Windows;
using ArkSaveAnalyzer.Infrastructure;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using ArkSaveAnalyzer.Savegame;
using GalaSoft.MvvmLight;
using SavegameToolkit;

namespace ArkSaveAnalyzer {
    public class MainViewModel : ViewModelBase {
        public MainViewModel() {
            if (!IsInDesignMode) {
                if (string.IsNullOrWhiteSpace(Settings.Default.ArkSavedDirectory) || string.IsNullOrWhiteSpace(Settings.Default.WorkingDirectory)) {
                    MessageBox.Show("Ark Saved Folder and/or Working Directory not configured.");
                }
            }

            MessengerInstance.Register<ShowGameObjectMessage>(this, message => showGameObject(message.Caption, message.GameObject));
            MessengerInstance.Register<ShowGameObjectListMessage>(this, message => showGameObjectList(message.Caption, message.GameObjects, message.MapData));
            MessengerInstance.Register<ApplicationShutdownMessage>(this, m => shutdown());
        }

        private static void showGameObject(string caption, GameObject gameObject) {
            GameObjectViewModel gameObjectViewModel = new GameObjectViewModel {
                Caption = caption,
                GameObject = gameObject
            };

            GameObjectWindow gameObjectWindow = new GameObjectWindow(gameObjectViewModel);
            gameObjectWindow.Show();
        }

        private static void showGameObjectList(string caption, List<GameObject> gameObjects, MapData mapData) {
            GameObjectListViewModel gameObjectListViewModel = new GameObjectListViewModel(caption, gameObjects, mapData);

            GameObjectListWindow gameObjectListWindow = new GameObjectListWindow(gameObjectListViewModel);
            gameObjectListWindow.Show();
        }

        private static void shutdown() {
            Settings.Default.Save();
        }
    }
}
