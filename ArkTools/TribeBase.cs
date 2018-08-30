using System;
using System.Collections.Generic;
using ArkTools.Data;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkTools {

    public sealed class TribeBase : IEquatable<TribeBase> {
        public string Name { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public float Size { get; }

        public List<GameObject> Structures { get; } = new List<GameObject>();

        public List<GameObject> Creatures { get; } = new List<GameObject>();

        public List<Item> Items { get; } = new List<Item>();

        public List<Item> Blueprints { get; } = new List<Item>();

        public List<DroppedItem> DroppedItems { get; } = new List<DroppedItem>();

        public TribeBase(string name, float x, float y, float z, float size) {
            Name = name;
            X = x;
            Y = y;
            Z = z;
            Size = size;
        }

        public bool InsideBounds(LocationData location) {
            float diffX = X - location.X;
            float diffY = Y - location.Y;
            float diffZ = Z - location.Z;

            float distance = (float)Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2) + Math.Pow(diffZ, 2));

            return distance < Size;
        }

        public override string ToString() {
            return "TribeBase [name=" + Name + ", x=" + X + ", y=" + Y + ", z=" + Z + ", size=" + Size + "]";
        }

        #region Equality members

        public override int GetHashCode() {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public override bool Equals(object other) {
            return !(other is null) && (ReferenceEquals(this, other) || Equals(other as TribeBase));
        }

        public bool Equals(TribeBase other) {
            return !(other is null) && (ReferenceEquals(this, other) || string.Equals(Name, other.Name));
        }

        public static bool operator ==(TribeBase left, TribeBase right) {
            return Equals(left, right);
        }

        public static bool operator !=(TribeBase left, TribeBase right) {
            return !Equals(left, right);
        }

        #endregion
    }

}
