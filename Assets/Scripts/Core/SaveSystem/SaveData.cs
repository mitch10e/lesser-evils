using System;
using Game.Core.Data;

namespace Game.Core.SaveSystem {

    [Serializable]
    public class SaveData {

        public int saveVersion;

        public string timestamp;

        public int slotIndex;

        public GameState gameState;

        public SaveData() {
            saveVersion = SaveConstants.CURRENT_SAVE_VERSION;
            timestamp = createTimestamp();
            slotIndex = 0;
            gameState = new();
        }

        public SaveData(GameState state, int slot) {
            saveVersion = SaveConstants.CURRENT_SAVE_VERSION;
            timestamp = createTimestamp();
            slotIndex = slot;
            gameState = state.createDeepCopy();
        }

        private string createTimestamp() {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public bool needsMigration() {
            return saveVersion < SaveConstants.CURRENT_SAVE_VERSION;
        }

    }

}
