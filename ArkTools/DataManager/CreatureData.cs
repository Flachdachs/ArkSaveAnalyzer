using System;

namespace ArkTools.DataManager {

    public sealed class CreatureData : IEquatable<CreatureData> {
        public string Name { get; }
        public string ClassName { get; }
        public string Blueprint { get; }
        public string PackagePath { get; }
        public string Category { get; }

        public CreatureData(string name, string className, string blueprint, string packagePath, string category) {
            Name = name;
            ClassName = className;
            Blueprint = blueprint;
            PackagePath = packagePath;
            Category = category;
        }

        public override string ToString() {
            return $"ArkCreature [name={Name}, id={ClassName}, blueprint={Blueprint}, category={Category}]";
        }

        #region Equality members

        public override bool Equals(object other) {
            return !(other is null) && (ReferenceEquals(this, other) || Equals(other as CreatureData));
        }

        public bool Equals(CreatureData other) {
            return !(other is null) && (ReferenceEquals(this, other) || string.Equals(ClassName, other.ClassName));
        }

        public override int GetHashCode() {
            return (ClassName != null ? ClassName.GetHashCode() : 0);
        }

        public static bool operator ==(CreatureData left, CreatureData right) {
            return Equals(left, right);
        }

        public static bool operator !=(CreatureData left, CreatureData right) {
            return !Equals(left, right);
        }

        #endregion
    }

}
