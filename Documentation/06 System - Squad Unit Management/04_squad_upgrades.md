# Part 4: Squad Upgrades and Recruitment

The player's squad starts small and grows over the campaign. Squad size upgrades are a major progression milestone — unlocking an extra slot means more tactical options but also more units to equip and keep healthy.

## Step 4.1: Squad Size Upgrade

Add an upgrade method to the existing `UnitRosterState.cs`:

```csharp
// Add to UnitRosterState:

// MARK: - Squad Upgrades

public bool canUpgradeSquadSize() {
    if (maxSquadSize >= GameConstants.MAX_SQUAD_SIZE_5) return false;
    return true;
}

public int getNextSquadSize() {
    switch (maxSquadSize) {
        case GameConstants.MAX_SQUAD_SIZE_1: return GameConstants.MAX_SQUAD_SIZE_2;
        case GameConstants.MAX_SQUAD_SIZE_2: return GameConstants.MAX_SQUAD_SIZE_3;
        case GameConstants.MAX_SQUAD_SIZE_3: return GameConstants.MAX_SQUAD_SIZE_4;
        case GameConstants.MAX_SQUAD_SIZE_4: return GameConstants.MAX_SQUAD_SIZE_5;
        default: return maxSquadSize;
    }
}

public void upgradeSquadSize() {
    if (!canUpgradeSquadSize()) return;
    maxSquadSize = getNextSquadSize();
}
```

Squad size upgrades are gated by whatever you choose — tech unlocks, story progress, resource cost. The roster just tracks the current max. The gating logic lives in the system that triggers the upgrade.

Example triggers:

```csharp
// Via tech unlock (in TechState completion handler):
if (techID == "squad_tactics_2") {
    GameStateManager.instance.roster.upgradeSquadSize();
}

// Via story mission reward (in MissionOutcomeProcessor):
if (mission.id == "story_05") {
    GameStateManager.instance.roster.upgradeSquadSize();
}
```

## Step 4.2: Recruitment

Create `Assets/Scripts/Core/Squad/RecruitmentManager.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Squad {

    public static class RecruitmentManager {

        // MARK: - Recruitment

        public static UnitData recruit(
            string id,
            string name,
            int level = 1,
            int loyalty = LoyaltyConstants.DEFAULT_LOYALTY
        ) {
            var unit = UnitData.CreateDefault(id, name);
            unit.level = level;
            unit.loyalty = loyalty;

            // Scale XP requirement to level
            unit.experienceToNextLevel = calculateXPForLevel(level + 1);

            return unit;
        }

        public static bool addToRoster(UnitData unit, UnitRosterState roster) {
            // Check if ID already exists
            if (roster.get(unit.id).HasValue) return false;

            roster.add(unit);
            return true;
        }

        // MARK: - Scaled Recruits

        public static UnitData recruitScaled(
            string id,
            string name,
            UnitRosterState roster
        ) {
            // New recruits join at the squad's average level (minus 1)
            // This prevents them from being useless in late-game
            int avgLevel = getAverageActiveLevel(roster);
            int recruitLevel = Math.Max(1, avgLevel - 1);

            return recruit(id, name, recruitLevel);
        }

        // MARK: - Queries

        public static int getAverageActiveLevel(UnitRosterState roster) {
            var active = roster.getActiveUnits();
            if (active.Count == 0) return 1;

            int totalLevel = 0;
            foreach (var unit in active) {
                totalLevel += unit.level;
            }

            return totalLevel / active.Count;
        }

        // MARK: - Private

        private static int calculateXPForLevel(int level) {
            return (int)(GameConstants.BASE_XP_TO_LEVEL *
                Math.Pow(GameConstants.XP_SCALING_PER_LEVEL, level - 1));
        }

    }

}
```

**Scaled recruitment** prevents the "useless rookie" problem. If your squad averages level 8, a new recruit joins at level 7 — behind the curve but not dead weight. They still need to earn their keep, but they can survive their first mission.

## Step 4.3: Recruitment Sources

Recruits can come from several places:

| Source | When | Loyalty |
|--------|------|---------|
| Story events | Narrative moments add specific named units | Varies (set by story) |
| Rescue missions | Successfully extract a prisoner | `DEFAULT_LOYALTY + 10` (grateful) |
| Between-mission recruitment | Strategic layer option | `DEFAULT_LOYALTY` |
| Defection events | Enemy units switch sides | Low (`DEFAULT_LOYALTY - 15`) |

Example — adding a rescued unit after a mission:

```csharp
// In a rescue mission completion handler:
var rescued = RecruitmentManager.recruitScaled(
    "unit_rescued_01",
    "Rescued Operative",
    GameStateManager.instance.roster
);
rescued.loyalty = LoyaltyConstants.DEFAULT_LOYALTY + 10;

RecruitmentManager.addToRoster(rescued, GameStateManager.instance.roster);
```

## Step 4.4: Squad Composition at a Glance

```
Roster (all units you own):
┌────────────────────────────────────────────────┐
│  Rook     Lv8  Active    Loyalty: 72  ██████░░ │
│  Viper    Lv7  Injured   Loyalty: 55  ████░░░░ │ ← 24h left
│  Ghost    Lv9  Active    Loyalty: 88  ████████ │ ← Devoted
│  Blaze    Lv6  Active    Loyalty: 22  ██░░░░░░ │ ← Disobey risk!
│  Wraith   Lv5  Active    Loyalty: 45  ████░░░░ │
│  Flicker  Lv3  Active    Loyalty: 50  ████░░░░ │ ← New recruit
└────────────────────────────────────────────────┘

Deploy for mission (max 5 slots):
┌─────────────────────────┐
│  [1] Rook               │
│  [2] Ghost              │
│  [3] Blaze  ⚠ disobey  │
│  [4] Wraith             │
│  [5] Flicker            │
│                         │
│  Viper unavailable      │
│  (injured, 24h left)    │
└─────────────────────────┘
```

**Checkpoint:** Add squad upgrade methods to `UnitRosterState`. Create `RecruitmentManager`. You should be able to upgrade squad size, recruit new units at scaled levels, and add them to the roster.
