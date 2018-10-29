using System.Collections.Generic;
using System.Linq;
using ArkTools.DataManager;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Structs;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;
using SavegameToolkitAdditions.IndexMappings;

namespace ArkTools.Data {
    public class Creature {
        private const int ColorSlotCount = 6;

        public ArkName className;

        public string type;

        public LocationData location;

        public long dinoId;

        public bool tamed;

        public int targetingTeam;

        public int owningPlayerId;

        public bool isFemale;

        public byte[] colorSetIndices = new byte[ColorSlotCount];

        public double tamedAtTime;

        public string tribeName;

        public string tamerstring;

        public string owningPlayerName;

        public string tamedName;

        public string imprinterName;

        public List<AncestorLineEntry> femaleAncestors = new List<AncestorLineEntry>();

        public List<AncestorLineEntry> maleAncestors = new List<AncestorLineEntry>();

        public int baseCharacterLevel;

        public byte[] numberOfLevelUpPointsApplied = new byte[AttributeNames.Instance.Count];

        public short extraCharacterLevel;

        public byte[] numberOfLevelUpPointsAppliedTamed = new byte[AttributeNames.Instance.Count];

        public bool allowLevelUps;

        public float experiencePoints;

        public float dinoImprintingQuality;

        public float wildRandomScale;

        public bool isWakingTame;

        public bool isSleeping;

        public float requiredTameAffinity;

        public float currentTameAffinity;

        public float tamedIneffectivenessModifier;

        public int tamedFollowTarget;

        public int tamingTeamID;

        public string tamedOnServerName;

        public string uploadedFromServerName;

        public int tamedAggressionLevel;

        public float matingProgress;

        public double lastEnterStasisTime;

        public GameObject status;

        public GameObject inventory;

        public Creature(GameObject creature, GameObjectContainer container) {
            className = creature.ClassName;
            CreatureData creatureData = ArkDataManager.GetCreature(creature.ClassString);
            type = creatureData != null ? creatureData.Name : creature.ClassString;

            location = creature.Location;

            int dinoID1 = creature.GetPropertyValue<int>("DinoID1");
            int dinoID2 = creature.GetPropertyValue<int>("DinoID2");
            dinoId = (long) dinoID1 << 32 | (dinoID2 & 0xFFFFFFFFL);

            targetingTeam = creature.GetPropertyValue<int>("TargetingTeam");
            tamed = targetingTeam < 0 || targetingTeam >= 50000;

            owningPlayerId = creature.GetPropertyValue<int>("OwningPlayerID");

            isFemale = creature.IsFemale();

            for (int i = 0; i < 6; i++) {
                colorSetIndices[i] = creature.GetPropertyValue<ArkByteValue>("ColorSetIndices", i)?.ByteValue ?? 0;
            }

            tamedAtTime = creature.GetPropertyValue<double>("TamedAtTime");

            tribeName = creature.GetPropertyValue<string>("TribeName", defaultValue: string.Empty);

            tamerstring = creature.GetPropertyValue<string>("Tamerstring", defaultValue: string.Empty);

            owningPlayerName = creature.GetPropertyValue<string>("OwningPlayerName", defaultValue: string.Empty);

            tamedName = creature.GetPropertyValue<string>("TamedName", defaultValue: string.Empty);

            imprinterName = creature.GetPropertyValue<string>("ImprinterName", defaultValue: string.Empty);

            // Not all ancestors are saved. Only those ancestor information 
            // are available which are displayed ingame in the UI.

            ArkArrayStruct ancestors = creature.GetPropertyValue<IArkArray, ArkArrayStruct>("DinoAncestors");
            if (ancestors != null) {
                // traverse female ancestor line
                foreach (IStruct value in ancestors) {
                    StructPropertyList propertyList = (StructPropertyList) value;
                    int fatherID1 = propertyList.GetPropertyValue<int>("MaleDinoID1");
                    int fatherID2 = propertyList.GetPropertyValue<int>("MaleDinoID2");
                    int motherID1 = propertyList.GetPropertyValue<int>("FemaleDinoID1");
                    int motherID2 = propertyList.GetPropertyValue<int>("FemaleDinoID2");
                    AncestorLineEntry entry = new AncestorLineEntry {
                        MaleName = propertyList.GetPropertyValue<string>("MaleName", defaultValue: string.Empty),
                        MaleId = (long) fatherID1 << 32 | (fatherID2 & 0xFFFFFFFFL),
                        FemaleName = propertyList.GetPropertyValue<string>("FemaleName", defaultValue: string.Empty),
                        FemaleId = (long) motherID1 << 32 | (motherID2 & 0xFFFFFFFFL)
                    };

                    femaleAncestors.Add(entry);
                }
            }

            ancestors = creature.GetPropertyValue<IArkArray, ArkArrayStruct>("DinoAncestorsMale");
            if (ancestors != null) {
                // traverse male ancestor line
                foreach (IStruct value in ancestors) {
                    StructPropertyList propertyList = (StructPropertyList) value;
                    int fatherID1 = propertyList.GetPropertyValue<int>("MaleDinoID1");
                    int fatherID2 = propertyList.GetPropertyValue<int>("MaleDinoID2");
                    int motherID1 = propertyList.GetPropertyValue<int>("FemaleDinoID1");
                    int motherID2 = propertyList.GetPropertyValue<int>("FemaleDinoID2");
                    AncestorLineEntry entry = new AncestorLineEntry {
                        MaleName = propertyList.GetPropertyValue<string>("MaleName", defaultValue: string.Empty),
                        MaleId = (long) fatherID1 << 32 | (fatherID2 & 0xFFFFFFFFL),
                        FemaleName = propertyList.GetPropertyValue<string>("FemaleName", defaultValue: string.Empty),
                        FemaleId = (long) motherID1 << 32 | (motherID2 & 0xFFFFFFFFL)
                    };

                    maleAncestors.Add(entry);
                }
            }

            wildRandomScale = creature.GetPropertyValue<float>("WildRandomScale", defaultValue: 1F);

            isWakingTame = creature.GetPropertyValue<bool>("bIsWakingTame");

            isSleeping = creature.GetPropertyValue<bool>("bIsSleeping");

            requiredTameAffinity = creature.GetPropertyValue<float>("RequiredTameAffinity");

            currentTameAffinity = creature.GetPropertyValue<float>("CurrentTameAffinity");

            tamedIneffectivenessModifier = creature.GetPropertyValue<float>("TameIneffectivenessModifier");

            tamedFollowTarget = creature.GetPropertyValue<ObjectReference>("TamedFollowTarget")?.ObjectId ?? -1;

            tamingTeamID = creature.GetPropertyValue<int>("TamingTeamID");

            tamedOnServerName = creature.GetPropertyValue<string>("TamedOnServerName", defaultValue: string.Empty);

            uploadedFromServerName = creature.GetPropertyValue<string>("UploadedFromServerName", defaultValue: string.Empty);

            tamedAggressionLevel = creature.GetPropertyValue<int>("TamedAggressionLevel");

            matingProgress = creature.GetPropertyValue<float>("MatingProgress");

            lastEnterStasisTime = creature.GetPropertyValue<double>("LastEnterStasisTime");

            status = creature.GetPropertyValue<ObjectReference, GameObject>("MyCharacterStatusComponent", map: reference => container[reference]);

            inventory = creature.GetPropertyValue<ObjectReference, GameObject>("MyInventoryComponent", map: reference => container[reference]);

            if (status != null && status.ClassString.StartsWith("DinoCharacterStatusComponent_")) {
                baseCharacterLevel = status.GetPropertyValue<int>("BaseCharacterLevel", defaultValue: 1);

                for (int index = 0; index < AttributeNames.Instance.Count; index++) {
                    numberOfLevelUpPointsApplied[index] =
                        status.GetPropertyValue<ArkByteValue>("NumberOfLevelUpPointsApplied", index)?.ByteValue ?? 0;
                }

                extraCharacterLevel = status.GetPropertyValue<short>("ExtraCharacterLevel");

                for (int index = 0; index < AttributeNames.Instance.Count; index++) {
                    numberOfLevelUpPointsAppliedTamed[index] = status.GetPropertyValue<ArkByteValue>("NumberOfLevelUpPointsAppliedTamed", index)?.ByteValue ?? 0;
                }

                allowLevelUps = status.GetPropertyValue<bool>("bAllowLevelUps");

                experiencePoints = status.GetPropertyValue<float>("ExperiencePoints");

                dinoImprintingQuality = status.GetPropertyValue<float>("DinoImprintingQuality");

                tamedIneffectivenessModifier = status.GetPropertyValue<float>("TamedIneffectivenessModifier", defaultValue: tamedIneffectivenessModifier);
            }
        }

        public class AncestorLineEntry {
            public string MaleName { get; set; }
            public long MaleId { get; set; }
            public string FemaleName { get; set; }
            public long FemaleId { get; set; }
        }

        public static readonly SortedDictionary<string, WriterFunction<Creature>> PROPERTIES = new SortedDictionary<string, WriterFunction<Creature>>();
        public static readonly List<WriterFunction<Creature>> PROPERTIES_LIST;

        static Creature() {
            /**
             * Creature Properties
             */
            PROPERTIES["type"] = (creature, generator, context, writeEmpty) => {
                if (context is DataCollector) {
                    generator.WriteField("type", creature.className.ToString());
                } else {
                    generator.WriteField("type", creature.type);
                }
            };
            PROPERTIES["location"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.location != null) {
                    if (creature.location == null) {
                        generator.WriteNullField("location");
                    } else {
                        generator.WriteObjectFieldStart("location");
                        generator.WriteField("x", creature.location.X);
                        generator.WriteField("y", creature.location.Y);
                        generator.WriteField("z", creature.location.Z);
                        if (context.MapData != null) {
                            generator.WriteField("lat", context.MapData.CalculateLat(creature.location.Y));
                            generator.WriteField("lon", context.MapData.CalculateLon(creature.location.X));
                        }

                        generator.WriteEndObject();
                    }
                }
            };
            PROPERTIES["myInventoryComponent"] = (creature, generator, context, writeEmpty) => {
                if (creature.inventory != null) {
                    generator.WriteField("myInventoryComponent", creature.inventory.Id);
                } else if (writeEmpty) {
                    generator.WriteNullField("myInventoryComponent");
                }
            };
            PROPERTIES["id"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.dinoId != 0) {
                    generator.WriteField("id", creature.dinoId);
                }
            };
            PROPERTIES["tamed"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamed) {
                    generator.WriteField("tamed", creature.tamed);
                }
            };
            PROPERTIES["team"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.targetingTeam != 0) {
                    generator.WriteField("team", creature.targetingTeam);
                }
            };
            PROPERTIES["playerId"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.owningPlayerId != 0) {
                    generator.WriteField("playerId", creature.owningPlayerId);
                }
            };
            PROPERTIES["female"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.isFemale) {
                    generator.WriteField("female", creature.isFemale);
                }
            };
            PROPERTIES["colorSetIndices"] = (creature, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("colorSetIndices");
                }

                for (int index = 0; index < creature.colorSetIndices.Length; index++) {
                    if (writeEmpty || creature.colorSetIndices[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("colorSetIndices");
                        }

                        generator.WriteField(index.ToString(), creature.colorSetIndices[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["femaleAncestors"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.femaleAncestors.Any()) {
                    generator.WriteArrayFieldStart("femaleAncestors");
                    foreach (AncestorLineEntry entry in creature.femaleAncestors) {
                        generator.WriteStartObject();

                        generator.WriteField("maleName", entry.MaleName);
                        generator.WriteField("maleId", entry.MaleId);
                        generator.WriteField("femaleName", entry.FemaleName);
                        generator.WriteField("femaleId", entry.FemaleId);

                        generator.WriteEndObject();
                    }

                    generator.WriteEndArray();
                }
            };
            PROPERTIES["maleAncestors"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.maleAncestors.Any()) {
                    generator.WriteArrayFieldStart("maleAncestors");
                    foreach (AncestorLineEntry entry in creature.maleAncestors) {
                        generator.WriteStartObject();

                        generator.WriteField("maleName", entry.MaleName);
                        generator.WriteField("maleId", entry.MaleId);
                        generator.WriteField("femaleName", entry.FemaleName);
                        generator.WriteField("femaleId", entry.FemaleId);

                        generator.WriteEndObject();
                    }

                    generator.WriteEndArray();
                }
            };
            PROPERTIES["tamedAtTime"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamedAtTime != 0.0) {
                    generator.WriteField("tamedAtTime", creature.tamedAtTime);
                }
            };
            PROPERTIES["tamedTime"] = (creature, generator, context, writeEmpty) => {
                if (context.Savegame != null && creature.tamedAtTime != 0.0) {
                    generator.WriteField("tamedTime", context.Savegame.GameTime - creature.tamedAtTime);
                } else if (writeEmpty) {
                    generator.WriteNullField("tamedTime");
                }
            };
            PROPERTIES["tribe"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.tribeName)) {
                    generator.WriteField("tribe", creature.tribeName);
                }
            };
            PROPERTIES["tamer"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.tamerstring)) {
                    generator.WriteField("tamer", creature.tamerstring);
                }
            };
            PROPERTIES["ownerName"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.owningPlayerName)) {
                    generator.WriteField("ownerName", creature.owningPlayerName);
                }
            };
            PROPERTIES["name"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.tamedName)) {
                    generator.WriteField("name", creature.tamedName);
                }
            };
            PROPERTIES["imprinter"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.imprinterName)) {
                    generator.WriteField("imprinter", creature.imprinterName);
                }
            };
            PROPERTIES["baseLevel"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.baseCharacterLevel != 0) {
                    generator.WriteField("baseLevel", creature.baseCharacterLevel);
                }
            };
            PROPERTIES["wildLevels"] = (creature, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("wildLevels");
                }

                for (int index = 0; index < creature.numberOfLevelUpPointsApplied.Length; index++) {
                    if (writeEmpty || creature.numberOfLevelUpPointsApplied[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("wildLevels");
                        }

                        generator.WriteField(AttributeNames.Instance[index], creature.numberOfLevelUpPointsApplied[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["extraLevel"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.extraCharacterLevel != 0) {
                    generator.WriteField("extraLevel", creature.extraCharacterLevel);
                }
            };
            PROPERTIES["tamedLevels"] = (creature, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("tamedLevels");
                }

                for (int index = 0; index < creature.numberOfLevelUpPointsAppliedTamed.Length; index++) {
                    if (writeEmpty || creature.numberOfLevelUpPointsAppliedTamed[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("tamedLevels");
                        }

                        generator.WriteField(AttributeNames.Instance[index], creature.numberOfLevelUpPointsAppliedTamed[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["allowLevelUps"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.allowLevelUps) {
                    generator.WriteField("allowLevelUps", creature.allowLevelUps);
                }
            };
            PROPERTIES["experience"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.experiencePoints != 0.0f) {
                    generator.WriteField("experience", creature.experiencePoints);
                }
            };
            PROPERTIES["imprintingQuality"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.dinoImprintingQuality != 0.0f) {
                    generator.WriteField("imprintingQuality", creature.dinoImprintingQuality);
                }
            };
            PROPERTIES["wildRandomScale"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.wildRandomScale != 1.0f) {
                    generator.WriteField("wildRandomScale", creature.wildRandomScale);
                }
            };
            PROPERTIES["isWakingTame"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.isWakingTame) {
                    generator.WriteField("isWakingTame", creature.isWakingTame);
                }
            };
            PROPERTIES["isSleeping"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.isSleeping) {
                    generator.WriteField("isSleeping", creature.isSleeping);
                }
            };
            PROPERTIES["requiredTameAffinity"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.requiredTameAffinity != 0.0f) {
                    generator.WriteField("requiredTameAffinity", creature.requiredTameAffinity);
                }
            };
            PROPERTIES["currentTameAffinity"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.currentTameAffinity != 0.0f) {
                    generator.WriteField("currentTameAffinity", creature.currentTameAffinity);
                }
            };
            PROPERTIES["tamingEffectivness"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamedIneffectivenessModifier != 1.0f) {
                    generator.WriteField("tamingEffectivness", 1.0f - creature.tamedIneffectivenessModifier);
                }
            };
            PROPERTIES["tamedFollowTarget"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamedFollowTarget != -1) {
                    generator.WriteField("tamedFollowTarget", creature.tamedFollowTarget);
                }
            };
            PROPERTIES["tamingTeamID"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamingTeamID != 0) {
                    generator.WriteField("tamingTeamID", creature.tamingTeamID);
                }
            };
            PROPERTIES["tamedOnServerName"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.tamedOnServerName)) {
                    generator.WriteField("tamedOnServerName", creature.tamedOnServerName);
                }
            };
            PROPERTIES["uploadedFromServerName"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(creature.uploadedFromServerName)) {
                    generator.WriteField("uploadedFromServerName", creature.uploadedFromServerName);
                }
            };
            PROPERTIES["tamedAggressionLevel"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.tamedAggressionLevel != 0) {
                    generator.WriteField("tamedAggressionLevel", creature.tamedAggressionLevel);
                }
            };
            PROPERTIES["matingProgress"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.matingProgress != 0.0f) {
                    generator.WriteField("matingProgress", creature.matingProgress);
                }
            };
            PROPERTIES["lastEnterStasisTime"] = (creature, generator, context, writeEmpty) => {
                if (writeEmpty || creature.lastEnterStasisTime != 0.0) {
                    generator.WriteField("lastEnterStasisTime", creature.lastEnterStasisTime);
                }
            };
            PROPERTIES_LIST = new List<WriterFunction<Creature>>(PROPERTIES.Values);
        }

        public void writeAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty) {
            foreach (WriterFunction<Creature> writer in PROPERTIES_LIST) {
                writer(this, generator, context, writeEmpty);
            }
        }

        public void writeInventory(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool inventorySummary) {
            if (inventory != null) {
                generator.WritePropertyName("inventory");
                new Inventory(inventory).writeInventory(generator, context, writeEmpty, inventorySummary);
            }
        }
    }
}
