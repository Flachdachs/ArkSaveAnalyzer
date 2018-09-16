using System.Linq;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace SavegameToolkitAdditions {
    public static class GameObjectExtensions {
        public static bool IsCreature(this GameObject gameObject) {
            return gameObject.HasAnyProperty("bServerInitializedDino");
        }

        public static bool IsDroppedItem(this GameObject gameObject) {
            return gameObject.HasAnyProperty("MyItem");
        }

        public static bool IsInventory(this GameObject gameObject) {
            return gameObject.HasAnyProperty("bInitializedMe");
        }

        public static bool IsPlayer(this GameObject gameObject) {
            return gameObject.HasAnyProperty("LinkedPlayerDataID");
        }

        public static bool IsStatusComponent(this GameObject gameObject) {
            return gameObject.HasAnyProperty("bServerFirstInitialized");
        }

        public static bool IsTamed(this GameObject gameObject) {
            TeamType teamType = TeamTypes.ForTeam(gameObject.GetPropertyValue<int>("TargetingTeam"));
            return teamType.IsTamed();
        }

        public static bool IsWeapon(this GameObject gameObject) {
            return gameObject.HasAnyProperty("AssociatedPrimalItem") || gameObject.HasAnyProperty("MyPawn");
        }

        public static bool IsWild(this GameObject gameObject) {
            TeamType teamType = TeamTypes.ForTeam(gameObject.GetPropertyValue<int>("TargetingTeam"));
            return !teamType.IsTamed();
        }

        public static bool IsDeathItemCache(this GameObject gameObject) {
            return gameObject.ClassString == "DeathItemCache_C";
        }

        public static bool IsFemale(this GameObject gameObject) {
            return gameObject.GetPropertyValue<bool>("bIsFemale");
        }

        public static GameObject CharacterStatusComponent(this GameObject gameObject) {
            return gameObject.Components.FirstOrDefault(component => component.Key.Name.StartsWith("DinoCharacterStatus_")).Value;
        }

        public static int GetBaseLevel(this GameObject gameObject) {
            return gameObject.CharacterStatusComponent()?.GetPropertyValue<int>("BaseCharacterLevel") ?? 0;
        }

        public static int GetBaseLevel(this GameObject gameObject, GameObjectContainer saveFile) {
            ObjectReference objectReference = gameObject.GetPropertyValue<ObjectReference>("MyCharacterStatusComponent");
            GameObject statusComponent = objectReference != null ? saveFile[objectReference] : null;

            return statusComponent?.GetPropertyValue<int>("BaseCharacterLevel") ?? 0;
        }

        public static int GetFullLevel(this GameObject gameObject) {
            GameObject statusComponent = gameObject.CharacterStatusComponent();
            return getFullLevel(statusComponent);
        }

        public static int GetFullLevel(this GameObject gameObject, GameObjectContainer saveFile) {
            ObjectReference objectReference = gameObject.GetPropertyValue<ObjectReference>("MyCharacterStatusComponent");

            GameObject statusComponent = objectReference != null ? saveFile[objectReference] : null;

            return getFullLevel(statusComponent);
        }

        private static int getFullLevel(GameObject statusComponent) {
            if (statusComponent == null) {
                return 1;
            }

            int baseLevel = statusComponent.GetPropertyValue<int>("BaseCharacterLevel", defaultValue: 1);
            short extraLevel = statusComponent.GetPropertyValue<short>("ExtraCharacterLevel");
            return baseLevel + extraLevel;
        }

        public static string GetNameForCreature(this GameObject gameObject, ArkData arkData) {
            return arkData.GetCreatureForClass(gameObject.ClassString)?.Name;
        }

        public static string GetNameForStructure(this GameObject gameObject, ArkData arkData) {
            return arkData.GetStructureForClass(gameObject.ClassString)?.Name;
        }

        public static string GetNameForItem(this GameObject gameObject, ArkData arkData) {
            return arkData.GetItemForClass(gameObject.ClassString)?.Name;
        }
    }
}
