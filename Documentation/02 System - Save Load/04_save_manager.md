# Part 2: SaveManager

## Step 2.1: SaveManager Singleton

Create `Assets/Scripts/Core/SaveSystem/SaveManager.cs`:

```csharp
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;

namespace Game.Core.SaveSystem {

    public class SaveManager {

        // MARK: - Singleton

        private static SaveManager _instance;

        public static SaveManager instance {
            get {
                _instance ??= new SaveManager();
                return _instance;
            }
        }

        // MARK: - Properties

        private string saveFolderPath;

        // MARK: - Events

        public event Action<int> onSaveCompleted;

        public event Action<int> onLoadCompleted;

        public event Action<int, string> onSaveError;

        public event Action<int, string> onLoadError;

        // MARK: - Constructor

        private SaveManager() {
            saveFolderPath = Path.Combine(Application.persistentDataPath, SaveConstants.SAVE_FOLDER);
            ensureSaveFolderExists();
        }

        // MARK: - Public Methods

        public List<SaveSlotInfo> getSaveSlots() {
            List<SaveSlotInfo> slots = new();

            for (int i = 0; i < SaveConstants.MANUAL_SLOT_COUNT; i++) {
                slots.Add(getSlotInfo(i));
            }

            return slots;
        }

        public SaveSlotInfo getSlotInfo(int slotIndex) {
            string filePath = getFilePath(slotIndex);

            if (!File.Exists(filePath)) {
                return SaveSlotInfo.CreateEmpty(slotIndex);
            }

            try {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return SaveSlotInfo.Create(slotIndex, data);
            } catch (Exception e) {
                Debug.LogWarning($"Failed to read save slot {slotIndex}: {e.Message}");
                return SaveSlotInfo.CreateEmpty(slotIndex);
            }
        }

        public bool save(GameState state, int slotIndex) {
            try {
                SaveData data = new SaveData(state, slotIndex);
                string json = JsonUtility.ToJson(data, true);
                string filePath = getFilePath(slotIndex);

                File.WriteAllText(filePath, json);

                Debug.Log($"Game saved to slot {slotIndex}");
                onSaveCompleted?.Invoke(slotIndex);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Failed to save game: {e.Message}");
                onSaveError?.Invoke(slotIndex, e.Message);
                return false;
            }
        }

        public SaveData load(int slotIndex) {
            string filePath = getFilePath(slotIndex);

            if (!File.Exists(filePath)) {
                Debug.LogWarning($"Save file not found: {filePath}");
                onLoadError?.Invoke(slotIndex, "Save file not found");
                return null;
            }

            try {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data.needsMigration()) {
                    data = VersionMigrator.migrate(data);
                }

                Debug.Log($"Game loaded from slot {slotIndex}");
                onLoadCompleted?.Invoke(slotIndex);
                return data;
            } catch (Exception e) {
                Debug.LogError($"Failed to load game: {e.Message}");
                onLoadError?.Invoke(slotIndex, e.Message);
                return null;
            }
        }

        public bool delete(int slotIndex) {
            string filePath = getFilePath(slotIndex);

            if (!File.Exists(filePath)) {
                return false;
            }

            try {
                File.Delete(filePath);
                Debug.Log($"Deleted save slot {slotIndex}");
                return true;
            } catch (Exception e) {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        public bool slotExists(int slotIndex) {
            return File.Exists(getFilePath(slotIndex));
        }

        public bool autoSave(GameState state) {
            return save(state, SaveConstants.AUTOSAVE_SLOT_INDEX);
        }

        public SaveData loadAutoSave() {
            return load(SaveConstants.AUTOSAVE_SLOT_INDEX);
        }

        public bool autoSaveExists() {
            return slotExists(SaveConstants.AUTOSAVE_SLOT_INDEX);
        }

        // MARK: - Private Methods

        private void ensureSaveFolderExists() {
            if (!Directory.Exists(saveFolderPath)) {
                Directory.CreateDirectory(saveFolderPath);
            }
        }

        private string getFilePath(int slotIndex) {
            string filename = slotIndex == SaveConstants.AUTOSAVE_SLOT_INDEX
                ? SaveConstants.AUTOSAVE_FILENAME
                : $"{SaveConstants.SAVE_FILE_PREFIX}{slotIndex}";

            return Path.Combine(saveFolderPath, filename + SaveConstants.SAVE_FILE_EXTENSION);
        }

    }

}
```

## Key Features

### Singleton Pattern (Non-MonoBehaviour)

Unlike `GameStateManager`, `SaveManager` is a pure C# singleton:
- No need for a GameObject in the scene
- Lazy initialization via the property getter
- Simpler to use from anywhere in code

### Events for UI Feedback

```csharp
SaveManager.instance.onSaveCompleted += (slot) => {
    // Show "Game Saved" toast
};

SaveManager.instance.onSaveError += (slot, error) => {
    // Show error dialog
};
```

### Auto-Save Support

The auto-save slot uses index `-1` and a special filename:
```csharp
SaveManager.instance.autoSave(GameStateManager.instance.current);
```

### Pretty-Printed JSON

The `true` parameter in `JsonUtility.ToJson(data, true)` enables pretty-printing for readable save files during development.

**Checkpoint:** Create this file and verify it compiles. You'll see a warning about `VersionMigrator` not existing yet - we'll create that next.
