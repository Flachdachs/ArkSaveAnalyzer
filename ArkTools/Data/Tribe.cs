using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Structs;

namespace ArkTools.Data {

    public class Tribe {
        public readonly string tribeName;

        public readonly int ownerPlayerDataId;

        public readonly int tribeId;

        public readonly List<string> membersPlayerName = new List<string>();

        public readonly List<int> membersPlayerDataId = new List<int>();

        public readonly List<int> tribeAdmins = new List<int>();

        public readonly List<byte> membersRankGroups = new List<byte>();

        public readonly bool setGovernment;

        public readonly int tribeGovernPINCode;

        public readonly int tribeGovernDinoOwnership;

        public readonly int tribeGovernStructureOwnership;

        public readonly int tribeGovernDinoTaming;

        public readonly int tribeGovernDinoUnclaimAdminOnly;

        public readonly List<string> tribeLog = new List<string>();

        public readonly int logIndex;

        public readonly List<TribeRankGroup> tribeRankGroups = new List<TribeRankGroup>();

        public Tribe(string path, ReadingOptions ro) {
            ArkTribe tribe = new ArkTribe().ReadBinary<ArkTribe>(path, ro);

            StructPropertyList tribeData = tribe.GetPropertyValue<IStruct, StructPropertyList>("TribeData");

            tribeName = tribeData.GetPropertyValue<string>("TribeName", defaultValue: string.Empty);
            ownerPlayerDataId = tribeData.GetPropertyValue<int>("OwnerPlayerDataID");
            tribeId = tribeData.GetPropertyValue<int>("TribeID");

            ArkArrayString membersNames = tribeData.GetPropertyValue<IArkArray, ArkArrayString>("MembersPlayerName");
            if (membersNames != null) {
                membersPlayerName.AddRange(membersNames);
                foreach (string memberName in membersNames) {
                    membersPlayerName.Add(memberName);
                }
            }

            ArkArrayUInt32 membersData = tribeData.GetPropertyValue<IArkArray, ArkArrayUInt32>("MembersPlayerDataID");
            if (membersData != null) {
                foreach (int memberDataId in membersData) {
                    membersPlayerDataId.Add(memberDataId);
                }
            }

            ArkArrayUInt32 tribeAdminIds = tribeData.GetPropertyValue<IArkArray, ArkArrayUInt32>("TribeAdmins");
            if (tribeAdminIds != null) {
                foreach (int tribeAdmin in tribeAdminIds) {
                    tribeAdmins.Add(tribeAdmin);
                }
            }

            ArkArrayInt8 memberRankGroups = tribeData.GetPropertyValue<IArkArray, ArkArrayInt8>("MembersRankGroups");
            if (memberRankGroups != null) {
                foreach (byte memberRankGroup in memberRankGroups) {
                    membersRankGroups.Add(memberRankGroup);
                }
            }

            setGovernment = tribeData.GetPropertyValue<bool>("SetGovernment");

            IPropertyContainer tribeGovernment = tribeData.GetPropertyValue<IStruct, IPropertyContainer>("TribeGovernment");
            if (tribeGovernment != null) {
                tribeGovernPINCode = tribeGovernment.GetPropertyValue<int>("TribeGovern_PINCode");
                tribeGovernDinoOwnership = tribeGovernment.GetPropertyValue<int>("TribeGovern_DinoOwnership");
                tribeGovernStructureOwnership = tribeGovernment.GetPropertyValue<int>("TribeGovern_StructureOwnership");
                tribeGovernDinoTaming = tribeGovernment.GetPropertyValue<int>("TribeGovern_DinoTaming");
                tribeGovernDinoUnclaimAdminOnly = tribeGovernment.GetPropertyValue<int>("TribeGovern_DinoUnclaimAdminOnly");
            } else {
                tribeGovernDinoOwnership = 1;
                tribeGovernStructureOwnership = 1;
            }

            ArkArrayString logEntrys = tribeData.GetPropertyValue<IArkArray, ArkArrayString>("TribeLog");
            if (logEntrys != null) {
                foreach (string log in logEntrys) {
                    tribeLog.Add(log);
                }
            }

            logIndex = tribeData.GetPropertyValue<int>("LogIndex");

            ArkArrayStruct tribeRankStructs = tribeData.GetPropertyValue<IStruct, ArkArrayStruct>("TribeRankGroups");
            if (tribeRankStructs != null) {
                foreach (IStruct tribeRankStruct in tribeRankStructs) {
                    tribeRankGroups.Add(new TribeRankGroup((IPropertyContainer)tribeRankStruct));
                }
            }
        }

        public static readonly SortedDictionary<string, WriterFunction<Tribe>> PROPERTIES;
        public static readonly List<WriterFunction<Tribe>> PROPERTIES_LIST;

        static Tribe() {
            PROPERTIES = new SortedDictionary<string, WriterFunction<Tribe>>();
            /**
             * Tribe Properties
             */
            PROPERTIES.Add("name", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || !string.IsNullOrEmpty(tribe.tribeName)) {
                    generator.WriteField("name", tribe.tribeName);
                }
            });
            PROPERTIES.Add("ownerPlayerId", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.ownerPlayerDataId != 0) {
                    generator.WriteField("ownerPlayerId", tribe.ownerPlayerDataId);
                }
            });
            PROPERTIES.Add("ownerPlayerName", (tribe, generator, context, writeEmpty) => {
                if (tribe.ownerPlayerDataId != 0) {
                    int index = tribe.membersPlayerDataId.IndexOf(tribe.ownerPlayerDataId);
                    if (index > -1) {
                        generator.WriteField("ownerPlayerName", tribe.membersPlayerName[index]);
                    } else {
                        generator.WriteNullField("ownerPlayerName");
                    }
                } else if (writeEmpty) {
                    generator.WriteNullField("ownerPlayerName");
                }
            });
            PROPERTIES.Add("tribeId", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeId != 0) {
                    generator.WriteField("tribeId", tribe.tribeId);
                }
            });
            PROPERTIES.Add("memberNames", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.membersPlayerName.Any()) {
                    generator.WriteArrayFieldStart("memberNames");
                    foreach (string value in tribe.membersPlayerName) {
                        generator.WriteValue(value);
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("memberIds", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.membersPlayerDataId.Any()) {
                    generator.WriteArrayFieldStart("memberIds");
                    foreach (int value in tribe.membersPlayerDataId) {
                        generator.WriteValue(value);
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("tribeAdminNames", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeAdmins.Any()) {
                    generator.WriteArrayFieldStart("tribeAdminNames");
                    foreach (int value in tribe.tribeAdmins) {
                        int index = tribe.membersPlayerDataId.IndexOf(value);
                        if (index > -1) {
                            generator.WriteValue(tribe.membersPlayerName[index]);
                        } else {
                            generator.WriteNull();
                        }
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("tribeAdminIds", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeAdmins.Any()) {
                    generator.WriteArrayFieldStart("tribeAdminIds");
                    foreach (int value in tribe.tribeAdmins) {
                        generator.WriteValue(value);
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("membersRankGroups", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.membersRankGroups.Any()) {
                    generator.WriteArrayFieldStart("membersRankGroups");
                    foreach (byte value in tribe.membersRankGroups) {
                        generator.WriteValue(value);
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("setGovernment", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || !tribe.setGovernment) {
                    generator.WriteField("setGovernment", tribe.setGovernment);
                }
            });
            PROPERTIES.Add("tribeGovernPINCode", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeGovernPINCode != 0) {
                    generator.WriteField("tribeGovernPINCode", tribe.tribeGovernPINCode);
                }
            });
            PROPERTIES.Add("tribeGovernDinoOwnership", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeGovernDinoOwnership != 0) {
                    generator.WriteField("tribeGovernDinoOwnership", tribe.tribeGovernDinoOwnership);
                }
            });
            PROPERTIES.Add("tribeGovernStructureOwnership", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeGovernStructureOwnership != 0) {
                    generator.WriteField("tribeGovernStructureOwnership", tribe.tribeGovernStructureOwnership);
                }
            });
            PROPERTIES.Add("tribeGovernDinoTaming", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeGovernDinoTaming != 0) {
                    generator.WriteField("tribeGovernDinoTaming", tribe.tribeGovernDinoTaming);
                }
            });
            PROPERTIES.Add("tribeGovernDinoUnclaimAdminOnly", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeGovernDinoUnclaimAdminOnly != 0) {
                    generator.WriteField("tribeGovernDinoUnclaimAdminOnly", tribe.tribeGovernDinoUnclaimAdminOnly);
                }
            });
            PROPERTIES.Add("tribeLog", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeLog.Any()) {
                    generator.WriteArrayFieldStart("tribeLog");
                    foreach (string value in tribe.tribeLog) {
                        generator.WriteValue(value);
                    }

                    generator.WriteEndArray();
                }
            });
            PROPERTIES.Add("logIndex", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.logIndex != 0) {
                    generator.WriteField("logIndex", tribe.logIndex);
                }
            });
            PROPERTIES.Add("tribeRankGroups", (tribe, generator, context, writeEmpty) => {
                if (writeEmpty || tribe.tribeRankGroups.Any()) {
                    generator.WriteArrayFieldStart("tribeRankGroups");
                    foreach (TribeRankGroup group in tribe.tribeRankGroups) {
                        generator.WriteStartObject();
                        if (writeEmpty || !string.IsNullOrEmpty(group.rankGroupName)) {
                            generator.WriteField("rankGroupName", group.rankGroupName);
                        }

                        if (writeEmpty || group.rankGroupRank != 0) {
                            generator.WriteField("rankGroupRank", group.rankGroupRank);
                        }

                        if (writeEmpty || group.inventoryRank != 0) {
                            generator.WriteField("inventoryRank", group.inventoryRank);
                        }

                        if (writeEmpty || group.structureActivationRank != 0) {
                            generator.WriteField("structureActivationRank", group.structureActivationRank);
                        }

                        if (writeEmpty || group.newStructureActivationRank != 0) {
                            generator.WriteField("newStructureActivationRank", group.newStructureActivationRank);
                        }

                        if (writeEmpty || group.newStructureInventoryRank != 0) {
                            generator.WriteField("newStructureInventoryRank", group.newStructureInventoryRank);
                        }

                        if (writeEmpty || group.petOrderRank != 0) {
                            generator.WriteField("petOrderRank", group.petOrderRank);
                        }

                        if (writeEmpty || group.petRidingRank != 0) {
                            generator.WriteField("petRidingRank", group.petRidingRank);
                        }

                        if (writeEmpty || group.inviteToGroupRank != 0) {
                            generator.WriteField("inviteToGroupRank", group.inviteToGroupRank);
                        }

                        if (writeEmpty || group.maxPromotionGroupRank != 0) {
                            generator.WriteField("maxPromotionGroupRank", group.maxPromotionGroupRank);
                        }

                        if (writeEmpty || group.maxDemotionGroupRank != 0) {
                            generator.WriteField("maxDemotionGroupRank", group.maxDemotionGroupRank);
                        }

                        if (writeEmpty || group.maxBanishmentGroupRank != 0) {
                            generator.WriteField("maxBanishmentGroupRank", group.maxBanishmentGroupRank);
                        }

                        if (writeEmpty || group.numInvitesRemaining != 0) {
                            generator.WriteField("numInvitesRemaining", group.numInvitesRemaining);
                        }

                        if (writeEmpty || group.preventStructureDemolish) {
                            generator.WriteField("preventStructureDemolish", group.preventStructureDemolish);
                        }

                        if (writeEmpty || group.preventStructureAttachment) {
                            generator.WriteField("preventStructureAttachment", group.preventStructureAttachment);
                        }

                        if (writeEmpty || group.preventStructureBuildInRange) {
                            generator.WriteField("preventStructureBuildInRange", group.preventStructureBuildInRange);
                        }

                        if (writeEmpty || group.preventUnclaiming) {
                            generator.WriteField("preventUnclaiming", group.preventUnclaiming);
                        }

                        if (writeEmpty || group.allowInvites) {
                            generator.WriteField("allowInvites", group.allowInvites);
                        }

                        if (writeEmpty || group.limitInvites) {
                            generator.WriteField("limitInvites", group.limitInvites);
                        }

                        if (writeEmpty || group.allowDemotions) {
                            generator.WriteField("allowDemotions", group.allowDemotions);
                        }

                        if (writeEmpty || group.allowPromotions) {
                            generator.WriteField("allowPromotions", group.allowPromotions);
                        }

                        if (writeEmpty || group.allowBanishments) {
                            generator.WriteField("allowBanishments", group.allowBanishments);
                        }

                        if (writeEmpty || group.defaultRank) {
                            generator.WriteField("defaultRank", group.defaultRank);
                        }

                        generator.WriteEndObject();
                    }

                    generator.WriteEndArray();
                }
            });

            PROPERTIES_LIST = new List<WriterFunction<Tribe>>(PROPERTIES.Values);
        }

        public void writeAllProperties(JsonTextWriter generator, IDataContext context, bool writeEmpty) {
            foreach (WriterFunction<Tribe> writer in PROPERTIES_LIST) {
                writer(this, generator, context, writeEmpty);
            }
        }
    }

}
