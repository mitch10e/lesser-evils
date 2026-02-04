# Part 4: Integration and Strategic Layer

This system follows the XCOM 2 model: **missions pause time**, while the **strategic layer advances time through player choices**.

## Time Flow Model

```
┌─────────────────────────────────────────────────────────┐
│                    TACTICAL LAYER                        │
│                   (In-Mission)                           │
│                                                          │
│   Time is PAUSED - no world threat growth                │
│   Focus purely on combat                                 │
└─────────────────────────────────────────────────────────┘
                         │
                         ▼ Mission Complete
┌─────────────────────────────────────────────────────────┐
│                   STRATEGIC LAYER                        │
│                  (Base/Geoscape)                         │
│                                                          │
│   Player queues actions:                                 │
│   - Research a tech (costs X hours)                      │
│   - Send squad on expedition (costs X hours)             │
│   - Build/upgrade facility (costs X hours)               │
│   - Rest/heal injured units (costs X hours)              │
│   - Wait for mission opportunity (skip time)             │
│                                                          │
│   Player presses ADVANCE → time ticks forward visually   │
│   Progress bars fill, events fire mid-flow               │
│   Player can PAUSE at any time                           │
└─────────────────────────────────────────────────────────┘
```

## Step 4.1: Time Action System

Create `Assets/Scripts/Core/Progression/TimeAction.cs`:

```csharp
using System;

namespace Game.Core.Progression {

    [Serializable]
    public class TimeAction {

        public string id;

        public string displayName;

        public int durationHours;

        public int elapsedHours;

        public TimeActionType actionType;

        public string targetID;

        // MARK: - Properties

        public float progress => durationHours > 0
            ? (float)elapsedHours / durationHours
            : 1f;

        public bool isComplete => elapsedHours >= durationHours;

        public int remainingHours => durationHours - elapsedHours;

        // MARK: - Methods

        /// <summary>
        /// Advances this action by the given hours.
        /// Returns true if the action completed during this tick.
        /// </summary>
        public bool tick(int hours) {
            if (isComplete) return false;

            bool wasPending = !isComplete;
            elapsedHours = Math.Min(elapsedHours + hours, durationHours);
            return wasPending && isComplete;
        }

        // MARK: - Factory Methods

        public static TimeAction createResearch(string techID, string name, int hours) {
            return new TimeAction {
                id = $"research_{techID}",
                displayName = $"Research: {name}",
                durationHours = hours,
                elapsedHours = 0,
                actionType = TimeActionType.Research,
                targetID = techID
            };
        }

        public static TimeAction createHealing(string unitID, string name, int hours) {
            return new TimeAction {
                id = $"heal_{unitID}",
                displayName = $"Healing: {name}",
                durationHours = hours,
                elapsedHours = 0,
                actionType = TimeActionType.Healing,
                targetID = unitID
            };
        }

        public static TimeAction createExpedition(string expeditionID, int hours) {
            return new TimeAction {
                id = $"expedition_{expeditionID}",
                displayName = "Expedition",
                durationHours = hours,
                elapsedHours = 0,
                actionType = TimeActionType.Expedition,
                targetID = expeditionID
            };
        }

        public static TimeAction createBuild(string facilityID, string name, int hours) {
            return new TimeAction {
                id = $"build_{facilityID}",
                displayName = $"Build: {name}",
                durationHours = hours,
                elapsedHours = 0,
                actionType = TimeActionType.Build,
                targetID = facilityID
            };
        }

        public static TimeAction createWait(int hours) {
            return new TimeAction {
                id = "wait",
                displayName = "Wait",
                durationHours = hours,
                elapsedHours = 0,
                actionType = TimeActionType.Wait,
                targetID = null
            };
        }

    }

    public enum TimeActionType {
        Research,
        Healing,
        Expedition,
        Build,
        Wait
    }

}
```

**Key changes from instant model:**
- `TimeAction` is now a `class` instead of a `struct` so active actions can be mutated in-place as time ticks forward
- `elapsedHours` tracks how far along the action is
- `progress` (0.0–1.0) maps directly to a UI progress bar fill
- `tick()` advances the action by a number of hours and returns `true` the moment it completes

## Step 4.2: Strategic Layer Manager

Create `Assets/Scripts/Core/Progression/StrategicLayerManager.cs`:

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Core.Game.Events;

namespace Game.Core.Progression {

    public class StrategicLayerManager : MonoBehaviour {

        public static StrategicLayerManager instance { get; private set; }

        // MARK: - Configuration

        [Header("Time Lapse Settings")]
        [SerializeField] private float secondsPerTick = 0.15f;
        [SerializeField] private int hoursPerTick = 1;

        // MARK: - State

        private List<TimeAction> activeActions = new();
        private bool isInMission = false;
        private bool isTimeLapseRunning = false;
        private bool pauseRequested = false;
        private TimeSpeed currentSpeed = TimeSpeed.Normal;

        // MARK: - Properties

        public IReadOnlyList<TimeAction> currentActions => activeActions;
        public bool isAdvancingTime => isTimeLapseRunning;
        public TimeSpeed speed => currentSpeed;

        // MARK: - Events

        /// <summary>Fired every tick with the hours that just passed.</summary>
        public event Action<int> onTick;

        /// <summary>Fired when a new action is queued.</summary>
        public event Action<TimeAction> onActionStarted;

        /// <summary>Fired mid-lapse when an action's timer runs out.</summary>
        public event Action<TimeAction> onActionCompleted;

        /// <summary>Fired when the time lapse starts.</summary>
        public event Action onTimeLapseStarted;

        /// <summary>Fired when the time lapse stops (all done or paused).</summary>
        public event Action onTimeLapseStopped;

        /// <summary>Fired when a world event triggers mid-lapse.</summary>
        public event Action<string> onWorldEventTriggered;

        // MARK: - Lifecycle

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // MARK: - Mission State

        public void enterMission() {
            if (isTimeLapseRunning) {
                pauseTimeLapse();
            }
            isInMission = true;
        }

        public void exitMission() {
            isInMission = false;
        }

        // MARK: - Queue Actions

        public bool canStartAction(TimeAction action) {
            if (isInMission) return false;

            foreach (var active in activeActions) {
                if (active.actionType == action.actionType && !active.isComplete) {
                    return false;
                }
            }

            return true;
        }

        public void startAction(TimeAction action) {
            if (!canStartAction(action)) return;

            activeActions.Add(action);
            onActionStarted?.Invoke(action);
        }

        public void removeAction(TimeAction action) {
            activeActions.Remove(action);
        }

        // MARK: - Time Lapse Controls

        /// <summary>
        /// Begins the time lapse. Time ticks forward at the configured rate
        /// until the player pauses or a world event interrupts.
        /// Works with or without queued actions (fast-forward idle time).
        /// </summary>
        public void startTimeLapse() {
            if (isInMission || isTimeLapseRunning) return;

            pauseRequested = false;
            StartCoroutine(timeLapseCoroutine());
        }

        /// <summary>
        /// Requests the time lapse to pause after the current tick finishes.
        /// </summary>
        public void pauseTimeLapse() {
            pauseRequested = true;
        }

        /// <summary>
        /// Immediately advances time by a number of hours without visual ticking.
        /// Useful for skipping small gaps or scripted events.
        /// </summary>
        public void advanceTimeImmediate(int hours) {
            if (isInMission) return;

            processTick(hours);
        }

        // MARK: - Tick Loop

        private IEnumerator timeLapseCoroutine() {
            isTimeLapseRunning = true;
            onTimeLapseStarted?.Invoke();

            while (!pauseRequested) {
                processTick(hoursPerTick);

                yield return new WaitForSeconds(secondsPerTick);
            }

            isTimeLapseRunning = false;
            pauseRequested = false;
            onTimeLapseStopped?.Invoke();

            // Clean up completed actions
            activeActions.RemoveAll(a => a.isComplete);
        }

        private void processTick(int hours) {
            var gm = GameStateManager.instance;

            // Advance world time and threat
            gm.progression.advanceTime(hours);

            // Tick each active action and handle completions
            foreach (var action in activeActions) {
                bool justCompleted = action.tick(hours);

                if (justCompleted) {
                    completeAction(action);
                }
            }

            // Check for world events
            checkWorldEvents();

            // Notify listeners
            onTick?.Invoke(hours);
            EventBus.publish(new TimeAdvancedEvent { hoursAdvanced = hours });
        }

        private bool hasPendingActions() {
            foreach (var action in activeActions) {
                if (!action.isComplete) return true;
            }
            return false;
        }

        // MARK: - Action Completion

        private void completeAction(TimeAction action) {
            var gm = GameStateManager.instance;

            switch (action.actionType) {
                case TimeActionType.Research:
                    gm.technology.completeCurrentResearch();
                    break;

                case TimeActionType.Healing:
                    gm.roster.updateStatus(action.targetID, UnitStatus.Active);
                    break;

                case TimeActionType.Expedition:
                    // Handle expedition completion
                    break;

                case TimeActionType.Build:
                    // Handle facility completion
                    break;

                case TimeActionType.Wait:
                    break;
            }

            onActionCompleted?.Invoke(action);
        }

        // MARK: - World Events

        private void checkWorldEvents() {
            var gm = GameStateManager.instance;
            float ratio = DifficultyCalculator.getDifficultyRatio(
                gm.progression,
                gm.roster,
                gm.technology
            );

            string warning = WorldEventTriggers.checkDifficultyWarning(
                ratio,
                gm.campaign.activeStoryFlags
            );

            if (warning != null) {
                gm.campaign.addStoryFlag(warning);
                onWorldEventTriggered?.Invoke(warning);

                // Pause the lapse so the player sees the event
                pauseTimeLapse();
            }
        }

        // MARK: - Speed Control

        public void setSpeed(TimeSpeed speed) {
            currentSpeed = speed;

            switch (speed) {
                case TimeSpeed.Normal:
                    secondsPerTick = 0.15f;
                    hoursPerTick = 1;
                    break;
                case TimeSpeed.Fast:
                    secondsPerTick = 0.08f;
                    hoursPerTick = 2;
                    break;
                case TimeSpeed.Ultra:
                    secondsPerTick = 0.03f;
                    hoursPerTick = 4;
                    break;
            }
        }

        public void cycleSpeed() {
            switch (currentSpeed) {
                case TimeSpeed.Normal:
                    setSpeed(TimeSpeed.Fast);
                    break;
                case TimeSpeed.Fast:
                    setSpeed(TimeSpeed.Ultra);
                    break;
                case TimeSpeed.Ultra:
                    setSpeed(TimeSpeed.Normal);
                    break;
            }
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
        }

    }

    public enum TimeSpeed {
        Normal,
        Fast,
        Ultra
    }

}
```

**How the tick loop works:**

1. Player optionally queues actions (research, heal, build)
2. Player presses "Advance Time" which calls `startTimeLapse()`
3. A coroutine begins ticking at `hoursPerTick` hours every `secondsPerTick` real seconds
4. Each tick: world threat updates, any active action progress bars update via `onTick`
5. When an action completes mid-lapse, `onActionCompleted` fires (UI can show a popup/notification)
6. When a world event triggers, the lapse **auto-pauses** so the player can react
7. The lapse runs **indefinitely** until the player pauses or a world event interrupts — this lets the player fast-forward through idle time when nothing is queued
8. Player can cycle through speed presets at any time, even mid-lapse

**Speed presets (via `TimeSpeed` enum):**

| TimeSpeed | secondsPerTick | hoursPerTick | Feel                          |
|-----------|----------------|--------------|-------------------------------|
| Normal    | 0.15           | 1            | Watch hours tick by            |
| Fast      | 0.08           | 2            | Quick skip, still readable     |
| Ultra     | 0.03           | 4            | Rapid fast-forward             |

Call `cycleSpeed()` to rotate Normal → Fast → Ultra → Normal, or `setSpeed(TimeSpeed.Fast)` directly.

## Step 4.3: Events

Create `Assets/Scripts/Core/Events/TimeAdvancedEvent.cs`:

```csharp
namespace Game.Core.Events {

    public struct TimeAdvancedEvent {

        public int hoursAdvanced;

    }

}
```

## Step 4.4: Update GameStateManager

Add progression property to `GameStateManager`:

```csharp
// Add to existing properties in GameStateManager.cs:

public ProgressionState progression => current.progression;

// Add convenience methods:

public void completeMainMission(string missionID, float progressValue) {
    progression.completeMainMission(missionID, progressValue);
    notifyStateChanged("Progression");
}
```

## Usage Example: Strategic Layer UI with Progress Bars

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Core.Progression;
using Game.Core.GameState;

public class StrategicLayerUI : MonoBehaviour {

    [Header("Controls")]
    [SerializeField] private Button advanceButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speedButton;

    [Header("Display")]
    [SerializeField] private Text threatLevelText;
    [SerializeField] private Text elapsedTimeText;
    [SerializeField] private Text speedText;
    [SerializeField] private Transform actionListParent;
    [SerializeField] private GameObject actionEntryPrefab;

    // Tracks spawned UI entries keyed by action id
    private Dictionary<string, ActionEntryUI> actionEntries = new();

    private void Start() {
        var slm = StrategicLayerManager.instance;

        slm.onTick += onTick;
        slm.onActionStarted += onActionStarted;
        slm.onActionCompleted += onActionCompleted;
        slm.onTimeLapseStarted += onTimeLapseStarted;
        slm.onTimeLapseStopped += onTimeLapseStopped;
        slm.onWorldEventTriggered += onWorldEvent;

        advanceButton.onClick.AddListener(onAdvanceClicked);
        pauseButton.onClick.AddListener(onPauseClicked);
        speedButton.onClick.AddListener(onSpeedClicked);

        pauseButton.gameObject.SetActive(false);
        updateDisplay();
    }

    // MARK: - Button Handlers

    private void onAdvanceClicked() {
        StrategicLayerManager.instance.startTimeLapse();
    }

    private void onPauseClicked() {
        StrategicLayerManager.instance.pauseTimeLapse();
    }

    private void onSpeedClicked() {
        StrategicLayerManager.instance.cycleSpeed();
        updateSpeedDisplay();
    }

    // MARK: - Adding Actions (called by other UI panels)

    public void queueResearch(string techID, string name, int hours) {
        var action = TimeAction.createResearch(techID, name, hours);
        StrategicLayerManager.instance.startAction(action);
    }

    public void queueHealing(string unitID, string name, int hours) {
        var action = TimeAction.createHealing(unitID, name, hours);
        StrategicLayerManager.instance.startAction(action);
    }

    // MARK: - Event Handlers

    private void onTick(int hours) {
        // Update every progress bar each tick
        foreach (var action in StrategicLayerManager.instance.currentActions) {
            if (actionEntries.TryGetValue(action.id, out var entry)) {
                entry.progressBar.fillAmount = action.progress;
                entry.remainingText.text = $"{action.remainingHours}h remaining";
            }
        }

        updateDisplay();
    }

    private void onActionStarted(TimeAction action) {
        // Spawn a new progress bar entry in the UI
        var go = Instantiate(actionEntryPrefab, actionListParent);
        var entry = go.GetComponent<ActionEntryUI>();
        entry.nameText.text = action.displayName;
        entry.progressBar.fillAmount = 0f;
        entry.remainingText.text = $"{action.durationHours}h remaining";

        actionEntries[action.id] = entry;
    }

    private void onActionCompleted(TimeAction action) {
        if (actionEntries.TryGetValue(action.id, out var entry)) {
            entry.progressBar.fillAmount = 1f;
            entry.remainingText.text = "Complete!";
            // Could play a completion animation here
        }
    }

    private void onTimeLapseStarted() {
        advanceButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        speedButton.gameObject.SetActive(true);
    }

    private void onTimeLapseStopped() {
        advanceButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
        speedButton.gameObject.SetActive(false);
    }

    private void onWorldEvent(string eventID) {
        // Show narrative popup — lapse is already paused
        Debug.Log($"World event triggered: {eventID}");
    }

    private void updateDisplay() {
        var progression = GameStateManager.instance.progression;
        threatLevelText.text = $"Threat: {progression.worldThreatLevel:F2}";

        int days = progression.totalElapsedHours / 24;
        int hours = progression.totalElapsedHours % 24;
        elapsedTimeText.text = $"Day {days + 1}, {hours:D2}:00";
    }

    private void updateSpeedDisplay() {
        switch (StrategicLayerManager.instance.speed) {
            case TimeSpeed.Normal:
                speedText.text = "▶";
                break;
            case TimeSpeed.Fast:
                speedText.text = "▶▶";
                break;
            case TimeSpeed.Ultra:
                speedText.text = "▶▶▶";
                break;
        }
    }

}
```

**ActionEntryUI** is a simple component on the prefab:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ActionEntryUI : MonoBehaviour {

    public Text nameText;
    public Image progressBar;  // Image with fillMethod set to Horizontal
    public Text remainingText;

}
```

## The XCOM Loop

```
┌──────────────────┐
│  Mission Board   │◄──────────────────────────┐
│  (Pick mission)  │                           │
└────────┬─────────┘                           │
         │                                     │
         ▼                                     │
┌──────────────────┐                           │
│  TACTICAL COMBAT │                           │
│  (Time frozen)   │                           │
└────────┬─────────┘                           │
         │ Victory/Defeat                      │
         ▼                                     │
┌──────────────────┐                           │
│  Results Screen  │                           │
│  (XP, loot)      │                           │
└────────┬─────────┘                           │
         │                                     │
         ▼                                     │
┌──────────────────────────────────────┐       │
│         STRATEGIC LAYER              │       │
│                                      │       │
│  1. Queue actions:                   │       │
│     [Research: Plasma ──────░░ 0%]   │       │
│     [Heal: Rook ─────░░░░░░░░ 0%]   │       │
│     [Build: Armory ──░░░░░░░░ 0%]   │       │
│                                      │       │
│  2. Press [▶ Advance Time]           │       │
│     Speed: [▶] [▶▶] [▶▶▶]           │       │
│                                      │       │
│  3. Watch time tick forward:         │       │
│     [Research: Plasma ██████░░ 72%]  │       │
│     [Heal: Rook ─────████████ Done!] │       │
│     [Build: Armory ──███░░░░░ 38%]  │       │
│          Day 4, 08:00                │       │
│          Threat: 1.24                │       │
│                                      │       │
│  4. Events pause the lapse:          │       │
│     ⚠ "Enemy faction grows bolder"  │       │
│     [▶ Resume]  [⏹ Stay]            │       │
│                                      │       │
│  5. Nothing queued? Fast-forward:    │       │
│     Hold [▶▶▶] to skip idle time    │       │
│     World events still interrupt     │       │
│                                      │       │
│  6. Ready → pick next mission        │───────┘
└──────────────────────────────────────┘
```

**Checkpoint:** Create these files. The strategic layer now ticks time forward visually, with progress bars that fill as hours pass and events that pause the lapse for player decisions.
