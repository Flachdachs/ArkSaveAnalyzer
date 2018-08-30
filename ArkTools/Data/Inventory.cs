using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Types;

namespace ArkTools.Data {

    public class Inventory {

        public List<int> inventoryItems = new List<int>();

        public List<int> equippedItems = new List<int>();

        public List<int> itemSlots = new List<int>();

        public double lastInventoryRefreshTime;

        public Inventory(GameObject inventory) {

            List<ObjectReference> inventoryItemReferences = inventory.GetPropertyValue<IArkArray, ArkArrayObjectReference>("InventoryItems");
            if (inventoryItemReferences != null) {
                foreach (ObjectReference inventoryItem in inventoryItemReferences) {
                    inventoryItems.Add(inventoryItem.ObjectId);
                }
            }

            List<ObjectReference> equippedItemReferences = inventory.GetPropertyValue<IArkArray, ArkArrayObjectReference>("EquippedItems");
            if (equippedItemReferences != null) {
                foreach (ObjectReference equippedItem in equippedItemReferences) {
                    equippedItems.Add(equippedItem.ObjectId);
                }
            }

            List<ObjectReference> itemSlotReferences = inventory.GetPropertyValue<IArkArray, ArkArrayObjectReference>("ItemSlots");
            if (itemSlotReferences != null) {
                foreach (ObjectReference itemSlot in itemSlotReferences) {
                    itemSlots.Add(itemSlot.ObjectId);
                }
            }

            lastInventoryRefreshTime = inventory.GetPropertyValue<double>("LastInventoryRefreshTime");
        }

        public static readonly SortedDictionary<string, WriterFunction<Inventory>> PROPERTIES;

        static Inventory() {
            PROPERTIES = new SortedDictionary<string, WriterFunction<Inventory>>();
            /**
             * Inventory Properties
             */
            PROPERTIES["inventoryItems"] = (inventory, generator, context, writeEmpty) => {
                if (writeEmpty || inventory.inventoryItems.Any()) {
                    generator.WriteArrayFieldStart("inventoryItems");
                    foreach (int itemId in inventory.inventoryItems) {
                        generator.WriteValue(itemId);
                    }

                    generator.WriteEndArrayAsync();
                }
            };
            PROPERTIES["equippedItems"] = (inventory, generator, context, writeEmpty) => {
                if (writeEmpty || inventory.equippedItems.Any()) {
                    generator.WriteArrayFieldStart("equippedItems");
                    foreach (int itemId in inventory.equippedItems) {
                        generator.WriteValue(itemId);
                    }

                    generator.WriteEndArray();
                }
            };
            PROPERTIES["itemSlots"] = (inventory, generator, context, writeEmpty) => {
                if (writeEmpty || inventory.itemSlots.Any()) {
                    generator.WriteArrayFieldStart("itemSlots");
                    foreach (int itemId in inventory.itemSlots) {
                        generator.WriteValue(itemId);
                    }

                    generator.WriteEndArray();
                }
            };
            PROPERTIES["lastInventoryRefreshTime"] = (inventory, generator, context, writeEmpty) => {
                if (writeEmpty || inventory.lastInventoryRefreshTime != 0.0) {
                    generator.WriteField("lastInventoryRefreshTime", inventory.lastInventoryRefreshTime);
                }
            };
        }

        public void writeInventory(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool summary) {
            List<GameObject> objects = context.ObjectContainer.ToList();

            if (summary) {

                List<Item> items = new List<Item>();
                foreach (int itemId in inventoryItems) {
                    if (itemId >= objects.Count) {
                        continue;
                    }

                    GameObject itemObject = objects[itemId];
                    if (!Item.isDefaultItem(itemObject)) {
                        items.Add(new Item(itemObject));
                    }
                }
                foreach (int itemId in equippedItems) {
                    if (itemId >= objects.Count) {
                        continue;
                    }

                    GameObject itemObject = objects[itemId];
                    if (!Item.isDefaultItem(itemObject)) {
                        items.Add(new Item(itemObject));
                    }
                }

                writeInventorySummary(generator, items);
            } else {
                generator.WriteStartObject();

                List<Item> items = new List<Item>();
                foreach (int itemId in inventoryItems) {
                    if (itemId >= objects.Count) {
                        continue;
                    }

                    GameObject itemObject = objects[itemId];
                    if (!Item.isDefaultItem(itemObject)) {
                        items.Add(new Item(itemObject));
                    }
                }

                generator.WritePropertyName("items");
                writeInventoryLong(generator, context, items, writeEmpty);

                items.Clear();
                foreach (int itemId in equippedItems) {
                    if (itemId >= objects.Count) {
                        continue;
                    }

                    GameObject itemObject = objects[itemId];
                    if (!Item.isDefaultItem(itemObject)) {
                        items.Add(new Item(itemObject));
                    }
                }

                generator.WritePropertyName("equipped");
                writeInventoryLong(generator, context, items, writeEmpty);

                generator.WriteEndObject();
            }
        }

        public static void writeInventorySummary(JsonTextWriter generator, List<Item> items) {
            Dictionary<ArkName, int> itemMap = new Dictionary<ArkName, int>();

            foreach (Item item in items) {
                if (itemMap.ContainsKey(item.className)) {
                    itemMap[item.className] += item.quantity;
                }
                else {
                    itemMap[item.className] = item.quantity;
                }
            }

            generator.WriteStartArray();

            foreach (KeyValuePair<ArkName, int> entry in itemMap.OrderByDescending(entry => entry.Value)) {
                generator.WriteStartObject();

                string name = entry.Key.ToString();
                if (DataManager.ArkDataManager.HasItem(name)) {
                    name = DataManager.ArkDataManager.GetItem(name).Name;
                }

                generator.WritePropertyName("name");
                generator.WriteValue(name);
                generator.WritePropertyName("count");
                generator.WriteValue(entry.Value);

                generator.WriteEndObject();
            }

            generator.WriteEndArray();
        }

        public static void writeInventoryLong(JsonTextWriter generator, IDataContext context, List<Item> items, bool writeEmpty) {
            generator.WriteStartArray();

            foreach (Item item in items.OrderBy(item => DataManager.ArkDataManager.HasItem(item.className.ToString()) ? DataManager.ArkDataManager.GetItem(item.className.ToString()).Name : item.className.ToString())) {
                generator.WriteStartObject();

                item.writeAllProperties(generator, context, writeEmpty);

                generator.WriteEndObject();
            }
            generator.WriteEndArray();
        }

    }

}
