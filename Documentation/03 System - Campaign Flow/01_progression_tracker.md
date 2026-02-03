# Part 1: Progression State and Constants

## Step 1.1: Progression Constants

Create `Assets/Scripts/Core/Data/Constants/ProgressionConstants.cs`:

```csharp
namespace Game.Core.Data {

    public static class ProgressionConstants {

        // Story progress thresholds (0.0 to 1.0)
        public const float EARLY_GAME_THRESHOLD = 0.25f;
        public const float MID_GAME_THRESHOLD = 0.50f;
        public const float LATE_GAME_THRESHOLD = 0.75f;

        // World threat settings
        public const float BASE_THREAT_LEVEL = 1.0f;
        public const float THREAT_GROWTH_PER_DAY = 0.02f;

        // Difficulty ratio comfort zones
        public const float DIFFICULTY_COMFORTABLE = 0.8f;
        public const float DIFFICULTY_BALANCED = 1.0f;
        public const float DIFFICULTY_CHALLENGING = 1.2f;
        public const float DIFFICULTY_PUNISHING = 1.5f;

        // Player power estimation weights
        public const float SQUAD_SIZE_WEIGHT = 0.3f;
        public const float AVERAGE_LEVEL_WEIGHT = 0.4f;
        public const float TECH_UNLOCKS_WEIGHT = 0.3f;

        // Time units (in-game time, not real time)
        public const int HOURS_PER_DAY = 24;
        public const int DAYS_PER_WEEK = 7;

    }

}
```

**Design Note:** These constants define the "feel" of progression. Tune `THREAT_GROWTH_PER_DAY` to control how quickly players fall behind if they stall.

## Step 1.2: Main Mission Data

Create `Assets/Scripts/Core/Data/Structs/MainMissionData.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct MainMissionData {

        public string id;

        public string title;

        public float progressValue;

        public string[] prerequisiteMissionIDs;

        public float minimumStoryProgress;

        public static MainMissionData Create(
            string id,
            string title,
            float progressValue,
            float minimumProgress = 0f,
            params string[] prerequisites
        ) {
            return new MainMissionData {
                id = id,
                title = title,
                progressValue = progressValue,
                minimumStoryProgress = minimumProgress,
                prerequisiteMissionIDs = prerequisites ?? new string[0]
            };
        }

    }

}
```

**Design Note:** Each main mission contributes a `progressValue` to the story. The sum of all main mission progress values should equal 1.0.

## Step 1.3: ProgressionState Class

Create `Assets/Scripts/Core/GameState/ProgressionState.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class ProgressionState {

        public float storyProgress;

        public float worldThreatLevel;

        public List<string> completedMainMissionIDs;

        public List<string> availableMainMissionIDs;

        public int totalElapsedHours;

        public ProgressionState() {
            storyProgress = 0f;
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL;
            completedMainMissionIDs = new List<string>();
            availableMainMissionIDs = new List<string>();
            totalElapsedHours = 0;
        }

        // MARK: - Story Progress

        public void completeMainMission(string missionID, float progressValue) {
            if (completedMainMissionIDs.Contains(missionID)) return;

            completedMainMissionIDs.Add(missionID);
            availableMainMissionIDs.Remove(missionID);
            storyProgress = Math.Min(1.0f, storyProgress + progressValue);
        }

        public bool hasCompletedMission(string missionID) {
            return completedMainMissionIDs.Contains(missionID);
        }

        public bool isMissionAvailable(string missionID) {
            return availableMainMissionIDs.Contains(missionID);
        }

        public void unlockMission(string missionID) {
            if (completedMainMissionIDs.Contains(missionID)) return;
            if (availableMainMissionIDs.Contains(missionID)) return;

            availableMainMissionIDs.Add(missionID);
        }

        // MARK: - Time and Threat

        public void advanceTime(int hours) {
            totalElapsedHours += hours;
            updateWorldThreat();
        }

        public int getElapsedDays() {
            return totalElapsedHours / ProgressionConstants.HOURS_PER_DAY;
        }

        public int getElapsedWeeks() {
            return getElapsedDays() / ProgressionConstants.DAYS_PER_WEEK;
        }

        private void updateWorldThreat() {
            float days = totalElapsedHours / (float)ProgressionConstants.HOURS_PER_DAY;
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL +
                (days * ProgressionConstants.THREAT_GROWTH_PER_DAY);
        }

        // MARK: - Progress Queries

        public bool isEarlyGame() {
            return storyProgress < ProgressionConstants.EARLY_GAME_THRESHOLD;
        }

        public bool isMidGame() {
            return storyProgress >= ProgressionConstants.EARLY_GAME_THRESHOLD &&
                   storyProgress < ProgressionConstants.LATE_GAME_THRESHOLD;
        }

        public bool isLateGame() {
            return storyProgress >= ProgressionConstants.LATE_GAME_THRESHOLD;
        }

        public bool isEndGame() {
            return storyProgress >= 1.0f;
        }

        // MARK: - Reset

        public void reset() {
            storyProgress = 0f;
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL;
            completedMainMissionIDs.Clear();
            availableMainMissionIDs.Clear();
            totalElapsedHours = 0;
        }

    }

}
```

## Step 1.4: Update GameState

Add `ProgressionState` to your `GameState` class:

```csharp
using System;
using Game.Core.States;
using UnityEngine;

namespace Game.Core {

    [Serializable]
    public class GameState {

        public int version;

        public CampaignState campaign;

        public MaterialState materials;

        public ProgressionState progression;  // <- ADD THIS

        public ResourceState resources;

        public TechState technology;

        public UnitRosterState roster;

        public GameState() {
            version = 1;
            campaign = new CampaignState();
            materials = new MaterialState();
            progression = new ProgressionState();  // <- ADD THIS
            resources = new ResourceState();
            technology = new TechState();
            roster = new UnitRosterState();
        }

        public GameState createDeepCopy() {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<GameState>(json);
        }

        public void reset() {
            campaign.reset();
            materials.reset();
            progression.reset();  // <- ADD THIS
            resources.reset();
            technology.reset();
            roster.reset();
        }

    }

}
```

**Checkpoint:** Create these files and verify they compile. Update your `GameState` to include the new `ProgressionState`.
