using System.Linq;
using System.Windows.Controls;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Savegame {
    public partial class SavegameControl : UserControl {
        public SavegameControl() {
            InitializeComponent();

            Messenger.Default.Register<GotoIdMessage>(this, gotoIdMessage => gotoId(gotoIdMessage.Id));
        }

        private void gotoId(int id) {
            GameObject foundItem = ListView.ItemsSource.OfType<GameObject>().FirstOrDefault(o => o.Id == id);

            ListView.SelectedItem = foundItem;
            if (ListView.SelectedItem == null)
                return;

            ListView.ScrollIntoView(ListView.SelectedItem);
            ListViewItem item = ListView.ItemContainerGenerator.ContainerFromItem(ListView.SelectedItem) as ListViewItem;
            item?.Focus();
        }
    }
}
