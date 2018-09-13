using System.Windows;

namespace ArkSaveAnalyzer.Savegame {
    public partial class GameObjectListWindow : Window {
        public GameObjectListWindow(GameObjectListViewModel gameObjectListViewModel) {
            InitializeComponent();
            DataContext = gameObjectListViewModel;
            TheList.Focus();
        }
    }
}
