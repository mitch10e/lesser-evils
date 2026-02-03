# Part 5: Integration with GameStateManager

Now we'll add save/load methods to `GameStateManager` to provide a clean API for the rest of the game.

## Step 5.1: Update GameStateManager

Add these methods to `Assets/Scripts/Core/GameState/GameStateManager.cs`:

```csharp
using System;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Game.Core.SaveSystem;
using Core.Game.Events;

namespace Game.Core {

    public class GameStateManager : MonoBehaviour {

        public static GameStateManager instance { get; private set; }

        public GameState current { get; private set; }

        // MARK: - States

        public CampaignState campaign => current.campaign;

        public MaterialState materials => current.materials;

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

        // MARK: - Save/Load Methods

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

        public bool autoSave() {
            return SaveManager.instance.autoSave(current);
        }

        public bool loadAutoSave() {
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
```

## API Usage Examples

### Save Game Menu

```csharp
public class SaveMenuUI : MonoBehaviour {

    private void Start() {
        GameStateManager.instance.onGameSaved += OnSaveComplete;
    }

    public void OnSaveSlotClicked(int slotIndex) {
        bool success = GameStateManager.instance.saveGame(slotIndex);

        if (!success) {
            ShowErrorDialog("Failed to save game");
        }
    }

    private void OnSaveComplete(int slot) {
        ShowToast($"Game saved to slot {slot + 1}");
        RefreshSlotDisplay();
    }

}
```

### Load Game Menu

```csharp
public class LoadMenuUI : MonoBehaviour {

    private void Start() {
        DisplaySaveSlots();
    }

    private void DisplaySaveSlots() {
        var slots = SaveManager.instance.getSaveSlots();

        foreach (var slot in slots) {
            if (slot.isEmpty) {
                DisplayEmptySlot(slot.slotIndex);
            } else {
                DisplayFilledSlot(slot);
            }
        }
    }

    public void OnLoadSlotClicked(int slotIndex) {
        if (GameStateManager.instance.loadGame(slotIndex)) {
            SceneManager.LoadScene("GameScene");
        } else {
            ShowErrorDialog("Failed to load save");
        }
    }

}
```

### Auto-Save on Mission Complete

```csharp
public class MissionManager : MonoBehaviour {

    public void CompleteMission(string missionID, bool success) {
        // Update game state...

        // Auto-save after each mission
        GameStateManager.instance.autoSave();
    }

}
```

### Continue Button (Main Menu)

```csharp
public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button continueButton;

    private void Start() {
        // Only show Continue if there's an auto-save
        continueButton.gameObject.SetActive(GameStateManager.instance.hasAutoSave());
    }

    public void OnContinueClicked() {
        if (GameStateManager.instance.loadAutoSave()) {
            SceneManager.LoadScene("GameScene");
        }
    }

}
```

## Event Flow

```
User clicks Save
       │
       ▼
GameStateManager.saveGame(slot)
       │
       ▼
SaveManager.save(current, slot)
       │
       ├─► Success: onSaveCompleted event
       │              │
       │              ▼
       │   GameStateManager.onGameSaved event
       │              │
       │              ▼
       │        UI updates
       │
       └─► Failure: onSaveError event
                     │
                     ▼
               UI shows error
```

**Checkpoint:** Update your `GameStateManager.cs` with these new methods. The save/load system is now fully integrated!
