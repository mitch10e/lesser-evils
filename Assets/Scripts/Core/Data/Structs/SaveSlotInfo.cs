using System;
using Game.Core.SaveSystem;

namespace Game.Core.Data {

    [Serializable]
    public struct SaveSlotInfo {

        public int slotIndex;

        public bool isEmpty;

        public string timestamp;

        public string campaignInfo;

        public int playTime;

        public static SaveSlotInfo CreateEmpty(int slot) {
            return new SaveSlotInfo {
                slotIndex = slot,
                isEmpty = true,
                timestamp = "",
                campaignInfo = "",
                playTime = 0
            };
        }

        public static SaveSlotInfo Create(int slot, SaveData data) {
            return new SaveSlotInfo {
                slotIndex = slot,
                isEmpty = false,
                timestamp = data.timestamp,
                campaignInfo = "Campaign",
                playTime = data.gameState.campaign.elapsedTime
            };
        }

    }

}
