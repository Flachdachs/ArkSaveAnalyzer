using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ArkSaveAnalyzer.Infrastructure;
using GalaSoft.MvvmLight;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Maps {

    public class StructuresViewModel : ViewModelBase {
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

        #region Count

        private int count;

        public int Count {
            get => count;
            set => Set(ref count, value);
        }

        #endregion

        #region Hidden

        private bool hidden;

        public bool Hidden {
            get => hidden;
            set => Set(ref hidden, value);
        }

        #endregion

        public List<GameObject> Structures { get; }

        public string UniqueStructureNames { get; }

        #region Color

        private Color color;

        public Color Color {
            get => color;
            set => Set(ref color, value);
        }

        #endregion

        public StructuresViewModel(float lat, float lon, int count, bool hidden, List<GameObject> structures) {
            this.lat = lat;
            this.lon = lon;
            this.count = count;
            this.hidden = hidden;
            Structures = structures;

            UniqueStructureNames = string.Join(", ", structures.Select(o => o.GetNameForStructure(ArkDataService.ArkData) ?? o.ClassString).Distinct());

            Color = Colors.Crimson;
        }
    }

}
