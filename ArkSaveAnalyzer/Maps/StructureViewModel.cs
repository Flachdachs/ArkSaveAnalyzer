using System.Windows.Media;
using ArkSaveAnalyzer.Infrastructure;
using GalaSoft.MvvmLight;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Maps {
    public class StructureViewModel : ViewModelBase {
        #region Lat

        private float lat;

        public float Lat {
            get => lat;
            set => Set(ref lat, value);
        }

        #endregion

        #region Lon

        private float lon;

        public float Lon {
            get => lon;
            set => Set(ref lon, value);
        }

        #endregion

        #region Hidden

        private bool hidden;

        public bool Hidden {
            get => hidden;
            set => Set(ref hidden, value);
        }

        #endregion

        public GameObject Structure { get; }

        #region StructureName

        private string structureName;

        public string StructureName {
            get => structureName;
            set => Set(ref structureName, value);
        }

        #endregion

        #region Color

        private Color color;

        public Color Color {
            get => color;
            set => Set(ref color, value);
        }

        #endregion

        public StructureViewModel(float lat, float lon, bool hidden, GameObject structure) {
            this.lat = lat;
            this.lon = lon;
            this.hidden = hidden;
            Structure = structure;

            StructureName = structure.GetNameForStructure(ArkDataService.ArkData) ?? structure.ClassString;

            Color = Colors.Crimson;
        }
    }
}
