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
│   Time ADVANCES via player actions:                      │
│   - Research a tech (costs X hours)                      │
│   - Send squad on expedition (costs X hours)             │
│   - Build/upgrade facility (costs X hours)               │
│   - Rest/heal injured units (costs X hours)              │
│   - Wait for mission opportunity (skip time)             │
└─────────────────────────────────────────────────────────┘
```

## Step 4.1: Time Action System

Create `Assets/Scripts/Core/Progression/TimeAction.cs`:

```csharp
using System;

namespace Game.Core.Progression {

    [Serializable]
    public struct TimeAction {

        public string id;

        public string displayName;

        public int durationHours;

        public TimeActionType actionType;

        public string targetID;

        public static TimeAction CreateResearch(string techID, string name, int hours) {
            return new TimeAction {
                id = $"research_{techID}",
                displayName = $"Research: {name}",
                durationHours = hours,
                actionType = TimeActionType.Research,
                targetID = techID
            };
        }

        public static TimeAction CreateHealing(string unitID, string name, int hours) {
            return new TimeAction {
                id = $"heal_{unitID}",
                displayName = $"Healing: {name}",
                durationHours = hours,
                actionType = TimeActionType.Healing,
                targetID = unitID
            };
        }

        public static TimeAction CreateExpedition(string expeditionID, int hours) {
            return new TimeAction {
                id = $"expedition_{expeditionID}",
                displayName = "Expedition",
                durationHours = hours,
                actionType = TimeActionType.Expedition,
                targetID = expeditionID
            };
        }

        public static TimeAction CreateBuild(string facilityID, string name, int hours) {
            return new TimeAction {
                id = $"build_{facilityID}",
                displayName = $"Build: {name}",
                durationHours = hours,
                actionType = TimeActionType.Build,
                targetID = facilityID
            };
        }

        public static TimeAction CreateWait(int hours) {
            return new TimeAction {
                id = "wait",
                displayName = "Wait",
                durationHours = hours,
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

## Step 4.2: Strategic Layer Manager

Create `Assets/Scripts/Core/Progression/StrategicLayerManager.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Core.Game.Events;

namespace Game.Core.Progression {

    public class StrategicLayerManager : MonoBehaviour {

        public static StrategicLayerManager instance { get; private set; }

        // MARK: - State

        private List<TimeAction> activeActions = new();

        private bool isInMission = false;

        // MARK: - Events

        public event Action<int> onTimeAdvanced;

        public event Action<TimeAction> onActionStarted;

        public event Action<TimeAction> onActionCompleted;

        public event Action<List<string>> onNewMissionsAvailable;

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
            isInMission = true;
            // Time is frozen during missions
        }

        public void exitMission() {
            isInMission = false;
            // Check for any events that should trigger post-mission
            checkWorldEvents();
        }

        // MARK: - Time Actions

        public bool canStartAction(TimeAction action) {
            if (isInMission) return false;

            // Check if we have an action of this type already running
            foreach (var active in activeActions) {
                if (active.actionType == action.actionType) {
                    return false; // Only one of each type at a time
                }
            }

            return true;
        }

        public void startAction(TimeAction action) {
            if (!canStartAction(action)) return;

            activeActions.Add(action);
            onActionStarted?.Invoke(action);
        }

        public void advanceTime(int hours) {
            if (isInMission) {
                Debug.LogWarning("Cannot advance time during mission");
                return;
            }

            var gm = GameStateManager.instance;

            // Advance world time and threat
            gm.progression.advanceTime(hours);

            // Process active actions
            processActions(hours);

            // Check for new content unlocks
            checkMissionUnlocks();

            // Check for world events
            checkWorldEvents();

            // Notify listeners
            onTimeAdvanced?.Invoke(hours);

            // Publish event bus notification
            EventBus.publish(new TimeAdvancedEvent { hoursAdvanced = hours });
        }

        public void skipToNextEvent() {
            // Find the soonest action completion or mission availability
            int hoursToSkip = findNextEventTime();

            if (hoursToSkip > 0) {
                advanceTime(hoursToSkip);
            }
        }

        // MARK: - Private Methods

        private void processActions(int hours) {
            var gm = GameStateManager.instance;
            List<TimeAction> completedActions = new();

            foreach (var action in activeActions) {
                // Decrement remaining time (simplified - actual impl would track remaining)
                // For now, assume single-step completion
                if (action.durationHours <= hours) {
                    completeAction(action);
                    completedActions.Add(action);
                }
            }

            foreach (var completed in completedActions) {
                activeActions.Remove(completed);
            }
        }

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
                    // Nothing to do
                    break;
            }

            onActionCompleted?.Invoke(action);
        }

        private void checkMissionUnlocks() {
            // This would check against a mission database
            // For now, just demonstrate the pattern
            var gm = GameStateManager.instance;

            // Publish event if new missions available
            // var newMissions = ContentUnlocker.getNewlyAvailableMissions(...);
            // if (newMissions.Count > 0) {
            //     onNewMissionsAvailable?.Invoke(newMissions);
            // }
        }

        private void checkWorldEvents() {
            var gm = GameStateManager.instance;
            float ratio = DifficultyCalculator.getDifficultyRatio(
                gm.progression,
                gm.roster,
                gm.technology
            );

            // Check difficulty warnings
            string warning = WorldEventTriggers.checkDifficultyWarning(
                ratio,
                gm.campaign.activeStoryFlags
            );

            if (warning != null) {
                gm.campaign.addStoryFlag(warning);
                onWorldEventTriggered?.Invoke(warning);
            }
        }

        private int findNextEventTime() {
            int minTime = int.MaxValue;

            foreach (var action in activeActions) {
                if (action.durationHours < minTime) {
                    minTime = action.durationHours;
                }
            }

            return minTime == int.MaxValue ? 24 : minTime; // Default to 1 day
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
        }

    }

}
```

## Step 4.3: Time Advanced Event

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

## Usage Example: Strategic Layer UI

```csharp
public class StrategicLayerUI : MonoBehaviour {

    [SerializeField] private Button researchButton;
    [SerializeField] private Button waitButton;
    [SerializeField] private Text threatLevelText;

    private void Start() {
        StrategicLayerManager.instance.onTimeAdvanced += OnTimeAdvanced;
        StrategicLayerManager.instance.onWorldEventTriggered += OnWorldEvent;

        researchButton.onClick.AddListener(OnResearchClicked);
        waitButton.onClick.AddListener(OnWaitClicked);

        UpdateDisplay();
    }

    private void OnResearchClicked() {
        // Start research - this will advance time when complete
        var action = TimeAction.CreateResearch("plasma_weapons", "Plasma Weapons", 72);
        StrategicLayerManager.instance.startAction(action);

        // Skip time to completion
        StrategicLayerManager.instance.skipToNextEvent();
    }

    private void OnWaitClicked() {
        // Advance time by 24 hours
        StrategicLayerManager.instance.advanceTime(24);
    }

    private void OnTimeAdvanced(int hours) {
        UpdateDisplay();
    }

    private void OnWorldEvent(string eventID) {
        // Show narrative popup
        ShowEventDialog(eventID);
    }

    private void UpdateDisplay() {
        var progression = GameStateManager.instance.progression;
        threatLevelText.text = $"Threat: {progression.worldThreatLevel:F2}";
    }

}
```

## The XCOM Loop

```
┌──────────────────┐
│  Mission Board   │◄────────────────────┐
│  (Pick mission)  │                     │
└────────┬─────────┘                     │
         │                               │
         ▼                               │
┌──────────────────┐                     │
│  TACTICAL COMBAT │                     │
│  (Time frozen)   │                     │
└────────┬─────────┘                     │
         │ Victory/Defeat                │
         ▼                               │
┌──────────────────┐                     │
│  Results Screen  │                     │
│  (XP, loot)      │                     │
└────────┬─────────┘                     │
         │                               │
         ▼                               │
┌──────────────────┐                     │
│  STRATEGIC LAYER │                     │
│  - Research      │──► Time Passes ─────┤
│  - Build         │                     │
│  - Heal          │                     │
│  - Explore       │                     │
└──────────────────┘                     │
         │                               │
         │ Ready for next mission        │
         └───────────────────────────────┘
```

**Checkpoint:** Create these files. The strategic layer now controls time flow outside of missions.
