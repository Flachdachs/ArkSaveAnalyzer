using System.Windows.Media;
using GalaSoft.MvvmLight;
using SavegameToolkit;

namespace ArkSaveAnalyzer.WikiMap {

    public class CreatureViewModel : ViewModelBase {
        #region Creature

        private GameObject creature;

        public GameObject Creature {
            get => creature;
            set => Set(ref creature, value);
        }

        #endregion

        #region Color

        private Color color;

        public Color Color {
            get => color;
            set => Set(ref color, value);
        }

        #endregion

        #region Marked

        private bool marked;

        public bool Marked {
            get => marked;
            set => Set(ref marked, value);
        }

        #endregion

        public CreatureViewModel(GameObject creature) {
            this.creature = creature;
            Color = Colors.Crimson;
        }
    }

}
