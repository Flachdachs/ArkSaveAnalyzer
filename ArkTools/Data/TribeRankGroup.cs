using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkTools.Data {
    public class TribeRankGroup {

        public readonly string rankGroupName;

        public readonly byte rankGroupRank;

        public readonly byte inventoryRank;

        public readonly byte structureActivationRank;

        public readonly byte newStructureActivationRank;

        public readonly byte newStructureInventoryRank;

        public readonly byte petOrderRank;

        public readonly byte petRidingRank;

        public readonly byte inviteToGroupRank;

        public readonly byte maxPromotionGroupRank;

        public readonly byte maxDemotionGroupRank;

        public readonly byte maxBanishmentGroupRank;

        public readonly byte numInvitesRemaining;

        public readonly bool preventStructureDemolish;

        public readonly bool preventStructureAttachment;

        public readonly bool preventStructureBuildInRange;

        public readonly bool preventUnclaiming;

        public readonly bool allowInvites;

        public readonly bool limitInvites;

        public readonly bool allowDemotions;

        public readonly bool allowPromotions;

        public readonly bool allowBanishments;

        public readonly bool defaultRank;

        public TribeRankGroup(IPropertyContainer tribeRankGroup) {
            rankGroupName = tribeRankGroup.GetPropertyValue<string>("RankGroupName", defaultValue: string.Empty);

            rankGroupRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("RankGroupRank")?.ByteValue ?? 0;
            inventoryRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("InventoryRank")?.ByteValue ?? 0;
            structureActivationRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("StructureActivationRank")?.ByteValue ?? 0;
            newStructureActivationRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("NewStructureActivationRank")?.ByteValue ?? 0;
            newStructureInventoryRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("NewStructureInventoryRank")?.ByteValue ?? 0;
            petOrderRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("PetOrderRank")?.ByteValue ?? 0;
            petRidingRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("PetRidingRank")?.ByteValue ?? 0;
            inviteToGroupRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("InviteToGroupRank")?.ByteValue ?? 0;
            maxPromotionGroupRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("MaxPromotionGroupRank")?.ByteValue ?? 0;
            maxDemotionGroupRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("MaxDemotionGroupRank")?.ByteValue ?? 0;
            maxBanishmentGroupRank = tribeRankGroup.GetPropertyValue<ArkByteValue>("MaxBanishmentGroupRank")?.ByteValue ?? 0;
            numInvitesRemaining = tribeRankGroup.GetPropertyValue<ArkByteValue>("NumInvitesRemaining")?.ByteValue ?? 0;

            preventStructureDemolish = tribeRankGroup.GetPropertyValue<bool>("bPreventStructureDemolish");
            preventStructureAttachment = tribeRankGroup.GetPropertyValue<bool>("bPreventStructureAttachment");
            preventStructureBuildInRange = tribeRankGroup.GetPropertyValue<bool>("bPreventStructureBuildInRange");
            preventUnclaiming = tribeRankGroup.GetPropertyValue<bool>("bPreventUnclaiming");
            allowInvites = tribeRankGroup.GetPropertyValue<bool>("bAllowInvites");
            limitInvites = tribeRankGroup.GetPropertyValue<bool>("bLimitInvites");
            allowDemotions = tribeRankGroup.GetPropertyValue<bool>("bAllowDemotions");
            allowPromotions = tribeRankGroup.GetPropertyValue<bool>("bAllowPromotions");
            allowBanishments = tribeRankGroup.GetPropertyValue<bool>("bAllowBanishments");
            defaultRank = tribeRankGroup.GetPropertyValue<bool>("bDefaultRank");
        }

    }
}
