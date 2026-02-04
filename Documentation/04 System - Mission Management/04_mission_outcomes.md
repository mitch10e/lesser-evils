# Part 4: Mission Outcome Processing

When a mission ends, the game needs to take the `MissionRecord` (what happened) and the `MissionData` (what was promised) and apply the results across every relevant system. This class is the single point where mission results flow back into GameState.

## Step 4.1: Outcome Processor

Create `Assets/Scripts/Core/Missions/MissionOutcomeProcessor.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Progression;

namespace Game.Core.Missions {

    public static class MissionOutcomeProcessor {

        private const int OPTIONAL_OBJECTIVE_BONUS_XP = 10;
        private const int OPTIONAL_OBJECTIVE_BONUS_CURRENCY = 15;
        private const float FAILURE_XP_MULTIPLIER = 0.25f;

        public static void process(MissionData mission, MissionRecord record) {
            var gm = GameStateManager.instance;

            applyCasualties(record, gm.roster);

            if (record.wasSuccessful) {
                applyRewards(mission, record, gm);
                applyCollectedLoot(record, gm);
                applySuccessConsequences(mission, gm);
            } else {
                applyFailureConsequences(mission, gm);
            }

            distributeXP(mission, record, gm);
        }

        // MARK: - Casualties

        private static void applyCasualties(MissionRecord record, UnitRosterState roster) {
            foreach (var unitID in record.injuredUnitIDs) {
                roster.updateStatus(unitID, UnitStatus.Injured);
            }

            foreach (var unitID in record.deadUnitIDs) {
                roster.updateStatus(unitID, UnitStatus.Dead);
            }
        }

        // MARK: - Rewards

        private static void applyRewards(
            MissionData mission,
            MissionRecord record,
            GameStateManager gm
        ) {
            // Base resource rewards
            foreach (var kvp in mission.rewards.resources) {
                gm.resources.add(kvp.Key, kvp.Value);
            }

            // Base material rewards (guaranteed)
            foreach (var kvp in mission.rewards.materials) {
                gm.materials.add(kvp.Key, kvp.Value);
            }

            // Tech unlocks
            foreach (var techID in mission.rewards.techUnlockIDs) {
                gm.technology.unlockTech(techID);
            }

            // Optional objective currency bonus
            if (record.completedOptionalObjectives.Count > 0) {
                int bonusCurrency = OPTIONAL_OBJECTIVE_BONUS_CURRENCY
                    * record.completedOptionalObjectives.Count;
                gm.resources.add(ResourceType.Currency, bonusCurrency);
            }

            // Difficulty-scaled resource bonus (catch-up mechanic)
            applyDifficultyResourceBonus(mission, gm);
        }

        private static void applyDifficultyResourceBonus(MissionData mission, GameStateManager gm) {
            float ratio = DifficultyCalculator.getDifficultyRatio(
                gm.progression,
                gm.roster,
                gm.technology
            );

            if (ratio <= 1.0f) return;

            float resourceMultiplier = DifficultyCalculator.getResourceMultiplier(ratio);

            foreach (var kvp in mission.rewards.resources) {
                int bonus = Mathf.RoundToInt(kvp.Value * (resourceMultiplier - 1.0f));
                if (bonus > 0) {
                    gm.resources.add(kvp.Key, bonus);
                }
            }
        }

        // MARK: - Collected Loot

        private static void applyCollectedLoot(MissionRecord record, GameStateManager gm) {
            foreach (var drop in record.collectedLoot) {
                switch (drop.category) {
                    case LootCategory.Material:
                        gm.materials.add(drop.materialType, drop.quantity);
                        break;
                    case LootCategory.Equipment:
                        // TODO: Add to equipment inventory when system exists
                        Debug.Log($"Collected equipment: {drop.equipmentID}");
                        break;
                }
            }
        }

        // MARK: - XP Distribution (Performance-Based)

        private static void distributeXP(
            MissionData mission,
            MissionRecord record,
            GameStateManager gm
        ) {
            if (record.unitPerformances.Count == 0) return;

            // Build total XP pool
            int xpPool = mission.rewards.baseXPPool;

            // Optional objective bonus adds to pool
            xpPool += OPTIONAL_OBJECTIVE_BONUS_XP * record.completedOptionalObjectives.Count;

            // Difficulty catch-up multiplier
            float ratio = DifficultyCalculator.getDifficultyRatio(
                gm.progression,
                gm.roster,
                gm.technology
            );
            if (ratio > 1.0f) {
                float xpMultiplier = DifficultyCalculator.getXPMultiplier(ratio);
                xpPool = Mathf.RoundToInt(xpPool * xpMultiplier);
            }

            // Failure penalty — reduced pool, still distributed by performance
            if (!record.wasSuccessful) {
                xpPool = Mathf.RoundToInt(xpPool * FAILURE_XP_MULTIPLIER);
            }

            // Sum contribution scores
            int totalContribution = 0;
            foreach (var perf in record.unitPerformances) {
                totalContribution += perf.getContributionScore();
            }

            int unitCount = record.unitPerformances.Count;
            int equalShare = xpPool / unitCount;

            foreach (var perf in record.unitPerformances) {
                int unitXP;
                int score = perf.getContributionScore();

                if (totalContribution == 0) {
                    // Everyone did nothing — split equally
                    unitXP = equalShare;
                } else if (score == 0) {
                    // This unit contributed nothing — minimum participation
                    unitXP = Mathf.RoundToInt(
                        equalShare * UnitPerformance.MIN_PARTICIPATION_SHARE
                    );
                } else {
                    // Proportional to contribution
                    unitXP = Mathf.RoundToInt(xpPool * ((float)score / totalContribution));
                }

                gm.roster.addExperience(perf.unitID, Mathf.Max(unitXP, 1));
            }
        }

        // MARK: - Consequences

        private static void applySuccessConsequences(MissionData mission, GameStateManager gm) {
            foreach (var flag in mission.consequences.storyFlagsOnSuccess) {
                gm.campaign.addStoryFlag(flag);
            }

            foreach (var kvp in mission.consequences.factionShiftsOnSuccess) {
                // TODO: Apply to FactionState when implemented
                // gm.factions.shiftLoyalty(kvp.Key, kvp.Value);
            }

            if (mission.consequences.storyProgressOnSuccess > 0f) {
                gm.progression.completeMainMission(mission.id);
            }
        }

        private static void applyFailureConsequences(MissionData mission, GameStateManager gm) {
            foreach (var flag in mission.consequences.storyFlagsOnFailure) {
                gm.campaign.addStoryFlag(flag);
            }

            foreach (var kvp in mission.consequences.factionShiftsOnFailure) {
                // TODO: Apply to FactionState when implemented
                // gm.factions.shiftLoyalty(kvp.Key, kvp.Value);
            }
        }

    }

}
```

## Step 4.2: Outcome Flow

```
MissionRecord (from combat)
       │
       ▼
MissionOutcomeProcessor.process()
       │
       ├─── applyCasualties()
       │      └► UnitRosterState: injured/dead status updates
       │
       ├─── applyRewards() (success only)
       │      ├► ResourceState: currency, alloys, intel, etc.
       │      ├► MaterialState: guaranteed crafting materials
       │      ├► TechState: tech unlocks
       │      ├► Optional objective currency bonus
       │      └► Difficulty catch-up resource bonus
       │
       ├─── applyCollectedLoot() (success only)
       │      ├► MaterialState: retrieved material drops
       │      └► Equipment inventory: retrieved gear (future)
       │
       ├─── distributeXP()
       │      ├► Builds XP pool: base + optional bonuses + difficulty scaling
       │      ├► Applies failure penalty (25%) if mission failed
       │      └► UnitRosterState: XP per unit weighted by UnitPerformance
       │         (kills, damage, objectives → contribution score)
       │
       └─── applyConsequences()
              ├► CampaignState: story flags (different for success/failure)
              ├► ProgressionState: story progress (story missions)
              └► FactionState: loyalty shifts (when implemented)
```

## Step 4.3: Design Decisions

**Casualties apply regardless of success/failure.** A pyrrhic victory still costs you soldiers. A retreat (failure) might still result in injuries.

**XP is performance-based.** The mission defines a `baseXPPool` that gets distributed based on each unit's `UnitPerformance` contribution score (kills, damage dealt, objectives completed). Units that contribute more level faster. Units with zero contribution still get `MIN_PARTICIPATION_SHARE` (10% of an equal share) — they were deployed, they learn something.

**All XP bonuses feed the pool, not individual units.** Optional objective bonuses and difficulty catch-up multipliers increase the total pool before distribution. This keeps the performance weighting consistent across all XP sources.

**Partial XP on failure (25%).** The entire pool is reduced before distribution. The squad learns from defeat, and the performance weighting still applies — the unit that fought hardest in a losing battle still gets the biggest share of the reduced pool.

**Loot is only applied if retrieved.** `MissionRewards.potentialDrops` defines what CAN drop. The combat system determines what actually appears and whether the player picked it up. Only `MissionRecord.collectedLoot` gets applied to inventory. This creates a risk/reward tradeoff during tactical play.

**Equipment drops are stubbed.** The `LootCategory.Equipment` path logs the item ID but doesn't add to any inventory yet. When the equipment/loadout system is built, this is where it hooks in.

**Difficulty bonus is additive, not multiplicative.** At difficulty ratio 1.5, you get `+25%` XP and `+12.5%` resources (based on `DifficultyCalculator` scalers). This helps struggling players catch up without making easy missions feel unrewarding.

**Faction shifts are stubbed.** The `TODO` comments mark where the integration goes. The consequence data is already being tracked so nothing needs to change when factions come online.

**Story progress uses `completeMainMission()` on ProgressionState.** This is the same method from Phase 3 — it adds the mission ID to the completed list. The organic progression (world threat vs player power) does the rest.

## Step 4.4: End-to-End Example

```csharp
// 1. Player selects a mission from the board
MissionManager.instance.startMission("story_02");

// 2. Combat happens (future system)...

// 3. Combat system produces a record
var record = MissionRecord.Create("story_02");
record.wasSuccessful = true;
record.completedOptionalObjectives.Add("no_casualties");
record.injuredUnitIDs.Add("unit_viper");
record.turnsTaken = 8;
record.resourcesGained[ResourceType.Alloys] = 30;

// Per-unit combat performance
var perfA = UnitPerformance.Create("unit_alpha");
perfA.kills = 5;
perfA.damageDealt = 200;
record.unitPerformances.Add(perfA);

var perfB = UnitPerformance.Create("unit_viper");
perfB.kills = 2;
perfB.damageDealt = 80;
record.unitPerformances.Add(perfB);

var perfC = UnitPerformance.Create("unit_echo");
perfC.damageDealt = 30;
record.unitPerformances.Add(perfC);

// Player retrieved a power core but skipped the alloy fragments
record.collectedLoot.Add(LootDrop.CreateEquipment("power_core_mk1"));

// 4. Mission completes — everything cascades
MissionManager.instance.completeMission(record);

// What just happened:
// - unit_viper marked as Injured in UnitRosterState
// - 30 Alloys + 100 Currency added to ResourceState (from mission definition)
// - XP pool (75 base + 10 optional bonus = 85) distributed by performance:
//     unit_alpha: score 70 → ~55 XP (contributed most)
//     unit_viper: score 30 → ~23 XP
//     unit_echo:  score 30 → ~23 XP
// - power_core_mk1 logged for equipment system (future)
// - Story flags added to CampaignState
// - story_02 marked as completed in ProgressionState
// - New missions checked for availability
// - Time resumes on strategic layer
```

**Checkpoint:** Create this file and verify it compiles. The outcome processor is the bridge between combat results and the rest of the game.
