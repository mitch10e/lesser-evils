# Part 2: Mission Pools and Availability

This part manages *which* missions the player can see and select. Story missions unlock based on prerequisites and story progress. Generic missions rotate based on world threat and what the player needs.

## Step 2.1: Mission Pool

Create `Assets/Scripts/Core/Missions/MissionPool.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Missions {

    public class MissionPool {

        private List<MissionData> allMissions = new();
        private Dictionary<string, MissionStatus> missionStatuses = new();

        // MARK: - Registration

        public void registerMission(MissionData mission) {
            allMissions.Add(mission);
            missionStatuses[mission.id] = MissionStatus.Locked;
        }

        public void registerMissions(List<MissionData> missions) {
            foreach (var mission in missions) {
                registerMission(mission);
            }
        }

        // MARK: - Status

        public MissionStatus getStatus(string missionID) {
            return missionStatuses.TryGetValue(missionID, out var status)
                ? status
                : MissionStatus.Locked;
        }

        public void setStatus(string missionID, MissionStatus status) {
            if (missionStatuses.ContainsKey(missionID)) {
                missionStatuses[missionID] = status;
            }
        }

        // MARK: - Queries

        public MissionData getMission(string missionID) {
            foreach (var mission in allMissions) {
                if (mission.id == missionID) return mission;
            }
            return null;
        }

        public List<MissionData> getMissionsByStatus(MissionStatus status) {
            List<MissionData> result = new();

            foreach (var mission in allMissions) {
                if (getStatus(mission.id) == status) {
                    result.Add(mission);
                }
            }

            return result;
        }

        public List<MissionData> getMissionsByType(MissionType type) {
            List<MissionData> result = new();

            foreach (var mission in allMissions) {
                if (mission.missionType == type) {
                    result.Add(mission);
                }
            }

            return result;
        }

        public List<MissionData> getAvailableMissions() {
            return getMissionsByStatus(MissionStatus.Unlocked);
        }

        public List<MissionData> getCompletedMissions() {
            return getMissionsByStatus(MissionStatus.Completed);
        }

    }

}
```

The pool is a flat container — it holds all missions and their current status. It doesn't decide *when* to unlock them. That logic lives in the availability checker below.

## Step 2.2: Mission Availability Checker

Create `Assets/Scripts/Core/Missions/MissionAvailability.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Missions {

    public static class MissionAvailability {

        public static bool canUnlock(
            MissionData mission,
            MissionPool pool,
            ProgressionState progression,
            CampaignState campaign
        ) {
            // Already unlocked, active, completed, or failed
            var status = pool.getStatus(mission.id);
            if (status != MissionStatus.Locked) return false;

            // Check prerequisite missions
            foreach (var prereqID in mission.prerequisiteMissionIDs) {
                if (pool.getStatus(prereqID) != MissionStatus.Completed) {
                    return false;
                }
            }

            // Check required story flags
            foreach (var flag in mission.requiredStoryFlags) {
                if (!campaign.activeStoryFlags.Contains(flag)) {
                    return false;
                }
            }

            // Check faction restriction
            if (mission.factionRestriction != FactionType.None &&
                mission.factionRestriction != campaign.currentFaction) {
                return false;
            }

            return true;
        }

        public static List<MissionData> findUnlockable(
            MissionPool pool,
            ProgressionState progression,
            CampaignState campaign
        ) {
            List<MissionData> unlockable = new();

            foreach (var mission in pool.getMissionsByStatus(MissionStatus.Locked)) {
                if (canUnlock(mission, pool, progression, campaign)) {
                    unlockable.Add(mission);
                }
            }

            return unlockable;
        }

    }

}
```

**Why separate from ContentUnlocker?**

`ContentUnlocker` (from Phase 3) works with the lightweight `MainMissionData` and `ProgressionState`. `MissionAvailability` works with the full `MissionData` and adds checks for story flags and faction restrictions. As the mission system matures, this is where you add more complex gating logic without bloating the progression system.

## Step 2.3: Generic Mission Rotation

Create `Assets/Scripts/Core/Missions/GenericMissionRotation.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Progression;
using UnityEngine;

namespace Game.Core.Missions {

    public class GenericMissionRotation {

        private List<MissionData> templates = new();
        private List<MissionData> currentRotation = new();

        public const int MAX_AVAILABLE_GENERIC = 3;

        // MARK: - Setup

        public void registerTemplate(MissionData template) {
            templates.Add(template);
        }

        // MARK: - Rotation

        public IReadOnlyList<MissionData> currentMissions => currentRotation;

        public void refreshRotation(
            ProgressionState progression,
            UnitRosterState roster,
            TechState technology
        ) {
            currentRotation.Clear();

            if (templates.Count == 0) return;

            float difficultyRatio = DifficultyCalculator.getDifficultyRatio(
                progression, roster, technology
            );

            // Shuffle templates and pick up to MAX_AVAILABLE_GENERIC
            var shuffled = new List<MissionData>(templates);
            shuffleList(shuffled);

            int count = Mathf.Min(MAX_AVAILABLE_GENERIC, shuffled.Count);

            for (int i = 0; i < count; i++) {
                var instance = instantiateFromTemplate(shuffled[i], difficultyRatio);
                currentRotation.Add(instance);
            }
        }

        // MARK: - Instantiation

        private MissionData instantiateFromTemplate(MissionData template, float difficultyRatio) {
            // Create a unique instance from the template
            var instance = MissionData.CreateGeneric(
                $"{template.id}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                template.title,
                template.description,
                template.category
            );

            // Scale enemy count with difficulty
            instance.baseEnemyCount = Mathf.RoundToInt(
                template.baseEnemyCount * DifficultyCalculator.getEnemyCountMultiplier(difficultyRatio)
            );

            // Scale rewards inversely — harder world = better loot
            instance.rewards = template.rewards;
            instance.rewards.baseXPPool = Mathf.RoundToInt(
                template.rewards.baseXPPool * DifficultyCalculator.getXPMultiplier(difficultyRatio)
            );

            instance.primaryObjectives = template.primaryObjectives;
            instance.optionalObjectives = template.optionalObjectives;

            return instance;
        }

        // MARK: - Utility

        public void removeFromRotation(string missionID) {
            currentRotation.RemoveAll(m => m.id == missionID);
        }

        private void shuffleList<T>(List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

    }

}
```

**How generic rotation works:**

1. You define **templates** — reusable mission blueprints like "Supply Convoy Raid" or "Recon Patrol"
2. When the player returns from a mission (or at game start), call `refreshRotation()`
3. It shuffles the templates, picks up to 3, and creates **instances** with unique IDs
4. Enemy count and rewards are scaled by the current difficulty ratio
5. The player picks one (or ignores them). After the next story mission, the rotation refreshes

This mirrors XCOM's "scan site" model — you always have a few optional missions available, but you can't do them all before the world moves on.

## Step 2.4: Example Mission Definitions

Here's how you'd define a set of story missions and generic templates:

```csharp
public static class MissionDatabase {

    public static List<MissionData> createStoryMissions() {
        var missions = new List<MissionData>();

        // First story mission - always available
        var m1 = MissionData.CreateStory(
            "story_01", "First Contact",
            "Make contact with the resistance cell in the outer districts.",
            MissionCategory.Recon,
            storyProgress: 0.1f
        );
        m1.primaryObjectives = new[] {
            ObjectiveData.Create("reach_safehouse", "Reach the safehouse"),
            ObjectiveData.Create("extract", "Extract with intel")
        };
        m1.optionalObjectives = new[] {
            ObjectiveData.Create("no_alarms", "Complete without raising alarms", optional: true)
        };
        m1.rewards = MissionRewards.Create(baseXPPool: 50);
        m1.rewards.resources[ResourceType.Intel] = 20;
        missions.Add(m1);

        // Second story mission - requires first
        var m2 = MissionData.CreateStory(
            "story_02", "Supply Line",
            "Raid the convoy to secure supplies for the resistance.",
            MissionCategory.Raid,
            storyProgress: 0.1f,
            "story_01" // prerequisite
        );
        m2.primaryObjectives = new[] {
            ObjectiveData.Create("destroy_convoy", "Destroy the supply convoy"),
            ObjectiveData.Create("secure_cargo", "Secure the cargo")
        };
        m2.rewards = MissionRewards.Create(baseXPPool: 75);
        m2.rewards.resources[ResourceType.Alloys] = 30;
        m2.rewards.resources[ResourceType.Currency] = 100;
        missions.Add(m2);

        return missions;
    }

    public static List<MissionData> createGenericTemplates() {
        var templates = new List<MissionData>();

        var raid = MissionData.CreateGeneric(
            "generic_raid", "Supply Raid",
            "Hit an enemy supply cache for resources.",
            MissionCategory.Raid
        );
        raid.baseEnemyCount = 5;
        raid.primaryObjectives = new[] {
            ObjectiveData.Create("raid_supplies", "Secure the supply cache")
        };
        raid.rewards = MissionRewards.Create(baseXPPool: 30);
        raid.rewards.resources[ResourceType.Currency] = 50;
        templates.Add(raid);

        var recon = MissionData.CreateGeneric(
            "generic_recon", "Recon Patrol",
            "Scout an area for enemy activity.",
            MissionCategory.Recon
        );
        recon.baseEnemyCount = 3;
        recon.primaryObjectives = new[] {
            ObjectiveData.Create("scout_area", "Scout the marked locations")
        };
        recon.rewards = MissionRewards.Create(baseXPPool: 20);
        recon.rewards.resources[ResourceType.Intel] = 15;
        templates.Add(recon);

        var rescue = MissionData.CreateGeneric(
            "generic_rescue", "Rescue Operation",
            "Extract a captured operative from enemy custody.",
            MissionCategory.Rescue
        );
        rescue.baseEnemyCount = 6;
        rescue.primaryObjectives = new[] {
            ObjectiveData.Create("find_prisoner", "Locate the prisoner"),
            ObjectiveData.Create("extract_prisoner", "Extract to safety")
        };
        rescue.rewards = MissionRewards.Create(baseXPPool: 40);
        templates.Add(rescue);

        return templates;
    }

}
```

**Checkpoint:** Create these files and verify they compile. You should be able to register story missions and generic templates into their respective pools.
