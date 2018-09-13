using System.Windows.Controls;

namespace ArkSaveAnalyzer.Maps {
    public partial class MapStructuresListControl : UserControl {
        public MapStructuresListControl() {
            InitializeComponent();

            ListViewStructures.Focus();
        }
    }
}
