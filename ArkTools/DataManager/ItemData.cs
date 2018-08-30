using System;

namespace ArkTools.DataManager {

    public sealed class ItemData : IEquatable<ItemData> {
        public string Name { get; }
        public string Blueprint { get; }
        public string BlueprintGeneratedClass { get; }
        public string Category { get; }

        public ItemData(string name, string blueprint, string blueprintGeneratedClass, string category) {
            Name = name;
            Blueprint = blueprint;
            BlueprintGeneratedClass = blueprintGeneratedClass;
            Category = category;
        }

        public override string ToString() {
            return $"ArkItem [name={Name}, blueprint={Blueprint}, blueprintGeneratedClass={BlueprintGeneratedClass}, category={Category}]";
        }

        #region Equality members

        public bool Equals(ItemData other) {
            return !(other is null) && (ReferenceEquals(this, other) || string.Equals(Blueprint, other.Blueprint));
        }

        public override bool Equals(object other) {
            return !(other is null) && (ReferenceEquals(this, other) || Equals(other as ItemData));
        }

        public override int GetHashCode() {
            return (Blueprint != null ? Blueprint.GetHashCode() : 0);
        }

        public static bool operator ==(ItemData left, ItemData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ItemData left, ItemData right) {
            return !Equals(left, right);
        }

        #endregion
    }

}