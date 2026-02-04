# Part 1: Unit Loyalty

Loyalty is a per-unit score that determines how reliably a unit follows orders. High-loyalty units are dependable. Low-loyalty units may refuse morally questionable orders or defect during critical moments.

## Step 1.1: Update UnitData

Add loyalty fields to the existing `UnitData` struct in `Assets/Scripts/Core/Data/Structs/UnitData.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct UnitData {

        public string id;

        public string displayName;

        public int level;

        public int experience;

        public int experienceToNextLevel;

        public UnitStatus status;

        public string loadoutID;

        public bool isVIP;

        // NEW: Loyalty
        public int loyalty;

        // NEW: Recovery
        public int recoveryHoursRemaining;

        public static UnitData CreateDefault(string id, string name) {
            return new UnitData {
                id = id,
                displayName = name,
                level = 1,
                experience = 0,
                experienceToNextLevel = 100,
                status = UnitStatus.Active,
                loadoutID = "",
                isVIP = false,
                loyalty = LoyaltyConstants.DEFAULT_LOYALTY,
                recoveryHoursRemaining = 0
            };
        }

    }

}
```

## Step 1.2: Loyalty Constants

Create `Assets/Scripts/Core/Data/Constants/LoyaltyConstants.cs`:

```csharp
namespace Game.Core.Data {

    public static class LoyaltyConstants {

        public const int MIN_LOYALTY = 0;
        public const int MAX_LOYALTY = 100;
        public const int DEFAULT_LOYALTY = 50;

        // Thresholds
        public const int LOYALTY_DEFECTION_RISK = 15;
        public const int LOYALTY_DISOBEY_RISK = 30;
        public const int LOYALTY_DEVOTED = 80;

        // Shift amounts
        public const int MORAL_CHOICE_SHIFT = 5;
        public const int MISSION_SUCCESS_BONUS = 3;
        public const int MISSION_FAILURE_PENALTY = 2;
        public const int COMRADE_DEATH_PENALTY = 5;

    }

}
```

**Threshold meanings:**

| Range | Behavior |
|-------|----------|
| 0–15 | **Defection risk** — Unit may leave the roster between missions |
| 16–30 | **Disobey risk** — Unit may refuse morally costly orders in combat |
| 31–79 | **Normal** — Reliable, follows orders |
| 80–100 | **Devoted** — Bonus morale effects, never refuses orders |

## Step 1.3: Loyalty Methods on UnitRosterState

Add loyalty management to the existing `UnitRosterState.cs`:

```csharp
// Add these methods to UnitRosterState:

// MARK: - Loyalty

public void shiftLoyalty(string unitID, int amount) {
    int index = indexOf(unitID);
    if (index < 0) return;

    UnitData unit = roster[index];
    unit.loyalty = Math.Clamp(
        unit.loyalty + amount,
        LoyaltyConstants.MIN_LOYALTY,
        LoyaltyConstants.MAX_LOYALTY
    );
    roster[index] = unit;
}

public void shiftAllLoyalty(int amount) {
    for (int i = 0; i < roster.Count; i++) {
        UnitData unit = roster[i];
        unit.loyalty = Math.Clamp(
            unit.loyalty + amount,
            LoyaltyConstants.MIN_LOYALTY,
            LoyaltyConstants.MAX_LOYALTY
        );
        roster[i] = unit;
    }
}

public bool isDefectionRisk(string unitID) {
    UnitData? unit = get(unitID);
    return unit.HasValue && unit.Value.loyalty <= LoyaltyConstants.LOYALTY_DEFECTION_RISK;
}

public bool isDisobeyRisk(string unitID) {
    UnitData? unit = get(unitID);
    return unit.HasValue && unit.Value.loyalty <= LoyaltyConstants.LOYALTY_DISOBEY_RISK;
}

public bool isDevoted(string unitID) {
    UnitData? unit = get(unitID);
    return unit.HasValue && unit.Value.loyalty >= LoyaltyConstants.LOYALTY_DEVOTED;
}

public List<UnitData> getDefectionRisks() {
    return roster.Where(u =>
        u.loyalty <= LoyaltyConstants.LOYALTY_DEFECTION_RISK &&
        u.status != UnitStatus.Dead
    ).ToList();
}
```

## Step 1.4: When Loyalty Changes

Loyalty shifts happen at specific moments, not continuously:

| Event | Effect | Where It's Applied |
|-------|--------|--------------------|
| Moral choice in mission | +/- `MORAL_CHOICE_SHIFT` per unit | MissionOutcomeProcessor |
| Mission success | +`MISSION_SUCCESS_BONUS` to deployed units | MissionOutcomeProcessor |
| Mission failure | -`MISSION_FAILURE_PENALTY` to deployed units | MissionOutcomeProcessor |
| Comrade killed in action | -`COMRADE_DEATH_PENALTY` to all deployed | MissionOutcomeProcessor |
| Story events | Variable, defined per event | CampaignState story flags |

## Step 1.5: Update MissionOutcomeProcessor

Add loyalty shifts to `MissionOutcomeProcessor.process()`:

```csharp
// Add this call in MissionOutcomeProcessor.process():
applyLoyaltyShifts(mission, record, gm);

// Add this method:
private static void applyLoyaltyShifts(
    MissionData mission,
    MissionRecord record,
    GameStateManager gm
) {
    // Success/failure shift to deployed units
    int baseShift = record.wasSuccessful
        ? LoyaltyConstants.MISSION_SUCCESS_BONUS
        : -LoyaltyConstants.MISSION_FAILURE_PENALTY;

    foreach (var unitID in gm.roster.deployedUnitIDs) {
        gm.roster.shiftLoyalty(unitID, baseShift);
    }

    // Comrade death penalty
    if (record.deadUnitIDs.Count > 0) {
        foreach (var unitID in gm.roster.deployedUnitIDs) {
            // Don't penalize dead units themselves
            if (!record.deadUnitIDs.Contains(unitID)) {
                gm.roster.shiftLoyalty(unitID, -LoyaltyConstants.COMRADE_DEATH_PENALTY);
            }
        }
    }

    // Moral choice shifts (applied by the moral system in Phase 9)
    // For now, choices are recorded in MissionRecord.choicesMade
    // and will affect loyalty when that system comes online
}
```

## Step 1.6: Defection Check

Add a method that `MissionManager` can call between missions to handle defections:

```csharp
// Add to UnitRosterState:

public List<string> checkDefections() {
    List<string> defected = new();

    foreach (var unit in roster) {
        if (unit.status == UnitStatus.Dead) continue;
        if (unit.isVIP) continue; // VIPs don't defect

        if (unit.loyalty <= LoyaltyConstants.LOYALTY_DEFECTION_RISK) {
            // Roll for defection — lower loyalty = higher chance
            float defectionChance = 1.0f - (unit.loyalty / (float)LoyaltyConstants.LOYALTY_DEFECTION_RISK);
            if (UnityEngine.Random.value < defectionChance) {
                defected.Add(unit.id);
            }
        }
    }

    // Remove defected units
    foreach (var unitID in defected) {
        remove(unitID);
    }

    return defected;
}
```

Defection is probabilistic — a unit at loyalty 5 has a much higher chance than one at loyalty 14. VIPs are exempt (they're narratively protected). The caller gets back a list of who left so the UI can show a notification.

**Checkpoint:** Update `UnitData` with loyalty and recovery fields. Create `LoyaltyConstants`. Add loyalty methods to `UnitRosterState`. Update `MissionOutcomeProcessor` with loyalty shifts.
