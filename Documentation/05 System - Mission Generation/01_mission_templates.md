# Part 1: Mission Templates

A template is a reusable blueprint that the generator instantiates into a concrete `MissionData`. Templates define *ranges* instead of fixed values, so each instance feels slightly different.

## Step 1.1: Variation Range

Create `Assets/Scripts/Core/Missions/Generation/VariationRange.cs`:

```csharp
using System;
using UnityEngine;

namespace Game.Core.Missions.Generation {

    [Serializable]
    public struct VariationRange {

        public int min;
        public int max;

        public VariationRange(int min, int max) {
            this.min = min;
            this.max = max;
        }

        public int roll() {
            return UnityEngine.Random.Range(min, max + 1);
        }

        public int scale(float multiplier) {
            int scaled = Mathf.RoundToInt(roll() * multiplier);
            return Mathf.Max(1, scaled);
        }

    }

}
```

Simple value type. `roll()` gives a random int in the range (inclusive). `scale()` rolls and then multiplies — used for difficulty scaling.

## Step 1.2: Mission Template

Create `Assets/Scripts/Core/Missions/Generation/MissionTemplate.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.Missions.Generation {

    [Serializable]
    public class MissionTemplate {

        public string templateID;

        public string titleBase;

        public string description;

        public MissionCategory category;

        // MARK: - Variation Ranges

        public VariationRange enemyCountRange;

        public VariationRange baseXPPoolRange;

        public Dictionary<ResourceType, VariationRange> resourceRewardRanges;

        // MARK: - Objectives

        public ObjectiveData[] primaryObjectives;

        public ObjectiveData[] optionalObjectivePool;

        public int optionalObjectiveCount;

        // MARK: - Availability

        public float minimumThreatLevel;

        // MARK: - Selection Weights

        public ResourceType primaryResourceReward;

        public float baseWeight;

        // MARK: - Factory

        public static MissionTemplate Create(
            string templateID,
            string title,
            string description,
            MissionCategory category,
            VariationRange enemyCount,
            VariationRange baseXPPool,
            float minimumThreat = 1.0f,
            float baseWeight = 1.0f
        ) {
            return new MissionTemplate {
                templateID = templateID,
                titleBase = title,
                description = description,
                category = category,
                enemyCountRange = enemyCount,
                baseXPPoolRange = baseXPPool,
                resourceRewardRanges = new Dictionary<ResourceType, VariationRange>(),
                primaryObjectives = new ObjectiveData[0],
                optionalObjectivePool = new ObjectiveData[0],
                optionalObjectiveCount = 0,
                minimumThreatLevel = minimumThreat,
                primaryResourceReward = ResourceType.Currency,
                baseWeight = baseWeight
            };
        }

    }

}
```

**Key fields:**

- `enemyCountRange` / `baseXPPoolRange` — Each instance rolls within these ranges, then the generator scales by difficulty ratio.
- `resourceRewardRanges` — Per-resource-type ranges. A raid template might have `Currency: 40-120, Alloys: 10-30`.
- `optionalObjectivePool` — A pool of possible optional objectives. The generator picks `optionalObjectiveCount` of them at random, so not every instance of the same template has the same side goals.
- `minimumThreatLevel` — Templates below the current world threat are available. Higher-threat templates gate behind progression (e.g., assassination missions don't appear until threat 1.3+).
- `primaryResourceReward` — The main resource this template provides. Used by the weighted selection to favor templates that give what the player is low on.
- `baseWeight` — Default selection probability. Common templates get higher weight, rare ones lower.

## Step 1.3: Template Library

Create `Assets/Scripts/Core/Missions/Generation/MissionTemplateLibrary.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.Missions.Generation {

    public static class MissionTemplateLibrary {

        public static List<MissionTemplate> createAll() {
            var templates = new List<MissionTemplate>();

            templates.Add(createSupplyRaid());
            templates.Add(createReconPatrol());
            templates.Add(createRescueOperation());
            templates.Add(createTechRecovery());
            templates.Add(createPatrolDefense());
            templates.Add(createSabotage());
            templates.Add(createAssassination());

            return templates;
        }

        // MARK: - Low Threat (available from start)

        private static MissionTemplate createSupplyRaid() {
            var t = MissionTemplate.Create(
                "tmpl_supply_raid",
                "Supply Raid",
                "Hit an enemy supply cache for resources.",
                MissionCategory.Raid,
                enemyCount: new VariationRange(3, 6),
                baseXPPool: new VariationRange(20, 40),
                minimumThreat: 1.0f,
                baseWeight: 1.5f // Common — shows up often
            );

            t.primaryResourceReward = ResourceType.Currency;
            t.resourceRewardRanges[ResourceType.Currency] = new VariationRange(40, 120);
            t.resourceRewardRanges[ResourceType.Alloys] = new VariationRange(5, 20);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("secure_cache", "Secure the supply cache")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("no_alarms", "Don't raise any alarms", optional: true),
                ObjectiveData.Create("destroy_comms", "Destroy the comms tower", optional: true),
                ObjectiveData.Create("under_turns", "Complete in under 8 turns", optional: true)
            };
            t.optionalObjectiveCount = 1;

            return t;
        }

        private static MissionTemplate createReconPatrol() {
            var t = MissionTemplate.Create(
                "tmpl_recon",
                "Recon Patrol",
                "Scout an area for enemy activity and report back.",
                MissionCategory.Recon,
                enemyCount: new VariationRange(2, 4),
                baseXPPool: new VariationRange(15, 30),
                minimumThreat: 1.0f,
                baseWeight: 1.2f
            );

            t.primaryResourceReward = ResourceType.Intel;
            t.resourceRewardRanges[ResourceType.Intel] = new VariationRange(10, 25);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("scout_zones", "Scout all marked zones"),
                ObjectiveData.Create("extract", "Extract safely")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("no_detection", "Avoid detection entirely", optional: true),
                ObjectiveData.Create("tag_targets", "Tag enemy assets for future ops", optional: true)
            };
            t.optionalObjectiveCount = 1;

            return t;
        }

        private static MissionTemplate createRescueOperation() {
            var t = MissionTemplate.Create(
                "tmpl_rescue",
                "Rescue Operation",
                "Extract a captured operative from enemy custody.",
                MissionCategory.Rescue,
                enemyCount: new VariationRange(4, 7),
                baseXPPool: new VariationRange(30, 50),
                minimumThreat: 1.0f,
                baseWeight: 0.8f // Less common
            );

            t.primaryResourceReward = ResourceType.Currency;
            t.resourceRewardRanges[ResourceType.Currency] = new VariationRange(20, 60);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("find_prisoner", "Locate the prisoner"),
                ObjectiveData.Create("extract_prisoner", "Extract to safety")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("no_casualties", "No squad casualties", optional: true),
                ObjectiveData.Create("recover_intel", "Recover enemy intel", optional: true)
            };
            t.optionalObjectiveCount = 1;

            return t;
        }

        // MARK: - Medium Threat (unlock as world threat rises)

        private static MissionTemplate createTechRecovery() {
            var t = MissionTemplate.Create(
                "tmpl_tech_recovery",
                "Tech Recovery",
                "Recover salvageable technology from a contested site.",
                MissionCategory.TechRecovery,
                enemyCount: new VariationRange(4, 6),
                baseXPPool: new VariationRange(25, 45),
                minimumThreat: 1.2f,
                baseWeight: 1.0f
            );

            t.primaryResourceReward = ResourceType.TechComponents;
            t.resourceRewardRanges[ResourceType.TechComponents] = new VariationRange(10, 30);
            t.resourceRewardRanges[ResourceType.Alloys] = new VariationRange(5, 15);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("secure_site", "Secure the research site"),
                ObjectiveData.Create("recover_tech", "Recover the prototype")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("recover_data", "Download research data", optional: true),
                ObjectiveData.Create("no_damage", "Prototype undamaged", optional: true),
                ObjectiveData.Create("clear_all", "Eliminate all hostiles", optional: true)
            };
            t.optionalObjectiveCount = 1;

            return t;
        }

        private static MissionTemplate createPatrolDefense() {
            var t = MissionTemplate.Create(
                "tmpl_defense",
                "Patrol Defense",
                "Defend a key position against an incoming assault.",
                MissionCategory.Defense,
                enemyCount: new VariationRange(5, 9),
                baseXPPool: new VariationRange(30, 55),
                minimumThreat: 1.2f,
                baseWeight: 1.0f
            );

            t.primaryResourceReward = ResourceType.Currency;
            t.resourceRewardRanges[ResourceType.Currency] = new VariationRange(50, 150);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("survive_waves", "Survive all enemy waves"),
                ObjectiveData.Create("hold_position", "Hold the position")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("no_breach", "Prevent any breach", optional: true),
                ObjectiveData.Create("under_casualties", "Fewer than 2 casualties", optional: true)
            };
            t.optionalObjectiveCount = 1;

            return t;
        }

        // MARK: - High Threat (late-game templates)

        private static MissionTemplate createSabotage() {
            var t = MissionTemplate.Create(
                "tmpl_sabotage",
                "Sabotage Operation",
                "Infiltrate and destroy a critical enemy facility.",
                MissionCategory.Sabotage,
                enemyCount: new VariationRange(5, 8),
                baseXPPool: new VariationRange(40, 65),
                minimumThreat: 1.4f,
                baseWeight: 0.7f
            );

            t.primaryResourceReward = ResourceType.Intel;
            t.resourceRewardRanges[ResourceType.Intel] = new VariationRange(15, 35);
            t.resourceRewardRanges[ResourceType.Currency] = new VariationRange(30, 80);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("plant_charges", "Plant demolition charges"),
                ObjectiveData.Create("extract_before_det", "Extract before detonation")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("steal_plans", "Steal enemy plans", optional: true),
                ObjectiveData.Create("stealth_entry", "Enter undetected", optional: true),
                ObjectiveData.Create("no_civilian", "No civilian casualties", optional: true)
            };
            t.optionalObjectiveCount = 2;

            return t;
        }

        private static MissionTemplate createAssassination() {
            var t = MissionTemplate.Create(
                "tmpl_assassination",
                "Assassination",
                "Eliminate a high-value enemy target.",
                MissionCategory.Assassination,
                enemyCount: new VariationRange(6, 10),
                baseXPPool: new VariationRange(50, 80),
                minimumThreat: 1.5f,
                baseWeight: 0.5f // Rare
            );

            t.primaryResourceReward = ResourceType.Intel;
            t.resourceRewardRanges[ResourceType.Intel] = new VariationRange(20, 40);
            t.resourceRewardRanges[ResourceType.Currency] = new VariationRange(60, 150);

            t.primaryObjectives = new[] {
                ObjectiveData.Create("eliminate_target", "Eliminate the target"),
                ObjectiveData.Create("extract_squad", "Extract the squad")
            };

            t.optionalObjectivePool = new[] {
                ObjectiveData.Create("confirm_kill", "Confirm the kill", optional: true),
                ObjectiveData.Create("recover_intel", "Recover target's intel", optional: true),
                ObjectiveData.Create("no_witnesses", "Leave no witnesses", optional: true),
                ObjectiveData.Create("under_turns", "Complete in under 6 turns", optional: true)
            };
            t.optionalObjectiveCount = 2;

            return t;
        }

    }

}
```

## Template Availability by Threat Level

Instead of fixed act gates, templates unlock organically as `worldThreatLevel` rises:

| Threat Level | New Templates Available | Feel |
|--------------|------------------------|------|
| 1.0+ (start) | Supply Raid, Recon Patrol, Rescue | Safe, resource-gathering |
| 1.2+ | Tech Recovery, Patrol Defense | Mid-range, mixed objectives |
| 1.4+ | Sabotage | High-risk infiltration |
| 1.5+ | Assassination | End-game, high-value targets |

As the world gets more dangerous, the mission board gets more varied and more rewarding — but also more lethal. Early game has 3 template types; by late game all 7 are in the pool.

**Checkpoint:** Create these files and verify they compile. You should be able to call `MissionTemplateLibrary.createAll()` and get a full list of templates.
