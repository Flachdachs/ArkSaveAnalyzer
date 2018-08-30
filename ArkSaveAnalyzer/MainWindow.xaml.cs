using System.Windows;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Savegame;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;

namespace ArkSaveAnalyzer {

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

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
