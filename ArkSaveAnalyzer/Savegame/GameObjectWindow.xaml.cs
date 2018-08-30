using System.Windows;

namespace ArkSaveAnalyzer.Savegame {
    public partial class GameObjectWindow : Window {
        public GameObjectWindow(GameObjectViewModel gameObjectViewModel) {
            InitializeComponent();
            DataContext = gameObjectViewModel;
        }
    }
}
