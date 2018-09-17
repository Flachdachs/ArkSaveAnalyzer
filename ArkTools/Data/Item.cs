using System;
using System.Collections.Generic;
using ArkTools.DataManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Data;
using SavegameToolkit.Propertys;
using SavegameToolkit.Structs;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;

namespace ArkTools.Data {
    public class Item {

        public static int COLOR_SLOT_COUNT = 6;

        public bool canEquip;

        public bool canSlot;

        public bool isEngram;

        public bool isBlueprint;

        public bool canRemove;

        public bool canRemoveFromCluster;

        public bool isHidden;

        public ArkName className;

        public string type;

        public string blueprintGeneratedClass;

        public int quantity;

        public string customName;

        public string customDescription;

        public float durability;

        public float rating;

        public byte quality;

        public double nextSpoilingTime;

        public double lastSpoilingTime;

        public short[] itemStatValues = new short[ItemStatDefinitions.Instance.Count];

        public short[] itemColors = new short[COLOR_SLOT_COUNT];

        public short[] preSkinItemColors = new short[COLOR_SLOT_COUNT];

        public byte[] eggLevelups = new byte[AttributeNames.Instance.Count];

        public byte[] eggColors = new byte[COLOR_SLOT_COUNT];

        public string crafterCharacterName;

        public string crafterTribeName;

        public float craftedSkillBonus;

        public int uploadOffset;

        public Item() {
            canEquip = true;
            canSlot = true;
            canRemove = true;
            canRemoveFromCluster = true;
            quantity = 1;
            customName = "";
            customDescription = "";
            crafterCharacterName = "";
            crafterTribeName = "";
        }

        /**
         * From ArkSavegame
         */
        public Item(GameObject item) {
            className = item.ClassName;
            ItemData itemData = ArkDataManager.GetItem(className.ToString());
            type = itemData != null ? itemData.Name : className.ToString();
            blueprintGeneratedClass = itemData != null ? itemData.BlueprintGeneratedClass : null;

            canEquip = item.GetPropertyValue<bool>("bAllowEquppingItem", defaultValue: true);
            canSlot = item.GetPropertyValue<bool>("bCanSlot", defaultValue: true);
            isEngram = item.GetPropertyValue<bool>("bIsEngram");
            isBlueprint = item.GetPropertyValue<bool>("bIsBlueprint");
            canRemove = item.GetPropertyValue<bool>("bAllowRemovalFromInventory", defaultValue: true);
            canRemoveFromCluster = true;
            isHidden = item.GetPropertyValue<bool>("bHideFromInventoryDisplay");

            //quantity = Math.Max(1, item.findPropertyValue<Number>("ItemQuantity").map(Number.intValue).orElse(1));
            quantity = Math.Max(1, item.GetPropertyValue<int>("ItemQuantity", defaultValue: 1));

            customName = item.GetPropertyValue<string>("CustomItemName", defaultValue: string.Empty);

            customDescription = item.GetPropertyValue<string>("CustomItemDescription", defaultValue: string.Empty);

            durability = item.GetPropertyValue<float>("SavedDurability");

            rating = item.GetPropertyValue<float>("ItemRating");

            quality = item.GetPropertyValue<ArkByteValue>("ItemQualityIndex")?.ByteValue ?? 0;

            nextSpoilingTime = item.GetPropertyValue<double>("NextSpoilingTime");
            lastSpoilingTime = item.GetPropertyValue<double>("LastSpoilingTime");

            for (int i = 0; i < ItemStatDefinitions.Instance.Count; i++) {
                itemStatValues[i] = item.GetPropertyValue<short>("ItemStatValues", i);
            }

            for (int i = 0; i < itemColors.Length; i++) {
                itemColors[i] = item.GetPropertyValue<short>("ItemColorID", i);
            }

            for (int i = 0; i < preSkinItemColors.Length; i++) {
                preSkinItemColors[i] = item.GetPropertyValue<short>("PreSkinItemColorID", i);
            }

            for (int i = 0; i < eggLevelups.Length; i++) {
                eggLevelups[i] = item.GetPropertyValue<ArkByteValue>("EggNumberOfLevelUpPointsApplied", i)?.ByteValue ?? 0;
            }

            for (int i = 0; i < eggColors.Length; i++) {
                eggColors[i] = item.GetPropertyValue<ArkByteValue>("EggColorSetIndices", i)?.ByteValue ?? 0;
            }

            crafterCharacterName = item.GetPropertyValue<string>("CrafterCharacterName", defaultValue: string.Empty);
            crafterTribeName = item.GetPropertyValue<string>("CrafterTribeName", defaultValue: string.Empty);
            craftedSkillBonus = item.GetPropertyValue<float>("CraftedSkillBonus");
        }

        /**
         * From cluster storage
         */
        public Item(IPropertyContainer item) {
            blueprintGeneratedClass = item.GetPropertyValue<ObjectReference>("ItemArchetype").ObjectString.ToString();
            className = ArkName.From(blueprintGeneratedClass.Substring(blueprintGeneratedClass.LastIndexOf('.') + 1));
            ItemData itemData = ArkDataManager.GetItem(className.ToString());
            type = itemData != null ? itemData.Name : className.ToString();

            canEquip = true;
            canSlot = item.GetPropertyValue<bool>("bIsSlot", defaultValue: true);
            isEngram = item.GetPropertyValue<bool>("bIsEngram");
            isBlueprint = item.GetPropertyValue<bool>("bIsBlueprint");
            canRemove = item.GetPropertyValue<bool>("bAllowRemovalFromInventory", defaultValue: true);
            canRemoveFromCluster = item.GetPropertyValue<bool>("bAllowRemovalFromSteamInventory", defaultValue: true);
            isHidden = item.GetPropertyValue<bool>("bHideFromInventoryDisplay");

            //quantity = Math.Max(1, item.findPropertyValue<Number>("ItemQuantity").map(Number::intValue).orElse(1));
            quantity = Math.Max(1, item.GetPropertyValue<int>("ItemQuantity", defaultValue: 1));

            customName = item.GetPropertyValue<string>("CustomItemName", defaultValue: string.Empty);

            customDescription = item.GetPropertyValue<string>("CustomItemDescription", defaultValue: string.Empty);

            durability = item.GetPropertyValue<float>("ItemDurability");

            rating = item.GetPropertyValue<float>("ItemRating");

            quality = item.GetPropertyValue<ArkByteValue>("ItemQualityIndex")?.ByteValue ?? 0;

            nextSpoilingTime = item.GetPropertyValue<double>("NextSpoilingTime");
            lastSpoilingTime = item.GetPropertyValue<double>("LastSpoilingTime");

            for (int i = 0; i < itemStatValues.Length; i++) {
                itemStatValues[i] = item.GetPropertyValue<short>("ItemStatValues", i);
            }

            for (int i = 0; i < itemColors.Length; i++) {
                itemColors[i] = item.GetPropertyValue<short>("ItemColorID", i);
            }

            for (int i = 0; i < preSkinItemColors.Length; i++) {
                preSkinItemColors[i] = item.GetPropertyValue<short>("PreSkinItemColorID", i);
            }

            for (int i = 0; i < eggLevelups.Length; i++) {
                eggLevelups[i] = item.GetPropertyValue<ArkByteValue>("EggNumberOfLevelUpPointsApplied", i)?.ByteValue ?? 0;
            }

            for (int i = 0; i < eggColors.Length; i++) {
                eggColors[i] = item.GetPropertyValue<ArkByteValue>("EggColorSetIndices", i)?.ByteValue ?? 0;
            }

            crafterCharacterName = item.GetPropertyValue<string>("CrafterCharacterName", defaultValue: string.Empty);
            crafterTribeName = item.GetPropertyValue<string>("CrafterTribeName", defaultValue: string.Empty);
            craftedSkillBonus = item.GetPropertyValue<float>("CraftedSkillBonus");
        }

        /**
         * From JSON / ModificationFile
         */
        public Item(JToken node) {
            className = ArkName.From(node.Value<string>("className"));
            ItemData itemData = ArkDataManager.GetItem(className.ToString());
            type = itemData != null ? itemData.Name : className.ToString();
            blueprintGeneratedClass = "BlueprintGeneratedClass " + node.Value<string>("blueprintGeneratedClass");

            canEquip = node.Value<bool?>("canEquip") ?? true;
            canSlot = node.Value<bool?>("canSlot") ?? true;
            isEngram = node.Value<bool>("isEngram");
            isBlueprint = node.Value<bool>("isBlueprint");
            canRemove = node.Value<bool?>("canRemove") ?? true;
            canRemoveFromCluster = node.Value<bool?>("canRemoveFromCluster") ?? true;
            isHidden = node.Value<bool>("isHidden");

            quantity = Math.Max(1, node.Value<int?>("quantity") ?? 1);

            customName = node.Value<string>("customName");

            customDescription = node.Value<string>("customDescription");

            durability = node.Value<float>("durability");
            rating = node.Value<float>("rating");

            quality = node.Value<byte>("quality");

            nextSpoilingTime = node.Value<double>("nextSpoilingTime");
            lastSpoilingTime = node.Value<double>("lastSpoilingTime");

            for (int i = 0; i < itemStatValues.Length; i++) {
                itemStatValues[i] = node.Value<short>("itemStatsValue_" + i);
            }

            for (int i = 0; i < itemColors.Length; i++) {
                itemColors[i] = node.Value<short>("itemColor_" + i);
            }

            for (int i = 0; i < preSkinItemColors.Length; i++) {
                preSkinItemColors[i] = node.Value<short>("preSkinItemColor_" + i);
            }

            for (int i = 0; i < eggLevelups.Length; i++) {
                eggLevelups[i] = node.Value<byte>("eggLevelup_" + i);
            }

            for (int i = 0; i < eggColors.Length; i++) {
                eggColors[i] = node.Value<byte>("eggColor_" + i);
            }

            crafterCharacterName = node.Value<string>("crafterCharacterName");
            crafterTribeName = node.Value<string>("crafterTribeName");
            craftedSkillBonus = node.Value<float>("craftedSkillBonus");

            uploadOffset = node.Value<int>("uploadOffset");
        }

        public StructPropertyList toClusterData() {
            if (blueprintGeneratedClass == "BlueprintGeneratedClass ") {
                Console.Error.WriteLine("Item " + className + " is missing blueprintGeneratedClass.");
                return null;
            }

            StructPropertyList result = new StructPropertyList();
            StructPropertyList arkTributeItem = new StructPropertyList();

            result.Properties.Add(new PropertyStruct("ArkTributeItem", arkTributeItem, ArkName.From("ItemNetInfo")));

            ObjectReference itemArchetype = new ObjectReference();
            itemArchetype.ObjectType = ObjectReference.TypePath;
            itemArchetype.ObjectString = ArkName.From(blueprintGeneratedClass);
            arkTributeItem.Properties.Add(new PropertyObject("ItemArchetype", itemArchetype));

            Random random = new Random();
            long randomId = random.NextLong();

            StructPropertyList structProperty = new StructPropertyList();

            structProperty.Properties.Add(new PropertyUInt32("ItemID1", (int) (randomId >> 32)));
            structProperty.Properties.Add(new PropertyUInt32("ItemID2", (int) randomId));

            arkTributeItem.Properties.Add(new PropertyStruct("ItemId", structProperty, ArkName.From("ItemNetID")));

            arkTributeItem.Properties.Add(new PropertyBool("bIsBlueprint", isBlueprint));
            arkTributeItem.Properties.Add(new PropertyBool("bIsEngram", isEngram));
            arkTributeItem.Properties.Add(new PropertyBool("bIsCustomRecipe", false));
            arkTributeItem.Properties.Add(new PropertyBool("bIsFoodRecipe", false));
            arkTributeItem.Properties.Add(new PropertyBool("bIsRepairing", false));
            arkTributeItem.Properties.Add(new PropertyBool("bAllowRemovalFromInventory", canRemove));
            arkTributeItem.Properties.Add(new PropertyBool("bAllowRemovalFromSteamInventory", canRemoveFromCluster));
            arkTributeItem.Properties.Add(new PropertyBool("bHideFromInventoryDisplay", isHidden));
            arkTributeItem.Properties.Add(new PropertyBool("bFromSteamInventory", false));
            arkTributeItem.Properties.Add(new PropertyBool("bIsFromAllClustersInventory", false));
            arkTributeItem.Properties.Add(new PropertyBool("bIsEquipped", false));
            arkTributeItem.Properties.Add(new PropertyBool("bIsSlot", canSlot));
            arkTributeItem.Properties.Add(new PropertyUInt32("ExpirationTimeUTC", 0));
            arkTributeItem.Properties.Add(new PropertyUInt32("ItemQuantity", quantity - 1));
            arkTributeItem.Properties.Add(new PropertyFloat("ItemDurability", durability));
            arkTributeItem.Properties.Add(new PropertyFloat("ItemRating", rating));

            arkTributeItem.Properties.Add(new PropertyByte("ItemQualityIndex", quality));

            for (int i = 0; i < itemStatValues.Length; i++) {
                arkTributeItem.Properties.Add(new PropertyUInt16("ItemStatValues", i, itemStatValues[i]));
            }

            for (int i = 0; i < itemColors.Length; i++) {
                arkTributeItem.Properties.Add(new PropertyInt16("ItemColorID", i, itemColors[i]));
            }

            ObjectReference itemCustomClass = new ObjectReference();
            itemCustomClass.Length = 8;
            itemCustomClass.ObjectId = -1;
            itemCustomClass.ObjectType = ObjectReference.TypeId;
            arkTributeItem.Properties.Add(new PropertyObject("ItemCustomClass", itemCustomClass));

            ObjectReference itemSkinTemplate = new ObjectReference();
            itemSkinTemplate.Length = 8;
            itemSkinTemplate.ObjectId = -1;
            itemSkinTemplate.ObjectType = ObjectReference.TypeId;
            arkTributeItem.Properties.Add(new PropertyObject("ItemSkinTemplate", itemSkinTemplate));

            arkTributeItem.Properties.Add(new PropertyFloat("CraftingSkill", 0.0f));

            arkTributeItem.Properties.Add(new PropertyString("CustomItemName", customName));
            arkTributeItem.Properties.Add(new PropertyString("CustomItemDescription", customDescription));

            // TODO: add other values

            arkTributeItem.Properties.Add(new PropertyDouble("NextSpoilingTime", nextSpoilingTime));
            arkTributeItem.Properties.Add(new PropertyDouble("LastSpoilingTime", lastSpoilingTime));

            ObjectReference lastOwnerPlayer = new ObjectReference();
            lastOwnerPlayer.Length = 4;
            lastOwnerPlayer.ObjectId = -1;
            lastOwnerPlayer.ObjectType = ObjectReference.TypeId;
            arkTributeItem.Properties.Add(new PropertyObject("LastOwnerPlayer", lastOwnerPlayer));

            arkTributeItem.Properties.Add(new PropertyDouble("LastAutoDurabilityDecreaseTime", 0.0));
            arkTributeItem.Properties.Add(new PropertyStruct("OriginalItemDropLocation", new StructVector(), ArkName.From("Vector")));

            for (int i = 0; i < preSkinItemColors.Length; i++) {
                arkTributeItem.Properties.Add(new PropertyInt16("PreSkinItemColorID", i, preSkinItemColors[i]));
            }

            for (int i = 0; i < eggLevelups.Length; i++) {
                arkTributeItem.Properties.Add(new PropertyByte("EggNumberOfLevelUpPointsApplied", i, eggLevelups[i]));
            }

            arkTributeItem.Properties.Add(new PropertyFloat("EggTamedIneffectivenessModifier", 0.0f));

            for (int i = 0; i < eggColors.Length; i++) {
                arkTributeItem.Properties.Add(new PropertyByte("EggColorSetIndices", i, eggColors[i]));
            }

            arkTributeItem.Properties.Add(new PropertyString("CrafterCharacterName", crafterCharacterName));
            arkTributeItem.Properties.Add(new PropertyString("CrafterTribeName", crafterTribeName));
            arkTributeItem.Properties.Add(new PropertyFloat("CraftedSkillBonus", craftedSkillBonus));

            arkTributeItem.Properties.Add(new PropertyByte("ItemVersion", (byte) 0));
            arkTributeItem.Properties.Add(new PropertyInt("CustomItemID", 0));
            arkTributeItem.Properties.Add(new PropertyArray("SteamUserItemID", new ArkArrayUInt64()));

            arkTributeItem.Properties.Add(new PropertyBool("bIsInitialItem", false));

            arkTributeItem.Properties.Add(new PropertyDouble("ClusterSpoilingTimeUTC", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

            result.Properties.Add(new PropertyFloat("Version", 2.0f));
            result.Properties.Add(new PropertyInt("UploadTime", (int) DateTimeOffset.UtcNow.AddSeconds(uploadOffset).ToUnixTimeSeconds()));

            return result;
        }

        public GameObject toGameObject(ICollection<GameObject> existingObjects, int ownerInventory) {
            GameObject gameObject = new GameObject();

            gameObject.ClassName = className;

            if (!canEquip) {
                gameObject.Properties.Add(new PropertyBool("bAllowEquppingItem", canEquip));
            }

            if (!canSlot) {
                gameObject.Properties.Add(new PropertyBool("bCanSlot", canSlot));
            }

            if (isEngram) {
                gameObject.Properties.Add(new PropertyBool("bIsEngram", isEngram));
            }

            if (isBlueprint) {
                gameObject.Properties.Add(new PropertyBool("bIsBlueprint", isBlueprint));
            }

            if (!canRemove) {
                gameObject.Properties.Add(new PropertyBool("bAllowRemovalFromInventory", canRemove));
            }

            if (isHidden) {
                gameObject.Properties.Add(new PropertyBool("bHideFromInventoryDisplay", isHidden));
            }

            if (quantity != 1) {
                gameObject.Properties.Add(new PropertyInt("ItemQuantity", quantity));
            }

            if (!string.IsNullOrEmpty(customName)) {
                gameObject.Properties.Add(new PropertyString("CustomItemName", customName));
            }

            if (!string.IsNullOrEmpty(customDescription)) {
                gameObject.Properties.Add(new PropertyString("CustomItemDescription", customDescription));
            }

            if (durability > 0) {
                gameObject.Properties.Add(new PropertyFloat("SavedDurability", durability));
            }

            if (quality > 0) {
                gameObject.Properties.Add(new PropertyByte("ItemQualityIndex", quality));
            }

            if (nextSpoilingTime > 0) {
                gameObject.Properties.Add(new PropertyDouble("NextSpoilingTime", nextSpoilingTime));
            }

            if (lastSpoilingTime > 0) {
                gameObject.Properties.Add(new PropertyDouble("LastSpoilingTime", lastSpoilingTime));
            }

            for (int i = 0; i < itemStatValues.Length; i++) {
                if (itemStatValues[i] != 0) {
                    gameObject.Properties.Add(new PropertyUInt16("ItemStatValues", i, itemStatValues[i]));
                }
            }

            if (crafterCharacterName != null && !string.IsNullOrEmpty(crafterCharacterName)) {
                gameObject.Properties.Add(new PropertyString("CrafterCharacterName", crafterCharacterName));
            }

            if (crafterTribeName != null && !string.IsNullOrEmpty(crafterTribeName)) {
                gameObject.Properties.Add(new PropertyString("CrafterTribeName", crafterTribeName));
            }

            if (craftedSkillBonus != 0.0f) {
                gameObject.Properties.Add(new PropertyFloat("CraftedSkillBonus", craftedSkillBonus));
            }

            HashSet<long> itemIDs = new HashSet<long>(); // Stored as StructPropertyList with 2 UInt32
            HashSet<ArkName> names = new HashSet<ArkName>();

            foreach (GameObject existingObject in existingObjects) {
                existingObject.Names.ForEach(name => names.Add(name));

                IPropertyContainer itemID = existingObject.GetPropertyValue<IPropertyContainer>("ItemId");
                if (itemID != null) {
                    int? itemID1 = itemID.GetPropertyValue<int?>("ItemID1");
                    int? itemID2 = itemID.GetPropertyValue<int?>("ItemID2");
                    if (itemID1 != null && itemID2 != null) {
                        long id = (long) itemID1.Value << 32 | (itemID2.Value & 0xFFFFFFFFL);
                        itemIDs.Add(id);
                        continue;
                    }
                }
            }

            Random random = new Random();

            Func<ArkName, ArkName> findFreeName = name => {
                for (int i = 1; i < int.MaxValue; i++) {
                    ArkName tempName = ArkName.From(name.Name, i);
                    if (!names.Contains(tempName)) {
                        return tempName;
                    }
                }

                throw new Exception("This is insane.");
            };

            long randomId = random.NextLong();
            while (itemIDs.Contains(randomId)) {
                randomId = random.NextLong();
            }

            StructPropertyList structProperty = new StructPropertyList();

            structProperty.Properties.Add(new PropertyUInt32("ItemID1", (int) (randomId >> 32)));
            structProperty.Properties.Add(new PropertyUInt32("ItemID2", (int) randomId));

            gameObject.Properties.Add(new PropertyStruct("ItemId", structProperty, ArkName.From("ItemNetID")));

            gameObject.Names.Clear();
            gameObject.Names.Add(findFreeName(className));

            gameObject.IsItem = true;

            ObjectReference ownerInventoryReference = new ObjectReference();
            ownerInventoryReference.Length = 8;
            ownerInventoryReference.ObjectId = ownerInventory;
            ownerInventoryReference.ObjectType = ObjectReference.TypeId;

            gameObject.Properties.Add(new PropertyObject("OwnerInventory", ownerInventoryReference));
            gameObject.ExtraData = new ExtraDataZero();

            return gameObject;
        }

        public static readonly SortedDictionary<string, WriterFunction<Item>> PROPERTIES;
        public static readonly List<WriterFunction<Item>> PROPERTIES_LIST;


        static Item() {
            PROPERTIES = new SortedDictionary<string, WriterFunction<Item>>();
            /**
             * Item Properties
             */
            PROPERTIES["canEquip"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !item.canEquip) {
                    generator.WriteField("canEquip", item.canEquip);
                }
            };
            PROPERTIES["canSlot"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !item.canSlot) {
                    generator.WriteField("canSlot", item.canSlot);
                }
            };
            PROPERTIES["isEngram"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.isEngram) {
                    generator.WriteField("isEngram", item.isEngram);
                }
            };
            PROPERTIES["isBlueprint"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.isBlueprint) {
                    generator.WriteField("isBlueprint", item.isBlueprint);
                }
            };
            PROPERTIES["canRemove"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !item.canRemove) {
                    generator.WriteField("canRemove", item.canRemove);
                }
            };
            PROPERTIES["canRemoveFromCluster"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !item.canRemoveFromCluster) {
                    generator.WriteField("canRemoveFromCluster", item.canRemoveFromCluster);
                }
            };
            PROPERTIES["isHidden"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.isHidden) {
                    generator.WriteField("isHidden", item.isHidden);
                }
            };
            PROPERTIES["type"]= (item, generator, context, writeEmpty) => {
                if (context is DataCollector) {
                    generator.WriteField("type", item.className.ToString());
                }
                else {
                    generator.WriteField("type", item.type);
                }
            };
            PROPERTIES["blueprintGeneratedClass"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.blueprintGeneratedClass != null && !string.IsNullOrEmpty(item.blueprintGeneratedClass)) {
                    if (item.blueprintGeneratedClass != null) {
                        generator.WriteField("blueprintGeneratedClass", item.blueprintGeneratedClass);
                    }
                    else {
                        generator.WriteNullField("blueprintGeneratedClass");
                    }
                }
            };
            PROPERTIES["quantity"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.quantity != 0) {
                    generator.WriteField("quantity", item.quantity);
                }
            };
            PROPERTIES["customName"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(item.customName)) {
                    generator.WriteField("customName", item.customName);
                }
            };
            PROPERTIES["customDescription"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(item.customDescription)) {
                    generator.WriteField("customDescription", item.customDescription);
                }
            };
            PROPERTIES["durability"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.durability != 0.0f) {
                    generator.WriteField("durability", item.durability);
                }
            };
            PROPERTIES["rating"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.rating != 0.0f) {
                    generator.WriteField("rating", item.rating);
                }
            };
            PROPERTIES["quality"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.quality != 0) {
                    generator.WriteField("quality", item.quality);
                }
            };
            PROPERTIES["itemStatValues"]= (item, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("itemStatValues");
                }

                for (int index = 0; index < item.itemStatValues.Length; index++) {
                    if (writeEmpty || item.itemStatValues[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("itemStatValues");
                        }

                        //generator.WriteNumberField(ItemStatDefinitions.get(index), Short.toUnsignedInt(item.itemStatValues[index]));
                        generator.WriteField(ItemStatDefinitions.Instance[index], item.itemStatValues[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["itemColors"]= (item, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("itemColors");
                }

                for (int index = 0; index < item.itemColors.Length; index++) {
                    if (writeEmpty || item.itemColors[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("itemColors");
                        }

                        //generator.WriteNumberField(index.ToString(), Short.toUnsignedInt(item.itemColors[index]));
                        generator.WriteField(index.ToString(), item.itemColors[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["preSkinItemColors"]= (item, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("preSkinItemColors");
                }

                for (int index = 0; index < item.preSkinItemColors.Length; index++) {
                    if (writeEmpty || item.preSkinItemColors[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("preSkinItemColors");
                        }

                        //generator.WriteNumberField(index.ToString(), Short.toUnsignedInt(item.preSkinItemColors[index]));
                        generator.WriteField(index.ToString(), item.preSkinItemColors[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["eggColors"]= (item, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("eggColors");
                }

                for (int index = 0; index < item.eggColors.Length; index++) {
                    if (writeEmpty || item.eggColors[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("eggColors");
                        }

                        generator.WriteField(index.ToString(), item.eggColors[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["eggLevelups"]= (item, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("eggLevelups");
                }

                for (int index = 0; index < item.eggLevelups.Length; index++) {
                    if (writeEmpty || item.eggLevelups[index] != 0) {
                        if (empty) {
                            empty = false;
                            generator.WriteObjectFieldStart("eggLevelups");
                        }

                        generator.WriteField(AttributeNames.Instance[index], item.eggLevelups[index]);
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            PROPERTIES["crafterCharacterName"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(item.crafterCharacterName)) {
                    generator.WriteField("crafterCharacterName", item.crafterCharacterName);
                }
            };
            PROPERTIES["crafterTribeName"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(item.crafterTribeName)) {
                    generator.WriteField("crafterTribeName", item.crafterTribeName);
                }
            };
            PROPERTIES["craftedSkillBonus"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.craftedSkillBonus != 0.0f) {
                    generator.WriteField("craftedSkillBonus", item.craftedSkillBonus);
                }
            };
            PROPERTIES["uploadOffset"]= (item, generator, context, writeEmpty) => {
                if (writeEmpty || item.uploadOffset != 0) {
                    generator.WriteField("uploadOffset", item.uploadOffset);
                }
            };

            PROPERTIES_LIST = new List<WriterFunction<Item>>(PROPERTIES.Values);
        }

        public static bool isDefaultItem(GameObject item) {
            if (item.GetPropertyValue<bool>("bIsEngram")) {
                return true;
            }

            if (item.GetPropertyValue<bool>("bHideFromInventoryDisplay")) {
                return true;
            }

            return false;
        }

        public void writeAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty) {
            foreach (WriterFunction<Item> writer in PROPERTIES_LIST) {
                writer(this, generator, context, writeEmpty);
            }
        }

    }
}
