# Part 3: Integration

This part connects `MissionGenerator` into `MissionManager`, replacing the simpler `GenericMissionRotation` from Phase 4.

## Step 3.1: Update MissionManager

Replace the `GenericMissionRotation` field with `MissionGenerator` in `MissionManager.cs`:

```csharp
// BEFORE (Phase 4):
private GenericMissionRotation genericRotation = new();

// AFTER (Phase 5):
private MissionGenerator missionGenerator = new();
```

Update `initialize()`:

```csharp
// BEFORE:
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

// AFTER:
public void initialize(
    List<MissionData> storyMissions,
    List<MissionTemplate> genericTemplates
) {
    storyPool.registerMissions(storyMissions);
    missionGenerator.registerTemplates(genericTemplates);
    refreshAvailability();
}
```

Update `refreshAvailability()`:

```csharp
// BEFORE:
genericRotation.refreshRotation(
    gm.progression,
    gm.roster,
    gm.technology
);

// AFTER:
missionGenerator.generate(
    gm.progression,
    gm.roster,
    gm.technology,
    gm.resources // NEW — enables resource-aware selection
);
```

Update `getAllAvailableMissions()`:

```csharp
// BEFORE:
available.AddRange(genericRotation.currentMissions);

// AFTER:
available.AddRange(missionGenerator.currentRotation);
```

Update `findInGenericRotation()`:

```csharp
// BEFORE:
private MissionData findInGenericRotation(string missionID) {
    foreach (var mission in genericRotation.currentMissions) {

// AFTER:
private MissionData findInGenericRotation(string missionID) {
    foreach (var mission in missionGenerator.currentRotation) {
```

Update `completeMission()` — the removal line:

```csharp
// BEFORE:
genericRotation.removeFromRotation(mission.id);

// AFTER:
missionGenerator.removeFromRotation(mission.id);
```

## Step 3.2: Updated MissionManager (Full Reference)

Here's the complete updated `MissionManager` with `MissionGenerator` integrated:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Game.Core.Progression;
using Game.Core.Missions.Generation;
using Core.Game.Events;

namespace Game.Core.Missions {

    public class MissionManager : MonoBehaviour {

        public static MissionManager instance { get; private set; }

        // MARK: - State

        private MissionPool storyPool = new();
        private MissionGenerator missionGenerator = new();
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
            List<MissionTemplate> genericTemplates
        ) {
            storyPool.registerMissions(storyMissions);
            missionGenerator.registerTemplates(genericTemplates);

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

            // Generate new generic rotation
            missionGenerator.generate(
                gm.progression,
                gm.roster,
                gm.technology,
                gm.resources
            );

            // Notify UI
            var allAvailable = getAllAvailableMissions();
            onAvailableMissionsChanged?.Invoke(allAvailable);
        }

        public List<MissionData> getAllAvailableMissions() {
            var available = new List<MissionData>();
            available.AddRange(storyPool.getAvailableMissions());
            available.AddRange(missionGenerator.currentRotation);
            return available;
        }

        // MARK: - Mission Lifecycle

        public bool startMission(string missionID) {
            if (isInMission) {
                Debug.LogWarning("Cannot start mission — already in a mission");
                return false;
            }

            var mission = storyPool.getMission(missionID);
            if (mission != null) {
                var status = storyPool.getStatus(missionID);
                if (status != MissionStatus.Unlocked) {
                    Debug.LogWarning($"Mission {missionID} is not available (status: {status})");
                    return false;
                }
                storyPool.setStatus(missionID, MissionStatus.Active);
            } else {
                mission = findInGenericRotation(missionID);
                if (mission == null) {
                    Debug.LogWarning($"Mission {missionID} not found");
                    return false;
                }
            }

            activeMission = mission;

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

            if (mission.missionType == MissionType.Story) {
                storyPool.setStatus(
                    mission.id,
                    record.wasSuccessful ? MissionStatus.Completed : MissionStatus.Failed
                );
            } else {
                missionGenerator.removeFromRotation(mission.id);
            }

            missionHistory.Add(record);

            MissionOutcomeProcessor.process(mission, record);

            activeMission = null;

            StrategicLayerManager.instance.exitMission();

            onMissionCompleted?.Invoke(mission, record);
            EventBus.publish(new MissionCompletedEvent {
                missionID = mission.id,
                wasSuccessful = record.wasSuccessful
            });

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
            foreach (var mission in missionGenerator.currentRotation) {
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

## Step 3.3: Initialization Example

```csharp
public class GameBootstrap : MonoBehaviour {

    private void Start() {
        // Load story missions (hardcoded here, could come from JSON/ScriptableObjects)
        var storyMissions = MissionDatabase.createStoryMissions();

        // Load generic templates
        var genericTemplates = MissionTemplateLibrary.createAll();

        // Initialize the mission system
        MissionManager.instance.initialize(storyMissions, genericTemplates);
    }

}
```

## Step 3.4: When Does the Rotation Refresh?

The generic rotation regenerates whenever `refreshAvailability()` is called. This happens:

| Trigger | Why |
|---------|-----|
| `MissionManager.initialize()` | Game start — populate the initial board |
| `MissionManager.completeMission()` | After any mission — new context, new options |
| Strategic layer action completes | Time passed, threat changed, resources changed |
| Manual call | UI "Refresh Board" button if you want one |

The generator uses the *current* game state every time, so the board naturally evolves. Early game you see supply raids and recon. As threat rises, sabotage and assassination missions appear. If you're low on alloys, more raid missions show up.

## Step 3.5: Removing GenericMissionRotation

Once `MissionGenerator` is in place, `GenericMissionRotation.cs` from Phase 4 can be deleted. The generator fully replaces it with smarter selection and procedural variation.

```
DELETE: Assets/Scripts/Core/Missions/GenericMissionRotation.cs
```

The Phase 4 version was a stepping stone. It served its purpose — getting the rotation concept working — and the generator is the complete implementation.

**Checkpoint:** Update `MissionManager` with the changes above, delete `GenericMissionRotation.cs`, and verify everything compiles. The mission board should now show contextually relevant, procedurally varied generic missions alongside story missions.
