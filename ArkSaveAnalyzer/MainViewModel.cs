using System.Windows;
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

        }

        private void showGameObject(string caption, GameObject gameObject) {
            GameObjectViewModel gameObjectViewModel = new GameObjectViewModel {
                    Caption = caption,
                    GameObject = gameObject
            };

            GameObjectWindow gameObjectWindow = new GameObjectWindow(gameObjectViewModel);
            gameObjectWindow.Show();
        }
    }

}
