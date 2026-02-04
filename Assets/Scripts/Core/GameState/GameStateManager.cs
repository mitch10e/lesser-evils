using System;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Core.Game.Events;
using Game.Core.SaveSystem;

namespace Game.Core {

    public class GameStateManager : MonoBehaviour {

        public static GameStateManager instance { get; private set; }

        public GameState current { get; private set; }

        // MARK: - States

        public CampaignState campaign => current.campaign;

        public MaterialState materials => current.materials;

        public ProgressionState progression => current.progression;

        public ResourceState resources => current.resources;

        public TechState technology => current.technology;

        public UnitRosterState roster => current.roster;

        // MARK: - Events

        public event Action<GameState> onStateChanged;

        public event Action<MaterialType, int, int> materialsChanged;

        public event Action<string, bool> missionCompleted;

        public event Action<ResourceType, int, int> resourcesChanged;

        public event Action<int> onGameSaved;

        public event Action<int> onGameLoaded;

        // MARK: - Lifecycle

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        public void Initialize(GameState state = null) {
            current = state ?? new();
            notifyStateChanged("All");

            Debug.Log("GameStateManager initialized");
        }

        public void resetToDefaults() {
            current.reset();
            notifyStateChanged("All");
        }

        public GameState getStateCopy() {
            return current.createDeepCopy();
        }

        public void load(GameState state) {
            current = state;
            notifyStateChanged("All");
        }

        public void startNewGame(FactionType factionType) {
            resetToDefaults();
            campaign.setFaction(factionType);
            createStartingSquad(factionType);

            notifyStateChanged("All");
        }

        // MARK: - Save / Load

        public bool saveGame(int slotIndex) {
            bool success = SaveManager.instance.save(current, slotIndex);

            if (success) {
                onGameSaved?.Invoke(slotIndex);
            }

            return success;
        }

        public bool loadGame(int slotIndex) {
            SaveData data = SaveManager.instance.load(slotIndex);

            if (data == null) {
                return false;
            }

            current = data.gameState;
            notifyStateChanged("All");
            onGameLoaded?.Invoke(slotIndex);

            return true;
        }

        public bool autosave() {
            return SaveManager.instance.autoSave(current);
        }

        public bool loadAutosave() {
            SaveData data = SaveManager.instance.loadAutoSave();

            if (data == null) {
                return false;
            }

            current = data.gameState;
            notifyStateChanged("All");
            onGameLoaded?.Invoke(SaveConstants.AUTOSAVE_SLOT_INDEX);

            return true;
        }

        public bool hasSaveGame(int slotIndex) {
            return SaveManager.instance.slotExists(slotIndex);
        }

        public bool hasAutoSave() {
            return SaveManager.instance.autoSaveExists();
        }

        public bool deleteSave(int slotIndex) {
            return SaveManager.instance.delete(slotIndex);
        }

        // MARK: - Private Methods

        private void notifyStateChanged(string system) {
            onStateChanged?.Invoke(current);
            EventBus.publish(new StateChangedEvent { changedSystem = system });
        }

        private void createStartingSquad(FactionType faction) {
            string prefix = faction.ToString().Substring(0, 3);
            roster.add(UnitData.CreateDefault($"{prefix}_001.debug", $"Recruit 1"));
            roster.add(UnitData.CreateDefault($"{prefix}_002.debug", $"Recruit 2"));
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
        }

    }

}

