using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Structs;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;
using SavegameToolkitAdditions.IndexMappings;

namespace ArkTools.Data {

    public class Player {
        private int savedPlayerDataVersion;

        /// <summary>
        /// Player ID
        /// </summary>
        public readonly long playerDataId;                        //

        /// <summary>
        /// Steam ID
        /// </summary>
        public readonly StructUniqueNetIdRepl uniqueId;           //

        private readonly string savedNetworkAddress;

        private readonly string playerName;                        //

        public readonly int tribeId;                               //

        private readonly int playerDataVersion;

        private readonly int spawnDayNumber;                       //

        private readonly float spawnDayTime;                       //

        private readonly bool isFemale;

        private readonly StructLinearColor[] bodyColors;            //

        private readonly StructLinearColor overrideHeadHairColor;

        private readonly StructLinearColor overrideFacialHairColor;

        private readonly byte facialHairIndex;                         //

        private readonly byte headHairIndex;                           //

        private readonly string playerCharacterName;                  //

        private readonly float[] rawBoneModifiers;

        // todo nachforschen
#pragma warning disable 649
        private readonly int playerSpawnRegionIndex;
#pragma warning restore 649

        private readonly int totalEngramPoints;                       //

        private readonly List<string> engramBlueprints = new List<string>();        //

        private readonly byte[] numberOfLevelUpPointsApplied;             //

        private readonly float percentageOfHeadHairGrowth;

        private readonly float percentageOfFacialHairGrowth;

        private readonly GameObject inventory;

        private readonly LocationData location;

        private readonly int characterLevel;                            //

        private readonly float experiencePoints;                        //

        private readonly float lastHypothermalCharacterInsulationValue;

        private readonly float lastHyperthermalCharacterInsulationValue;

        private readonly float[] currentStatusValues = new float[AttributeNames.Instance.Count];

        public Player(string path, IDataContext context, ReadingOptions ro) {
            ArkProfile profile = new ArkProfile().ReadBinary<ArkProfile>(path, ro);

            savedPlayerDataVersion = profile.GetPropertyValue<int>("SavedPlayerDataVersion");

            StructPropertyList myData = profile.GetPropertyValue<IStruct, StructPropertyList>("MyData");

            playerDataId = myData.GetPropertyValue<long>("PlayerDataID");
            uniqueId = myData.GetPropertyValue<IStruct, StructUniqueNetIdRepl>("UniqueID");
            savedNetworkAddress = myData.GetPropertyValue<string>("SavedNetworkAddress");
            playerName = myData.GetPropertyValue<string>("PlayerName");
            tribeId = myData.GetPropertyValue<int>("TribeID");
            playerDataVersion = myData.GetPropertyValue<int>("PlayerDataVersion");
            spawnDayNumber = myData.GetPropertyValue<int>("SpawnDayNumber");
            spawnDayTime = myData.GetPropertyValue<float>("SpawnDayTime");

            IPropertyContainer characterConfig = myData.GetPropertyValue<IStruct, IPropertyContainer>("MyPlayerCharacterConfig");

            // Character data

            isFemale = characterConfig.GetPropertyValue<bool>("bIsFemale");
            bodyColors = Enumerable.Range(0, PlayerBodyColorRegions.Instance.Count).Select(i => characterConfig.GetPropertyValue<IStruct, StructLinearColor>("BodyColors", i)).ToArray();
            //for (int i = 0; i < PlayerBodyColorRegions.Instance.Count; i++) {
            //    bodyColors[i] = characterConfig.getPropertyValue<IStruct, StructLinearColor>("BodyColors", i);
            //}

            overrideHeadHairColor = characterConfig.GetPropertyValue<IStruct, StructLinearColor>("OverrideHeadHairColor");
            overrideFacialHairColor = characterConfig.GetPropertyValue<IStruct, StructLinearColor>("OverrideFacialHairColor");
            facialHairIndex = characterConfig.GetPropertyValue<ArkByteValue>("FacialHairIndex")?.ByteValue ?? 0;
            headHairIndex = characterConfig.GetPropertyValue<ArkByteValue>("HeadHairIndex")?.ByteValue ?? 0;
            playerCharacterName = characterConfig.GetPropertyValue<string>("PlayerCharacterName", defaultValue: string.Empty);

            rawBoneModifiers = Enumerable.Range(0, PlayerBoneModifierNames.Instance.Count).Select(i => characterConfig.GetPropertyValue<float>("RawBoneModifiers", i)).ToArray();
            //for (int i = 0; i < PlayerBoneModifierNames.Instance.Count; i++) {
            //    rawBoneModifiers[i] = characterConfig.findPropertyValue<float>("RawBoneModifiers", i);
            //}

            IPropertyContainer characterStats = myData.GetPropertyValue<IStruct, IPropertyContainer>("MyPersistentCharacterStats");

            characterLevel = characterStats.GetPropertyValue<short>("CharacterStatusComponent_ExtraCharacterLevel") + 1;
            experiencePoints = characterStats.GetPropertyValue<float>("CharacterStatusComponent_ExperiencePoints");
            totalEngramPoints = characterStats.GetPropertyValue<int>("PlayerState_TotalEngramPoints");

            List<ObjectReference> learnedEngrams = characterStats.GetPropertyValue<IArkArray, ArkArrayObjectReference>("PlayerState_EngramBlueprints");

            if (learnedEngrams != null) {
                foreach (ObjectReference reference in learnedEngrams) {
                    engramBlueprints.Add(reference.ObjectString.ToString());
                }
            }

            numberOfLevelUpPointsApplied = Enumerable.Range(0, AttributeNames.Instance.Count)
                    .Select(i => characterStats.GetPropertyValue<ArkByteValue>("CharacterStatusComponent_NumberOfLevelUpPointsApplied", i)?.ByteValue ?? 0)
                    .ToArray();
            //for (int i = 0; i < AttributeNames.Instance.Count; i++) {
            //    numberOfLevelUpPointsApplied[i] = characterStats.findPropertyValue<ArkByteValue>("CharacterStatusComponent_NumberOfLevelUpPointsApplied", i)
            //            ?.ByteValue ?? 0;
            //}

            percentageOfHeadHairGrowth = characterStats.GetPropertyValue<float>("PercentageOfHeadHairGrowth");
            percentageOfFacialHairGrowth = characterStats.GetPropertyValue<float>("PercentageOfFacialHairGrowth");

            if (context.ObjectContainer == null) {
                return;
            }

            GameObject player = null;
            foreach (GameObject gameObject in context.ObjectContainer) {
                long? linkedPlayerDataId = gameObject.GetPropertyValue<long?>("LinkedPlayerDataID");
                if (linkedPlayerDataId == null || linkedPlayerDataId != playerDataId)
                    continue;
                player = gameObject;
                break;
            }

            if (player == null) {
                return;
            }

            GameObject playerCharacterStatus = null;
            foreach (GameObject gameObject in context.ObjectContainer) {
                if (gameObject.Names.Count != 2 || gameObject.Names[0].ToString() != "PlayerCharacterStatus" || gameObject.Names[1] != player.Names[0])
                    continue;
                playerCharacterStatus = gameObject;
                break;
            }

            inventory = player.GetPropertyValue<ObjectReference, GameObject>("MyInventoryComponent", map: reference => context.ObjectContainer[reference]);
            location = player.Location;

            if (playerCharacterStatus == null)
                return;
            lastHypothermalCharacterInsulationValue = playerCharacterStatus.GetPropertyValue<float>("LastHypothermalCharacterInsulationValue");
            lastHyperthermalCharacterInsulationValue = playerCharacterStatus.GetPropertyValue<float>("LastHyperthermalCharacterInsulationValue");

            for (int index = 0; index < AttributeNames.Instance.Count; index++) {
                currentStatusValues[index] = playerCharacterStatus.GetPropertyValue<float>("CurrentStatusValues", index);
            }
        }

        public static readonly SortedDictionary<string, WriterFunction<Player>> Properties;
        private static readonly List<WriterFunction<Player>> propertiesList;
        private static readonly List<WriterFunction<Player>> propertiesListPrivacy;

        private static void addPropertyWriterFunction<T>(string name, Func<Player, bool> testValue, Func<Player, T> getValue) {
            Properties[name] = (player, generator, context, writeEmpty) => {
                if (writeEmpty || testValue(player)) {
                    generator.WriteField(name, getValue(player));
                }
            };
        }

        private static void addPropertyWriterFunctionWithNullTest<T1, T2>(string name, Func<Player, T1> nullTest, Func<Player, T2> getValue) where T1 : class {
            Properties[name] = (player, generator, context, writeEmpty) => {
                if (!writeEmpty && nullTest(player) is null)
                    return;
                if (nullTest(player) is null) {
                    generator.WriteNullField(name);
                } else {
                    generator.WriteField(name, getValue(player));
                }
            };
        }

        static Player() {
            Properties = new SortedDictionary<string, WriterFunction<Player>>();

            addPropertyWriterFunction("id", p => p.playerDataId != 0, p => p.playerDataId);
            //Properties["id"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.playerDataId != 0) {
            //        generator.WriteField("id", player.playerDataId);
            //    }
            //};

            addPropertyWriterFunctionWithNullTest("steamId", p => p.uniqueId, p => p.uniqueId.NetId);
            //Properties["steamId"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.uniqueId != null) {
            //        if (player.uniqueId == null) {
            //            generator.WriteNullField("steamId");
            //        } else {
            //            generator.WriteField("steamId", player.uniqueId.NetId);
            //        }
            //    }
            //};
            addPropertyWriterFunctionWithNullTest("savedNetworkAddress", p => p.savedNetworkAddress, p => p.savedNetworkAddress);
            //Properties["savedNetworkAddress"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.savedNetworkAddress != null) {
            //        if (player.savedNetworkAddress == null) {
            //            generator.WriteNullField("savedNetworkAddress");
            //        } else {
            //            generator.WriteField("savedNetworkAddress", player.savedNetworkAddress);
            //        }
            //    }
            //};
            addPropertyWriterFunctionWithNullTest("playerName", p => p.playerName, p => p.playerName);
            //Properties["playerName"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.playerName != null) {
            //        if (player.playerName == null) {
            //            generator.WriteNullField("playerName");
            //        } else {
            //            generator.WriteField("playerName", player.playerName);
            //        }
            //    }
            //};
            addPropertyWriterFunction("tribeId", p => p.tribeId != 0, p => p.tribeId);
            //Properties["tribeId"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.tribeId != 0) {
            //        generator.WriteField("tribeId", player.tribeId);
            //    }
            //};
            addPropertyWriterFunction("playerDataVersion", p => p.playerDataVersion != 0, p => p.playerDataVersion);
            //Properties["playerDataVersion"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.playerDataVersion != 0) {
            //        generator.WriteField("playerDataVersion", player.playerDataVersion);
            //    }
            //};
            addPropertyWriterFunction("spawnDayNumber", p => p.spawnDayNumber != 0, p => p.spawnDayNumber);
            //Properties["spawnDayNumber"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.spawnDayNumber != 0) {
            //        generator.WriteField("spawnDayNumber", player.spawnDayNumber);
            //    }
            //};
            addPropertyWriterFunction("spawnDayTime", p => Math.Abs(p.spawnDayTime) > 0f, p => p.spawnDayTime);
            //Properties["spawnDayTime"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.spawnDayTime) > 0f) {
            //        generator.WriteField("spawnDayTime", player.spawnDayTime);
            //    }
            //};
            addPropertyWriterFunction("isFemale", p => p.isFemale, p => p.isFemale);
            //Properties["isFemale"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.isFemale) {
            //        generator.WriteField("isFemale", player.isFemale);
            //    }
            //};
            Properties["bodyColors"] = (player, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("bodyColors");
                }

                for (int index = 0; index < player.bodyColors.Length; index++) {
                    if (!writeEmpty && player.bodyColors[index] == null)
                        continue;
                    if (empty) {
                        empty = false;
                        generator.WriteObjectFieldStart("bodyColors");
                    }

                    if (player.bodyColors[index] == null) {
                        generator.WriteNullField(PlayerBodyColorRegions.Instance[index]);
                    } else {
                        generator.WriteField(PlayerBodyColorRegions.Instance[index], player.bodyColors[index].GetRGBA());
                    }
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            addPropertyWriterFunctionWithNullTest("overrideHeadHairColor", p => p.overrideHeadHairColor, p => p.overrideHeadHairColor.GetRGBA());
            //Properties["overrideHeadHairColor"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.overrideHeadHairColor != null) {
            //        if (player.overrideHeadHairColor == null) {
            //            generator.WriteNullField("overrideHeadHairColor");
            //        } else {
            //            generator.WriteField("overrideHeadHairColor", player.overrideHeadHairColor.GetRGBA());
            //        }
            //    }
            //};
            addPropertyWriterFunctionWithNullTest("overrideFacialHairColor", p => p.overrideFacialHairColor, p => p.overrideFacialHairColor.GetRGBA());
            //Properties["overrideFacialHairColor"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.overrideFacialHairColor != null) {
            //        if (player.overrideFacialHairColor == null) {
            //            generator.WriteNullField("overrideFacialHairColor");
            //        } else {
            //            generator.WriteField("overrideFacialHairColor", player.overrideFacialHairColor.GetRGBA());
            //        }
            //    }
            //};
            addPropertyWriterFunction("headHairIndex", p => p.headHairIndex != 0, p => p.headHairIndex);
            //Properties["headHairIndex"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.headHairIndex != 0) {
            //        generator.WriteField("headHairIndex", player.headHairIndex);
            //    }
            //};
            addPropertyWriterFunction("facialHairIndex", p => p.facialHairIndex != 0, p => p.facialHairIndex);
            //Properties["facialHairIndex"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.facialHairIndex != 0) {
            //        generator.WriteField("facialHairIndex", player.facialHairIndex);
            //    }
            //};
            addPropertyWriterFunction("playerCharacterName", p => !string.IsNullOrEmpty(p.playerCharacterName), p => p.playerCharacterName);
            //Properties["playerCharacterName"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || !string.IsNullOrEmpty(player.playerCharacterName)) {
            //        generator.WriteField("playerCharacterName", player.playerCharacterName);
            //    }
            //};
            Properties["rawBoneModifiers"] = (player, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("rawBoneModifiers");
                }

                for (int index = 0; index < player.rawBoneModifiers.Length; index++) {
                    if (!writeEmpty && Math.Abs(player.rawBoneModifiers[index]) < 0.01f)
                        continue;
                    if (empty) {
                        empty = false;
                        generator.WriteObjectFieldStart("rawBoneModifiers");
                    }

                    generator.WriteField(PlayerBoneModifierNames.Instance[index], player.rawBoneModifiers[index]);
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            addPropertyWriterFunction("playerSpawnRegionIndex", p => p.playerSpawnRegionIndex != 0, p => p.playerSpawnRegionIndex);
            //Properties["playerSpawnRegionIndex"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.playerSpawnRegionIndex != 0) {
            //        generator.WriteField("playerSpawnRegionIndex", player.playerSpawnRegionIndex);
            //    }
            //};
            addPropertyWriterFunction("totalEngramPoints", p => p.totalEngramPoints != 0, p => p.totalEngramPoints);
            //Properties["totalEngramPoints"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.totalEngramPoints != 0) {
            //        generator.WriteField("totalEngramPoints", player.totalEngramPoints);
            //    }
            //};
            Properties["engramBlueprints"] = (player, generator, context, writeEmpty) => {
                if (!writeEmpty && !player.engramBlueprints.Any())
                    return;
                generator.WriteArrayFieldStart("engramBlueprints");
                foreach (string engramBlueprint in player.engramBlueprints) {
                    generator.WriteValue(engramBlueprint);
                }

                generator.WriteEndArray();
            };
            Properties["numberOfLevelUpPointsApplied"] = (player, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("numberOfLevelUpPointsApplied");
                }

                for (int index = 0; index < player.numberOfLevelUpPointsApplied.Length; index++) {
                    if (!writeEmpty && player.numberOfLevelUpPointsApplied[index] == 0)
                        continue;
                    if (empty) {
                        empty = false;
                        generator.WriteObjectFieldStart("numberOfLevelUpPointsApplied");
                    }

                    generator.WriteField(AttributeNames.Instance[index], player.numberOfLevelUpPointsApplied[index]);
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            Properties["currentStatusValues"] = (player, generator, context, writeEmpty) => {
                bool empty = !writeEmpty;
                if (!empty) {
                    generator.WriteObjectFieldStart("currentStatusValues");
                }

                for (int index = 0; index < player.currentStatusValues.Length; index++) {
                    if (!writeEmpty && Math.Abs(player.currentStatusValues[index]) < 0.01f)
                        continue;
                    if (empty) {
                        empty = false;
                        generator.WriteObjectFieldStart("currentStatusValues");
                    }

                    generator.WriteField(AttributeNames.Instance[index], player.currentStatusValues[index]);
                }

                if (!empty) {
                    generator.WriteEndObject();
                }
            };
            addPropertyWriterFunction("percentageOfHeadHairGrowth", p => Math.Abs(p.percentageOfHeadHairGrowth) > 0f, p => p.percentageOfHeadHairGrowth);
            //Properties["percentageOfHeadHairGrowth"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.percentageOfHeadHairGrowth) > 0f) {
            //        generator.WriteField("percentageOfHeadHairGrowth", player.percentageOfHeadHairGrowth);
            //    }
            //};
            addPropertyWriterFunction("percentageOfFacialHairGrowth", p => Math.Abs(p.percentageOfFacialHairGrowth) > 0f, p => p.percentageOfFacialHairGrowth);
            //Properties["percentageOfFacialHairGrowth"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.percentageOfFacialHairGrowth) > 0f) {
            //        generator.WriteField("percentageOfFacialHairGrowth", player.percentageOfFacialHairGrowth);
            //    }
            //};
            Properties["myInventoryComponent"] = (player, generator, context, writeEmpty) => {
                if (player.inventory != null) {
                    generator.WriteField("myInventoryComponent", player.inventory.Id);
                } else if (writeEmpty) {
                    generator.WriteNullField("myInventoryComponent");
                }
            };
            Properties["location"] = (player, generator, context, writeEmpty) => {
                if (!writeEmpty && player.location == null)
                    return;
                if (player.location == null) {
                    generator.WriteNullField("location");
                } else {
                    generator.WriteObjectFieldStart("location");
                    generator.WriteField("x", player.location.X);
                    generator.WriteField("y", player.location.Y);
                    generator.WriteField("z", player.location.Z);
                    if (context.MapData != null) {
                        generator.WriteField("lat", context.MapData.CalculateLat(player.location.Y));
                        generator.WriteField("lon", context.MapData.CalculateLon(player.location.X));
                    }

                    generator.WriteEndObject();
                }
            };
            addPropertyWriterFunction("characterLevel", p => p.characterLevel != 0, p => p.characterLevel);
            //Properties["characterLevel"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || player.characterLevel != 0) {
            //        generator.WriteField("characterLevel", player.characterLevel);
            //    }
            //};
            addPropertyWriterFunction("experiencePoints", p => Math.Abs(p.experiencePoints) > 0f, p => p.experiencePoints);
            //Properties["experiencePoints"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.experiencePoints) > 0f) {
            //        generator.WriteField("experiencePoints", player.experiencePoints);
            //    }
            //};
            addPropertyWriterFunction("lastHypothermalCharacterInsulationValue", p => Math.Abs(p.lastHypothermalCharacterInsulationValue) > 0f, p => p.lastHypothermalCharacterInsulationValue);
            //Properties["lastHypothermalCharacterInsulationValue"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.lastHypothermalCharacterInsulationValue) > 0f) {
            //        generator.WriteField("lastHypothermalCharacterInsulationValue", player.lastHypothermalCharacterInsulationValue);
            //    }
            //};
            addPropertyWriterFunction("lastHyperthermalCharacterInsulationValue", p => Math.Abs(p.lastHyperthermalCharacterInsulationValue) > 0f, p => p.lastHyperthermalCharacterInsulationValue);
            //Properties["lastHyperthermalCharacterInsulationValue"] = (player, generator, context, writeEmpty) => {
            //    if (writeEmpty || Math.Abs(player.lastHyperthermalCharacterInsulationValue) > 0f) {
            //        generator.WriteField("lastHyperthermalCharacterInsulationValue", player.lastHyperthermalCharacterInsulationValue);
            //    }
            //};

            propertiesList = new List<WriterFunction<Player>>(Properties.Values);

            propertiesListPrivacy = Properties.Keys
                    .Where(key => key != "steamId" && key != "savedNetworkAddress")
                    .Select(key => Properties[key])
                    .ToList();
        }

        public void WriteAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool noPrivacy) {
            if (noPrivacy) {
                foreach (WriterFunction<Player> writer in propertiesList) {
                    writer(this, generator, context, writeEmpty);
                }
            } else {
                foreach (WriterFunction<Player> writer in propertiesListPrivacy) {
                    writer(this, generator, context, writeEmpty);
                }
            }
        }

        public void WriteInventory(JsonTextWriter generator, IDataContext context, bool writeEmpty, bool inventorySummary) {
            if (inventory == null)
                return;
            generator.WritePropertyName("inventory");
            new Inventory(inventory).writeInventory(generator, context, writeEmpty, inventorySummary);
        }
    }

}
