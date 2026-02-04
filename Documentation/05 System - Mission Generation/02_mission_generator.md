# Part 2: Mission Generator

The generator takes templates, filters them by threat level, scores them against the player's current state, and instantiates varied mission instances.

## Step 2.1: Mission Generator

Create `Assets/Scripts/Core/Missions/Generation/MissionGenerator.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Progression;

namespace Game.Core.Missions.Generation {

    public class MissionGenerator {

        private List<MissionTemplate> allTemplates = new();
        private List<MissionData> currentMissions = new();

        public const int MAX_AVAILABLE = 3;

        // MARK: - Setup

        public void registerTemplates(List<MissionTemplate> templates) {
            allTemplates.AddRange(templates);
        }

        // MARK: - Properties

        public IReadOnlyList<MissionData> currentRotation => currentMissions;

        // MARK: - Generation

        public void generate(
            ProgressionState progression,
            UnitRosterState roster,
            TechState technology,
            ResourceState resources
        ) {
            currentMissions.Clear();

            // 1. Filter templates by threat level
            var eligible = filterByThreat(progression.worldThreatLevel);

            if (eligible.Count == 0) return;

            // 2. Score each template against player state
            var scored = scoreTemplates(eligible, resources, roster);

            // 3. Weighted random selection
            var selected = weightedSelect(scored, MAX_AVAILABLE);

            // 4. Instantiate with variation and difficulty scaling
            float difficultyRatio = DifficultyCalculator.getDifficultyRatio(
                progression, roster, technology
            );

            foreach (var template in selected) {
                var mission = instantiate(template, difficultyRatio);
                currentMissions.Add(mission);
            }
        }

        // MARK: - Step 1: Filter

        private List<MissionTemplate> filterByThreat(float worldThreatLevel) {
            List<MissionTemplate> eligible = new();

            foreach (var template in allTemplates) {
                if (worldThreatLevel >= template.minimumThreatLevel) {
                    eligible.Add(template);
                }
            }

            return eligible;
        }

        // MARK: - Step 2: Score

        private List<ScoredTemplate> scoreTemplates(
            List<MissionTemplate> templates,
            ResourceState resources,
            UnitRosterState roster
        ) {
            List<ScoredTemplate> scored = new();

            foreach (var template in templates) {
                float weight = template.baseWeight;

                // Boost weight if player is low on this template's primary resource
                weight *= getResourceNeedMultiplier(template.primaryResourceReward, resources);

                // Reduce weight for rescue missions if nobody is captured/missing
                if (template.category == MissionCategory.Rescue) {
                    weight *= getRescueRelevance(roster);
                }

                scored.Add(new ScoredTemplate {
                    template = template,
                    weight = weight
                });
            }

            return scored;
        }

        private float getResourceNeedMultiplier(ResourceType type, ResourceState resources) {
            int current = resources.getAmount(type);

            // The less you have, the more this template is favored
            // At 0 resources → 2.0x weight
            // At 100+ resources → 1.0x weight (no boost)
            float need = Mathf.InverseLerp(100f, 0f, current);
            return 1.0f + need;
        }

        private float getRescueRelevance(UnitRosterState roster) {
            // If units are captured or missing, rescue missions are more relevant
            bool hasCaptures = false;
            foreach (var unit in roster.roster) {
                if (unit.status == UnitStatus.Captured || unit.status == UnitStatus.Missing) {
                    hasCaptures = true;
                    break;
                }
            }

            return hasCaptures ? 1.5f : 0.5f;
        }

        // MARK: - Step 3: Weighted Selection

        private List<MissionTemplate> weightedSelect(List<ScoredTemplate> scored, int count) {
            List<MissionTemplate> selected = new();
            var pool = new List<ScoredTemplate>(scored);

            int picks = Mathf.Min(count, pool.Count);

            for (int i = 0; i < picks; i++) {
                float totalWeight = 0f;
                foreach (var entry in pool) {
                    totalWeight += entry.weight;
                }

                float roll = UnityEngine.Random.Range(0f, totalWeight);
                float cumulative = 0f;

                for (int j = 0; j < pool.Count; j++) {
                    cumulative += pool[j].weight;

                    if (roll <= cumulative) {
                        selected.Add(pool[j].template);
                        pool.RemoveAt(j); // No duplicates
                        break;
                    }
                }
            }

            return selected;
        }

        // MARK: - Step 4: Instantiation

        private MissionData instantiate(MissionTemplate template, float difficultyRatio) {
            string uniqueID = $"{template.templateID}_{Guid.NewGuid().ToString().Substring(0, 8)}";

            var mission = MissionData.CreateGeneric(
                uniqueID,
                template.titleBase,
                template.description,
                template.category
            );

            // Roll enemy count within range, then scale by difficulty
            mission.baseEnemyCount = template.enemyCountRange.scale(
                DifficultyCalculator.getEnemyCountMultiplier(difficultyRatio)
            );

            // Roll XP, scale by difficulty
            int baseXP = template.baseXPPoolRange.roll();
            mission.rewards = MissionRewards.Create(
                baseXPPool: Mathf.RoundToInt(baseXP * DifficultyCalculator.getXPMultiplier(difficultyRatio))
            );

            // Roll resource rewards
            foreach (var kvp in template.resourceRewardRanges) {
                float resourceMult = DifficultyCalculator.getResourceMultiplier(difficultyRatio);
                int amount = kvp.Value.scale(resourceMult);
                mission.rewards.resources[kvp.Key] = amount;
            }

            // Primary objectives are always the same
            mission.primaryObjectives = template.primaryObjectives;

            // Pick random optional objectives from the pool
            mission.optionalObjectives = pickOptionalObjectives(
                template.optionalObjectivePool,
                template.optionalObjectiveCount
            );

            // Scale recommended squad level with threat
            mission.recommendedSquadLevel = Mathf.RoundToInt(difficultyRatio * 3f);

            return mission;
        }

        private ObjectiveData[] pickOptionalObjectives(ObjectiveData[] pool, int count) {
            if (pool.Length == 0 || count == 0) return new ObjectiveData[0];

            int picks = Mathf.Min(count, pool.Length);

            // Shuffle the pool
            var shuffled = new List<ObjectiveData>(pool);
            for (int i = shuffled.Count - 1; i > 0; i--) {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            var selected = new ObjectiveData[picks];
            for (int i = 0; i < picks; i++) {
                selected[i] = shuffled[i];
            }

            return selected;
        }

        // MARK: - Removal

        public void removeFromRotation(string missionID) {
            currentMissions.RemoveAll(m => m.id == missionID);
        }

        // MARK: - Internal Types

        private struct ScoredTemplate {
            public MissionTemplate template;
            public float weight;
        }

    }

}
```

## How Weighted Selection Works

Each template gets a **weight** that starts at its `baseWeight` and gets multiplied by context:

```
Template: Supply Raid
  baseWeight:        1.5
  × resource need:   1.8  (player is low on currency)
  = final weight:    2.7

Template: Recon Patrol
  baseWeight:        1.2
  × resource need:   1.0  (player has plenty of intel)
  = final weight:    1.2

Template: Rescue Operation
  baseWeight:        0.8
  × rescue relevance: 0.5  (nobody is captured)
  = final weight:    0.4
```

Weighted random selection picks from this distribution. Supply Raid has a `2.7 / (2.7 + 1.2 + 0.4) = 63%` chance of being picked first. The player sees more of what they need without it being deterministic.

## Procedural Variation Breakdown

Each instance of the same template differs in:

| What Varies | How |
|-------------|-----|
| Enemy count | `enemyCountRange.roll()` × difficulty multiplier |
| XP reward | `baseXPPoolRange.roll()` × XP multiplier |
| Resource amounts | Per-resource `VariationRange.roll()` × resource multiplier |
| Optional objectives | Random subset picked from `optionalObjectivePool` |
| Unique ID | GUID suffix prevents collisions |

A "Supply Raid" might spawn with 3 enemies and 45 currency one time, and 6 enemies and 110 currency the next. Same template, different feel.

## Tuning Guide

**If players always see the same mission types:**
- Check `baseWeight` values — high-weight templates dominate
- Add more templates to the library
- Reduce the resource need multiplier range

**If the board feels too easy:**
- Increase `minimumThreatLevel` on lower templates so they phase out
- Raise enemy count ranges
- Increase `DAILY_THREAT_GROWTH` in ProgressionConstants

**If the board feels too hard:**
- Lower enemy count ranges
- Increase reward ranges (more reward per risk)
- Add more low-threat templates

**Checkpoint:** Create this file and verify it compiles. You should be able to call `generate()` with the current game state and get a list of varied, contextually relevant missions.
