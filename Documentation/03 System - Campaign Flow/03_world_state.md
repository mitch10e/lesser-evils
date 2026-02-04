# Part 3: World State and Content Unlocking

The world evolves as time passes and story progresses. This part covers how content unlocks and the world changes.

## Step 3.1: Content Unlock Thresholds

Create `Assets/Scripts/Core/Progression/ContentUnlocker.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class ContentUnlocker {

        // MARK: - Mission Availability

        public static bool canUnlockMission(
            MainMissionData mission,
            ProgressionState progression
        ) {
            // Check story progress threshold
            if (progression.storyProgress < mission.minimumStoryProgress) {
                return false;
            }

            // Check prerequisites
            foreach (var prereq in mission.prerequisiteMissionIDs) {
                if (!progression.hasCompletedMission(prereq)) {
                    return false;
                }
            }

            // Already completed or available
            if (progression.hasCompletedMission(mission.id)) {
                return false;
            }

            return true;
        }

        public static List<string> getNewlyAvailableMissions(
            List<MainMissionData> allMissions,
            ProgressionState progression
        ) {
            List<string> newMissions = new();

            foreach (var mission in allMissions) {
                if (canUnlockMission(mission, progression) &&
                    !progression.isMissionAvailable(mission.id)) {
                    newMissions.Add(mission.id);
                }
            }

            return newMissions;
        }

        // MARK: - Faction Activity

        public static bool isFactionActive(FactionType faction, ProgressionState progression) {
            // All factions active from the start in this design
            // Could be gated by progress if desired
            return faction != FactionType.None;
        }

        public static float getFactionThreatLevel(
            FactionType faction,
            ProgressionState progression
        ) {
            // Each faction's threat scales with world threat
            // Could differentiate by faction type
            return progression.worldThreatLevel;
        }

    }

}
```

## Step 3.2: World Event Triggers

Create `Assets/Scripts/Core/Progression/WorldEventTriggers.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class WorldEventTriggers {

        // MARK: - Time-Based Events

        public static List<string> checkTimeEvents(
            ProgressionState progression,
            List<string> alreadyTriggered
        ) {
            List<string> newTriggers = new();
            int weeks = progression.getElapsedWeeks();

            // Weekly world events
            string weekEvent = $"week_{weeks}_event";
            if (!alreadyTriggered.Contains(weekEvent) && weeks > 0) {
                newTriggers.Add(weekEvent);
            }

            // Threat escalation milestones
            if (progression.worldThreatLevel >= 1.5f &&
                !alreadyTriggered.Contains("threat_level_high")) {
                newTriggers.Add("threat_level_high");
            }

            if (progression.worldThreatLevel >= 2.0f &&
                !alreadyTriggered.Contains("threat_level_critical")) {
                newTriggers.Add("threat_level_critical");
            }

            return newTriggers;
        }

        // MARK: - Difficulty Warning Events

        public static string checkDifficultyWarning(
            float difficultyRatio,
            List<string> alreadyTriggered
        ) {
            // Subtle narrative hints when falling behind
            if (difficultyRatio >= ProgressionConstants.DIFFICULTY_CHALLENGING &&
                !alreadyTriggered.Contains("warning_falling_behind")) {
                return "warning_falling_behind";
            }

            if (difficultyRatio >= ProgressionConstants.DIFFICULTY_PUNISHING &&
                !alreadyTriggered.Contains("warning_critical_danger")) {
                return "warning_critical_danger";
            }

            return null;
        }

    }

}
```

## Step 3.3: Narrative Flavor Based on State

The world should feel different based on progression:

```csharp
public static class WorldNarrative {

    public static string getThreatDescription(float worldThreatLevel) {
        if (worldThreatLevel < 1.2f) {
            return "Enemy forces are organizing.";
        } else if (worldThreatLevel < 1.5f) {
            return "Enemy strength grows daily.";
        } else if (worldThreatLevel < 2.0f) {
            return "Enemy forces are formidable.";
        } else {
            return "Enemy dominance is nearly complete.";
        }
    }

    public static string getDifficultyHint(DifficultyTier tier) {
        switch (tier) {
            case DifficultyTier.Easy:
                return "Your squad is well-prepared for current threats.";
            case DifficultyTier.Comfortable:
                return "Operations proceed smoothly.";
            case DifficultyTier.Balanced:
                return "The enemy matches your strength.";
            case DifficultyTier.Challenging:
                return "Intel suggests enemy forces are gaining ground.";
            case DifficultyTier.Punishing:
                return "Command warns: enemy superiority is critical.";
            default:
                return "";
        }
    }

}
```

## How It Comes Together

```
Time Passes (mission complete, rest, etc.)
         │
         ▼
ProgressionState.advanceTime(hours)
         │
         ├──► worldThreatLevel increases
         │
         ▼
ContentUnlocker.getNewlyAvailableMissions()
         │
         ├──► New missions unlocked based on progress
         │
         ▼
WorldEventTriggers.checkTimeEvents()
         │
         ├──► Narrative events fire
         │
         ▼
DifficultyCalculator.getDifficultyRatio()
         │
         └──► Enemy scaling updated for next mission
```

**Checkpoint:** Create these files. The world state system provides context for how the campaign evolves organically.
