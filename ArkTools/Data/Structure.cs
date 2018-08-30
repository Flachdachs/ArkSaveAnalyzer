using System.Collections.Generic;
using ArkTools.DataManager;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Types;

namespace ArkTools.Data {

    public class Structure {
        public string id;

        public ArkName className;

        public string type;

        public LocationData location;

        public GameObject inventory;

        public bool containerActivated;

        public int owningPlayerId;

        public string owningPlayerName;

        public int[] linkedStructures;

        public int placedOnFloorStructure;

        public string ownerName;

        public string boxName;

        public string bedName;

        public float maxHealth;

        public float health;

        public int targetingTeam;

        public Structure(GameObject structure, GameObjectContainer saveFile) {
            id = structure.Names[0].ToString();

            className = structure.ClassName;
            CreatureData structureData = ArkDataManager.GetStructure(className.ToString());
            type = structureData != null ? structureData.Name : className.ToString();

            location = structure.Location;

            inventory = structure.GetPropertyValue<ObjectReference, GameObject>("MyInventoryComponent", map: reference => saveFile[reference]);

            containerActivated = structure.GetPropertyValue<bool>("bContainerActivated");

            owningPlayerId = structure.GetPropertyValue<int>("OwningPlayerID");

            owningPlayerName = structure.GetPropertyValue<string>("OwningPlayerName", defaultValue: string.Empty);

            ArkArrayObjectReference linkedStructuresReferences = structure.GetPropertyValue<ArkArrayObjectReference>("LinkedStructures");

            if (linkedStructuresReferences != null) {
                linkedStructures = new int[linkedStructuresReferences.Count];
                int index = 0;
                foreach (ObjectReference objectReference in linkedStructuresReferences) {
                    linkedStructures[index++] = objectReference.ObjectId;
                }
            }

            placedOnFloorStructure = structure.GetPropertyValue<ObjectReference>("PlacedOnFloorStructure")?.ObjectId ?? -1;

            ownerName = structure.GetPropertyValue<string>("OwnerName", defaultValue: string.Empty);
            boxName = structure.GetPropertyValue<string>("BoxName", defaultValue: string.Empty);
            bedName = structure.GetPropertyValue<string>("BedName", defaultValue: string.Empty);

            maxHealth = structure.GetPropertyValue<float>("MaxHealth");

            health = structure.GetPropertyValue<float>("Health", defaultValue: maxHealth);

            targetingTeam = structure.GetPropertyValue<int>("TargetingTeam");
        }

        public static readonly SortedDictionary<string, WriterFunction<Structure>> PROPERTIES;
        public static readonly List<WriterFunction<Structure>> PROPERTIES_LIST;

        static Structure() {
            PROPERTIES = new SortedDictionary<string, WriterFunction<Structure>>();
            
            /**
             * Structure Properties
             */
            PROPERTIES.Add("id", (structure, generator, context, writeEmpty) => {
                generator.WriteField("id", structure.id);
            });
            PROPERTIES.Add("type", (structure, generator, context, writeEmpty) => {
                if (context is DataCollector) {
                    generator.WriteField("type", structure.className.ToString());
                } else {
                    generator.WriteField("type", structure.type);
                }
            });
            PROPERTIES.Add("location", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.location != null) {
                    if (structure.location == null) {
                        generator.WriteNullField("location");
                    } else {
                        generator.WriteObjectFieldStart("location");
                        generator.WriteField("x", structure.location.X);
                        generator.WriteField("y", structure.location.Y);
                        generator.WriteField("z", structure.location.Z);
                        if (context.MapData!= null) {
                            generator.WriteField("lat", context.MapData.CalculateLat(structure.location.Y));
                            generator.WriteField("lon", context.MapData.CalculateLon(structure.location.X));
                        }

                        generator.WriteEndObject();
                    }
                }
            });
            PROPERTIES.Add("myInventoryComponent", (structure, generator, context, writeEmpty) => {
                if (structure.inventory != null) {
                    generator.WriteField("myInventoryComponent", structure.inventory.Id);
                } else if (writeEmpty) {
                    generator.WriteNullField("myInventoryComponent");
                }
            });
            PROPERTIES.Add("containerActivated", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.containerActivated) {
                    generator.WriteField("containerActivated", structure.containerActivated);
                }
            });
            PROPERTIES.Add("owningPlayerId", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.owningPlayerId != 0) {
                    generator.WriteField("owningPlayerId", structure.owningPlayerId);
                }
            });
            PROPERTIES.Add("owningPlayerName", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(structure.owningPlayerName)) {
                    generator.WriteField("owningPlayerName", structure.owningPlayerName);
                }
            });
            PROPERTIES.Add("linkedStructures", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.linkedStructures != null && structure.linkedStructures.Length > 0) {
                    if (structure.linkedStructures == null) {
                        generator.WriteNullField("linkedStructures");
                    } else {
                        generator.WriteArrayFieldStart("linkedStructures");
                        foreach (int linkedStructure in structure.linkedStructures) {
                            generator.WriteValue(linkedStructure);
                        }

                        generator.WriteEndArray();
                    }
                }
            });
            PROPERTIES.Add("placedOnFloorStructure", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.placedOnFloorStructure != -1) {
                    generator.WriteField("placedOnFloorStructure", structure.placedOnFloorStructure);
                }
            });
            PROPERTIES.Add("ownerName", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(structure.ownerName)) {
                    generator.WriteField("ownerName", structure.ownerName);
                }
            });
            PROPERTIES.Add("boxName", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(structure.boxName)) {
                    generator.WriteField("boxName", structure.boxName);
                }
            });
            PROPERTIES.Add("bedName", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(structure.bedName)) {
                    generator.WriteField("bedName", structure.bedName);
                }
            });
            PROPERTIES.Add("maxHealth", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.maxHealth != 0) {
                    generator.WriteField("maxHealth", structure.maxHealth);
                }
            });
            PROPERTIES.Add("health", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.health != structure.maxHealth) {
                    generator.WriteField("health", structure.health);
                }
            });
            PROPERTIES.Add("targetingTeam", (structure, generator, context, writeEmpty) => {
                if (writeEmpty || structure.targetingTeam != 0) {
                    generator.WriteField("targetingTeam", structure.targetingTeam);
                }
            });

            PROPERTIES_LIST = new List<WriterFunction<Structure>>(PROPERTIES.Values);
        }

    public void writeAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty) {
            foreach (WriterFunction<Structure> writer in PROPERTIES_LIST) {
                writer(this, generator, context, writeEmpty);
            }
        }

        public void writeInventory(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool inventorySummary) {
            if (this.inventory != null) {
                Inventory inventory = new Inventory(this.inventory);
                generator.WritePropertyName("inventory");
                inventory.writeInventory(generator, context, writeEmpty, inventorySummary);
            }
        }
    }

}
