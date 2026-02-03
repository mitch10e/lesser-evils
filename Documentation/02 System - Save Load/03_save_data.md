# Part 1: SaveData and Constants

## Step 1.1: Save Constants

Create `Assets/Scripts/Core/Data/Constants/SaveConstants.cs`:

```csharp
namespace Game.Core.Data {

    public static class SaveConstants {

        public const int MANUAL_SLOT_COUNT = 3;

        public const int AUTOSAVE_SLOT_INDEX = -1;

        public const string SAVE_FOLDER = "Saves";

        public const string SAVE_FILE_PREFIX = "slot_";

        public const string AUTOSAVE_FILENAME = "autosave";

        public const string SAVE_FILE_EXTENSION = ".json";

        public const int CURRENT_SAVE_VERSION = 1;

    }

}
```

**Design Note:** Constants are centralized for easy tuning. `AUTOSAVE_SLOT_INDEX = -1` distinguishes auto-saves from manual slots.

## Step 1.2: SaveSlotInfo Struct

Create `Assets/Scripts/Core/Data/Structs/SaveSlotInfo.cs`:

```csharp
using System;

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
                campaignInfo = $"Act {(int)data.gameState.campaign.currentAct + 1} - {data.gameState.campaign.currentFaction}",
                playTime = data.gameState.campaign.elapsedTime
            };
        }

    }

}
```

**Design Note:** `SaveSlotInfo` is a lightweight struct for displaying save slots in the UI without loading the full game state. The `campaignInfo` provides a quick summary for the player.

## Step 1.3: SaveData Wrapper Class

Create `Assets/Scripts/Core/SaveSystem/SaveData.cs`:

```csharp
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
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            slotIndex = 0;
            gameState = new GameState();
        }

        public SaveData(GameState state, int slot) {
            saveVersion = SaveConstants.CURRENT_SAVE_VERSION;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            slotIndex = slot;
            gameState = state.createDeepCopy();
        }

        public bool needsMigration() {
            return saveVersion < SaveConstants.CURRENT_SAVE_VERSION;
        }

    }

}
```

**Design Note:**
- `saveVersion` tracks the save file format version (independent of `GameState.version`)
- `timestamp` is human-readable for display in save slot UI
- `createDeepCopy()` ensures we don't save a reference to the live state
- `needsMigration()` checks if the save file is from an older version

## Why Two Version Numbers?

| Field | Purpose |
|-------|---------|
| `SaveData.saveVersion` | Tracks save **file format** changes (new metadata fields) |
| `GameState.version` | Tracks game **data structure** changes (new state fields) |

Both may need migration logic, but they change independently.

**Checkpoint:** Create these three files and verify they compile. You should see no errors before proceeding.
