# Part 3: Mission Manager

The MissionManager owns the mission lifecycle. It coordinates between the mission pools, the strategic layer, and the combat system (when it exists).

## Step 3.1: Mission Manager

Create `Assets/Scripts/Core/Missions/MissionManager.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Game.Core.Progression;
using Core.Game.Events;

namespace Game.Core.Missions {

    public class MissionManager : MonoBehaviour {

        public static MissionManager instance { get; private set; }

        // MARK: - State

        private MissionPool storyPool = new();
        private GenericMissionRotation genericRotation = new();
        private MissionData activeMission;
        private List<MissionRecord> missionHistory = new();

        // MARK: - Properties

        public MissionData currentMission => activeMission;
        public bool isInMission => activeMission != null;
        public IReadOnlyList<MissionRecord> history => missionHistory;

        // MARK: - Events

        public event Action<MissionData> onMissionStarted;
        public event Action<MissionData, MissionRecord> onMissionCompleted;
        public event Action<List<MissionData>> onAvailableMissionsChanged;

        // MARK: - Lifecycle

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // MARK: - Initialization

        public void initialize(
            List<MissionData> storyMissions,
            List<MissionData> genericTemplates
        ) {
            storyPool.registerMissions(storyMissions);
            foreach (var template in genericTemplates) {
                genericRotation.registerTemplate(template);
            }

            refreshAvailability();
        }

        // MARK: - Availability

        public void refreshAvailability() {
            var gm = GameStateManager.instance;

            // Unlock any story missions whose prerequisites are now met
            var unlockable = MissionAvailability.findUnlockable(
                storyPool,
                gm.progression,
                gm.campaign
            );

            foreach (var mission in unlockable) {
                storyPool.setStatus(mission.id, MissionStatus.Unlocked);
            }

            // Refresh the generic rotation
            genericRotation.refreshRotation(
                gm.progression,
                gm.roster,
                gm.technology
            );

            // Notify UI
            var allAvailable = getAllAvailableMissions();
            onAvailableMissionsChanged?.Invoke(allAvailable);
        }

        public List<MissionData> getAllAvailableMissions() {
            var available = new List<MissionData>();
            available.AddRange(storyPool.getAvailableMissions());
            available.AddRange(genericRotation.currentMissions);
            return available;
        }

        // MARK: - Mission Lifecycle

        public bool startMission(string missionID) {
            if (isInMission) {
                Debug.LogWarning("Cannot start mission — already in a mission");
                return false;
            }

            // Check story pool first, then generic rotation
            var mission = storyPool.getMission(missionID);
            if (mission != null) {
                var status = storyPool.getStatus(missionID);
                if (status != MissionStatus.Unlocked) {
                    Debug.LogWarning($"Mission {missionID} is not available (status: {status})");
                    return false;
                }
                storyPool.setStatus(missionID, MissionStatus.Active);
            } else {
                // Look in generic rotation
                mission = findInGenericRotation(missionID);
                if (mission == null) {
                    Debug.LogWarning($"Mission {missionID} not found");
                    return false;
                }
            }

            activeMission = mission;

            // Pause strategic time
            StrategicLayerManager.instance.enterMission();

            onMissionStarted?.Invoke(mission);
            EventBus.publish(new MissionStartedEvent { missionID = missionID });

            return true;
        }

        public void completeMission(MissionRecord record) {
            if (!isInMission) {
                Debug.LogWarning("No active mission to complete");
                return;
            }

            var mission = activeMission;

            // Update story pool status
            if (mission.missionType == MissionType.Story) {
                storyPool.setStatus(
                    mission.id,
                    record.wasSuccessful ? MissionStatus.Completed : MissionStatus.Failed
                );
            } else {
                // Remove generic mission from rotation
                genericRotation.removeFromRotation(mission.id);
            }

            // Store the record
            missionHistory.Add(record);

            // Process outcomes (rewards, consequences, progression)
            MissionOutcomeProcessor.process(mission, record);

            // Clear active mission
            activeMission = null;

            // Resume strategic time
            StrategicLayerManager.instance.exitMission();

            // Fire events
            onMissionCompleted?.Invoke(mission, record);
            EventBus.publish(new MissionCompletedEvent {
                missionID = mission.id,
                wasSuccessful = record.wasSuccessful
            });

            // Refresh available missions (prerequisites may have been met)
            refreshAvailability();
        }

        public void abandonMission() {
            if (!isInMission) return;

            var record = MissionRecord.Create(activeMission.id);
            record.wasSuccessful = false;

            completeMission(record);
        }

        // MARK: - Queries

        public MissionData getStoryMission(string missionID) {
            return storyPool.getMission(missionID);
        }

        public MissionStatus getStoryMissionStatus(string missionID) {
            return storyPool.getStatus(missionID);
        }

        public List<MissionData> getCompletedStoryMissions() {
            return storyPool.getCompletedMissions();
        }

        // MARK: - Private

        private MissionData findInGenericRotation(string missionID) {
            foreach (var mission in genericRotation.currentMissions) {
                if (mission.id == missionID) return mission;
            }
            return null;
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
        }

    }

}
```

## Step 3.2: Mission Started Event

Create `Assets/Scripts/Core/Events/MissionStartedEvent.cs`:

```csharp
namespace Game.Core.Events {

    public struct MissionStartedEvent {

        public string missionID;

    }

}
```

`MissionCompletedEvent` already exists from Phase 1.

## Step 3.3: The Mission Lifecycle

```
 ┌──────────┐      Player meets       ┌──────────┐
 │  Locked  │ ──── prerequisites ────► │ Unlocked │
 └──────────┘                          └────┬─────┘
                                            │
                                   Player selects
                                            │
                                            ▼
                                       ┌─────────┐
                                       │ Active  │
                                       └────┬────┘
                                            │
                             ┌──────────────┼──────────────┐
                             │              │              │
                         Success        Failure       Abandoned
                             │              │              │
                             ▼              ▼              ▼
                       ┌───────────┐  ┌──────────┐   ┌──────────┐
                       │ Completed │  │  Failed  │   │  Failed  │
                       └───────────┘  └──────────┘   └──────────┘
                             │              │
                             └──────┬───────┘
                                    │
                         refreshAvailability()
                                    │
                        New missions may unlock
```

**What happens at each stage:**

| Stage | Time | Strategic Layer | UI |
|-------|------|----------------|-----|
| Unlocked | Running | Player sees mission on board | Mission card visible |
| Active | **Paused** | `enterMission()` called | Combat scene loads |
| Completed | Resumed | `exitMission()` called | Results screen, then back to board |

## Step 3.4: Integration with Strategic Layer

The mission manager bridges the strategic and tactical layers:

```csharp
// Player selects a mission from the UI
MissionManager.instance.startMission("story_02");
// → StrategicLayerManager.instance.enterMission() is called
// → Time pauses, strategic actions freeze
// → Combat scene loads (future system)

// After combat resolves (from combat system callback)
var record = MissionRecord.Create("story_02");
record.wasSuccessful = true;
record.completedOptionalObjectives.Add("no_alarms");
record.injuredUnitIDs.Add("unit_rook");
record.turnsTaken = 12;

MissionManager.instance.completeMission(record);
// → Outcomes processed (rewards, consequences, XP)
// → StrategicLayerManager.instance.exitMission() is called
// → Time resumes
// → refreshAvailability() checks for new unlocks
// → UI updates with new mission options
```

The combat system (Phase 13: Combat Black Box) will eventually own the `MissionRecord` creation. For now, the interface is clean — combat produces a `MissionRecord`, and `MissionManager.completeMission()` handles everything else.

**Checkpoint:** Create these files and verify they compile. You should be able to initialize the manager with story missions and generic templates, start a mission, and complete it with a record.
