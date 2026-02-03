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

        private readonly string saveFolderPath;

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

        public SaveSlotInfo getSlotInfo(int index) {
            string filepath = getFilePath(index);

            if (!File.Exists(filepath)) {
                return SaveSlotInfo.CreateEmpty(index);
            }

            try {
                string json = File.ReadAllText(filepath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return SaveSlotInfo.Create(index, data);
            } catch (Exception e) {
                Debug.LogWarning($"Failed to read save slot {index}: {e.Message}");
                return SaveSlotInfo.CreateEmpty(index);
            }
        }

        public bool save(GameState state, int index) {
            try {
                SaveData data = new SaveData(state, index);
                string json = JsonUtility.ToJson(data, true);
                string filepath = getFilePath(index);

                File.WriteAllText(filepath, json);

                Debug.Log($"Game saved to slot {index}");
                onSaveCompleted?.Invoke(index);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Failed to save game: {e.Message}");
                onSaveError?.Invoke(index, e.Message);
                return false;
            }
        }

        public SaveData load(int index) {
            string filePath = getFilePath(index);

            if (!File.Exists(filePath)) {
                Debug.LogWarning($"Save file not found: {filePath}");
                onLoadError?.Invoke(index, "Save file not found");
                return null;
            }

            try {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data.needsMigration()) {
                    data = VersionMigrator.migrate(data);
                }

                Debug.Log($"Game loaded from slot {index}");
                onLoadCompleted?.Invoke(index);
                return data;
            } catch (Exception e) {
                Debug.LogError($"Failed to load game: {e.Message}");
                onLoadError?.Invoke(index, e.Message);
                return null;
            }
        }

        public bool delete(int index) {
            string filePath = getFilePath(index);

            if (!File.Exists(filePath)) {
                return false;
            }

            try {
                File.Delete(filePath);
                Debug.Log($"Deleted save slot {index}");
                return true;
            } catch (Exception e) {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        public bool slotExists(int index) {
            return File.Exists(getFilePath(index));
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

        private string getFilePath(int index) {
            string filename = index == SaveConstants.AUTOSAVE_SLOT_INDEX
                ? SaveConstants.AUTOSAVE_FILENAME
                : $"{SaveConstants.SAVE_FILE_PREFIX}{index}";

            return Path.Combine(saveFolderPath, filename + SaveConstants.SAVE_FILE_EXTENSION);
        }

    }

}
