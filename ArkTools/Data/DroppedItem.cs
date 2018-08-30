using System.Collections.Generic;
using ArkTools.DataManager;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkTools.Data {
    public class DroppedItem {

        public ArkName className;

        public string type;

        public LocationData location;

        public GameObject myItem;

        public string droppedByName;

        public int targetingTeam;

        public double originalCreationTime;

        public float initialLifeSpan;

        public DroppedItem(GameObject droppedItem, GameObjectContainer container) {
            className = droppedItem.ClassName;
            CreatureData structureData = ArkDataManager.GetStructure(droppedItem.ClassString);
            type = structureData != null ? structureData.Name : droppedItem.ClassString;

            location = droppedItem.Location;

            myItem = droppedItem.GetPropertyValue<ObjectReference, GameObject>("MyItem", map: reference => container[reference]);
            droppedByName = droppedItem.GetPropertyValue<string>("DroppedByName", defaultValue: string.Empty);
            targetingTeam = droppedItem.GetPropertyValue<int>("TargetingTeam");
            originalCreationTime = droppedItem.GetPropertyValue<double>("OriginalCreationTime");
            initialLifeSpan = droppedItem.GetPropertyValue<float>("InitialLifeSpan", defaultValue: float.PositiveInfinity);
        }

        public static readonly SortedDictionary<string, WriterFunction<DroppedItem>> PROPERTIES;
        public static readonly List<WriterFunction<DroppedItem>> PROPERTIES_LIST;

        static DroppedItem() {
            PROPERTIES = new SortedDictionary<string, WriterFunction<DroppedItem>>();
            /**
             * Creature Properties
             */
            PROPERTIES["type"]= (droppedItem, generator, context, writeEmpty) => {
                if (context is DataCollector) {
                    generator.WriteField("type", droppedItem.className.ToString());
                }
                else {
                    generator.WriteField("type", droppedItem.type);
                }
            };
            PROPERTIES["location"]= (droppedItem, generator, context, writeEmpty) => {
                if (writeEmpty || droppedItem.location != null) {
                    if (droppedItem.location == null) {
                        generator.WriteNullField("location");
                    }
                    else {
                        generator.WriteObjectFieldStart("location");
                        generator.WriteField("x", droppedItem.location.X);
                        generator.WriteField("y", droppedItem.location.Y);
                        generator.WriteField("z", droppedItem.location.Z);
                        if (context.MapData!= null) {
                            generator.WriteField("lat", context.MapData.CalculateLat(droppedItem.location.Y));
                            generator.WriteField("lon", context.MapData.CalculateLon(droppedItem.location.X));
                        }

                        generator.WriteEndObject();
                    }
                }
            };
            PROPERTIES["myItem"]= (droppedItem, generator, context, writeEmpty) => {
                if (droppedItem.myItem != null) {
                    generator.WriteField("myItem", droppedItem.myItem.Id);
                }
                else
                    if (writeEmpty) {
                        generator.WriteNullField("myItem");
                    }
            };
            PROPERTIES["droppedByName"]= (droppedItem, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(droppedItem.droppedByName)) {
                    generator.WriteField("droppedByName", droppedItem.droppedByName);
                }
            };
            PROPERTIES["targetingTeam"]= (droppedItem, generator, context, writeEmpty) => {
                if (writeEmpty || droppedItem.targetingTeam != 0) {
                    generator.WriteField("targetingTeam", droppedItem.targetingTeam);
                }
            };
            PROPERTIES["originalCreationTime"]= (droppedItem, generator, context, writeEmpty) => {
                if (writeEmpty || droppedItem.originalCreationTime != 0.0) {
                    generator.WriteField("originalCreationTime", droppedItem.originalCreationTime);
                }
            };
            PROPERTIES["initialLifeSpan"]= (droppedItem, generator, context, writeEmpty) => {
                if (writeEmpty || droppedItem.initialLifeSpan != float.PositiveInfinity) {
                    generator.WriteField("initialLifeSpan", droppedItem.initialLifeSpan);
                }
            };
            PROPERTIES["lifeSpanLeft"]= (droppedItem, generator, context, writeEmpty) => {
                if (context.Savegame!= null && droppedItem.initialLifeSpan != float.PositiveInfinity) {
                    generator.WriteField("lifeSpanLeft", droppedItem.initialLifeSpan - (context.Savegame.GameTime- droppedItem.originalCreationTime));
                }
                else
                    if (writeEmpty) {
                        if (!float.IsPositiveInfinity(droppedItem.initialLifeSpan)) {
                            generator.WriteField("lifeSpanLeft", float.NaN);
                        }
                        else {
                            generator.WriteField("lifeSpanLeft", float.PositiveInfinity);
                        }
                    }
            };
            PROPERTIES_LIST = new List<WriterFunction<DroppedItem>>(PROPERTIES.Values);
        }

        public void writeAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty) {
            foreach (WriterFunction<DroppedItem> writer in PROPERTIES_LIST) {
                writer(this, generator, context, writeEmpty);
            }
        }

        public void writeInventory(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool inventorySummary) {
            if (myItem != null) {
                Item item = new Item(myItem);

                generator.WritePropertyName("item");
                if (inventorySummary) {
                    generator.WriteStartObject();

                    generator.WritePropertyName("name");
                    generator.WriteValue(item.type);
                    generator.WritePropertyName("count");
                    generator.WriteValue(item.quantity);

                    generator.WriteEndObject();
                }
                else {
                    generator.WriteStartObject();

                    item.writeAllProperties(generator, context, writeEmpty);

                    generator.WriteEndObject();
                }
            }
        }

    }
}
