# Part 2: Difficulty Scaling System

The difficulty system compares player power to world threat, creating organic challenge scaling.

## Step 2.1: Player Power Estimator

Create `Assets/Scripts/Core/Progression/PlayerPowerEstimator.cs`:

```csharp
using System.Linq;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class PlayerPowerEstimator {

        public static float calculate(UnitRosterState roster, TechState tech) {
            float squadPower = calculateSquadPower(roster);
            float techPower = calculateTechPower(tech);

            return (squadPower * 0.7f) + (techPower * 0.3f);
        }

        private static float calculateSquadPower(UnitRosterState roster) {
            var activeUnits = roster.getActiveUnits();

            if (activeUnits.Count == 0) {
                return ProgressionConstants.BASE_THREAT_LEVEL;
            }

            // Squad size contribution
            float sizeScore = activeUnits.Count / (float)GameConstants.MAX_SQUAD_SIZE_3;

            // Average level contribution
            float avgLevel = activeUnits.Average(u => u.level);
            float levelScore = avgLevel / (float)GameConstants.MAX_UNIT_LEVEL;

            return ProgressionConstants.BASE_THREAT_LEVEL +
                (sizeScore * ProgressionConstants.SQUAD_SIZE_WEIGHT) +
                (levelScore * ProgressionConstants.AVERAGE_LEVEL_WEIGHT);
        }

        private static float calculateTechPower(TechState tech) {
            // Estimate based on number of unlocked techs
            // Adjust divisor based on your total tech count
            const float estimatedTotalTechs = 20f;

            float techScore = tech.unlockedTechIDs.Count / estimatedTotalTechs;

            return techScore * ProgressionConstants.TECH_UNLOCKS_WEIGHT;
        }

    }

}
```

**Design Note:** This is an approximation. The goal isn't perfect accuracy—it's to create a reasonable sense of whether the player is keeping pace.

## Step 2.2: Difficulty Calculator

Create `Assets/Scripts/Core/Progression/DifficultyCalculator.cs`:

```csharp
using System;
using Game.Core.Data;
using Game.Core.States;
using UnityEngine;

namespace Game.Core.Progression {

    public static class DifficultyCalculator {

        // MARK: - Core Calculations

        public static float getDifficultyRatio(
            ProgressionState progression,
            UnitRosterState roster,
            TechState tech
        ) {
            float playerPower = PlayerPowerEstimator.calculate(roster, tech);
            float worldThreat = progression.worldThreatLevel;

            // Avoid division by zero
            if (playerPower <= 0) {
                return ProgressionConstants.DIFFICULTY_PUNISHING;
            }

            return worldThreat / playerPower;
        }

        public static DifficultyTier getDifficultyTier(float ratio) {
            if (ratio < ProgressionConstants.DIFFICULTY_COMFORTABLE) {
                return DifficultyTier.Easy;
            } else if (ratio < ProgressionConstants.DIFFICULTY_BALANCED) {
                return DifficultyTier.Comfortable;
            } else if (ratio < ProgressionConstants.DIFFICULTY_CHALLENGING) {
                return DifficultyTier.Balanced;
            } else if (ratio < ProgressionConstants.DIFFICULTY_PUNISHING) {
                return DifficultyTier.Challenging;
            } else {
                return DifficultyTier.Punishing;
            }
        }

        // MARK: - Enemy Scaling

        public static float getEnemyHealthMultiplier(float difficultyRatio) {
            // Scale enemy health based on how far behind/ahead the player is
            // ratio 1.0 = 1.0x health
            // ratio 1.5 = 1.25x health (enemies tougher)
            // ratio 0.5 = 0.875x health (enemies weaker)
            return Mathf.Lerp(0.75f, 1.5f, Mathf.InverseLerp(0.5f, 1.5f, difficultyRatio));
        }

        public static float getEnemyDamageMultiplier(float difficultyRatio) {
            // Similar scaling for damage
            return Mathf.Lerp(0.8f, 1.4f, Mathf.InverseLerp(0.5f, 1.5f, difficultyRatio));
        }

        public static int getEnemyLevelBonus(float difficultyRatio) {
            // Add bonus levels to enemies when player falls behind
            if (difficultyRatio <= 1.0f) return 0;

            float excess = difficultyRatio - 1.0f;
            return Mathf.FloorToInt(excess * 5); // +1 level per 0.2 ratio
        }

        public static float getEnemyCountMultiplier(float difficultyRatio) {
            // Optionally spawn more enemies when difficulty is high
            // Capped to avoid overwhelming spawns
            return Mathf.Clamp(difficultyRatio, 0.8f, 1.3f);
        }

        // MARK: - Reward Scaling

        public static float getXPMultiplier(float difficultyRatio) {
            // Bonus XP for fighting tougher enemies (catch-up mechanic)
            if (difficultyRatio <= 1.0f) return 1.0f;

            return 1.0f + ((difficultyRatio - 1.0f) * 0.5f);
        }

        public static float getResourceMultiplier(float difficultyRatio) {
            // Slightly more resources when behind (softer catch-up)
            if (difficultyRatio <= 1.0f) return 1.0f;

            return 1.0f + ((difficultyRatio - 1.0f) * 0.25f);
        }

    }

    public enum DifficultyTier {
        Easy,        // Player significantly ahead
        Comfortable, // Player ahead
        Balanced,    // On pace
        Challenging, // Player behind
        Punishing    // Player significantly behind
    }

}
```

## Step 2.3: Using Difficulty in Combat

Example of how the mission system might use these calculations:

```csharp
public class MissionSetup {

    public void configureEnemies(List<EnemyData> enemies) {
        var gm = GameStateManager.instance;
        float ratio = DifficultyCalculator.getDifficultyRatio(
            gm.progression,
            gm.roster,
            gm.technology
        );

        float healthMult = DifficultyCalculator.getEnemyHealthMultiplier(ratio);
        float damageMult = DifficultyCalculator.getEnemyDamageMultiplier(ratio);
        int levelBonus = DifficultyCalculator.getEnemyLevelBonus(ratio);

        foreach (var enemy in enemies) {
            enemy.maxHealth = (int)(enemy.baseHealth * healthMult);
            enemy.damage = (int)(enemy.baseDamage * damageMult);
            enemy.level += levelBonus;
        }
    }

}
```

## Difficulty Feedback (Subtle)

The system creates pressure without explicit notifications:

| Player Perception | Actual Mechanics |
|-------------------|------------------|
| "Enemies feel tougher lately" | World threat outpacing player power |
| "I should do more main missions" | Organic urgency from difficulty |
| "Side missions are getting risky" | High difficulty ratio |
| "I feel powerful" | Player ahead of threat curve |

## Tuning Tips

**If players are always struggling:**
- Decrease `THREAT_GROWTH_PER_DAY`
- Increase catch-up XP/resource multipliers
- Lower enemy scaling caps

**If players never feel pressure:**
- Increase `THREAT_GROWTH_PER_DAY`
- Raise enemy scaling multipliers
- Decrease player power estimation

**Checkpoint:** Create these files and verify they compile. The difficulty system is now ready to integrate.
