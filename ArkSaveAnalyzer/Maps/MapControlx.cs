using System.Windows.Controls;

namespace ArkSaveAnalyzer.Maps {

    public partial class MapControl : UserControl {

        public MapControl() {
            InitializeComponent();

            TheListTames.Focus();
            TheListStructures.Focus();
        }

    }

}
