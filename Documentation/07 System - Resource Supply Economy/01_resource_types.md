# Part 1: Resource Types

The economy is built on six resource types, each tied to a specific gameplay pressure.

## Step 1.1: Update ResourceType Enum

Update `Assets/Scripts/Core/Data/Enums/ResourceType.cs`:

```csharp
namespace Game.Core.Data {

    public enum ResourceType {
        Currency,
        Alloys,
        TechComponents,
        Intel,
        MedicalSupplies,
        Fuel
    }

}
```

Two new types: `MedicalSupplies` and `Fuel`. Everything that references `ResourceType` (ResourceState, MissionRewards, etc.) automatically picks these up because ResourceState initializes from `Enum.GetValues`.

## Step 1.2: Resource Roles

| Resource | Earned From | Spent On | Scarcity Pressure |
|----------|-------------|----------|-------------------|
| **Currency** | Most missions, generic raids | Unit upgrades, loadout purchases, facility construction | General-purpose, always needed |
| **Alloys** | Raids, salvage missions | Equipment crafting, facility upgrades, construction | Mid-to-late game bottleneck |
| **TechComponents** | Tech recovery missions, recon | Research prerequisites, advanced equipment | Gates tech progression |
| **Intel** | Recon missions, sabotage | Reveal optional objectives, unlock mission details, strategic reconnaissance | Information advantage |
| **MedicalSupplies** | Rescue missions, supply raids | Accelerate injury recovery | Roster pressure relief |
| **Fuel** | Supply raids, steady income | Mission deployment cost | Mission access gate |

## Step 1.3: Starting Resources

Add starting resource values to a new constant class.

Create `Assets/Scripts/Core/Data/Constants/EconomyConstants.cs`:

```csharp
namespace Game.Core.Data {

    public static class EconomyConstants {

        // Starting resources for new game
        public const int STARTING_CURRENCY = 200;
        public const int STARTING_ALLOYS = 50;
        public const int STARTING_TECH_COMPONENTS = 20;
        public const int STARTING_INTEL = 10;
        public const int STARTING_MEDICAL_SUPPLIES = 30;
        public const int STARTING_FUEL = 100;

        // Deployment costs (per mission)
        public const int DEPLOY_FUEL_BASE = 20;
        public const int DEPLOY_FUEL_PER_UNIT = 5;

        // Recovery acceleration
        public const int MED_SUPPLIES_PER_RECOVERY = 10;
        public const int ACCELERATED_RECOVERY_HOURS = 12;

        // Scarcity warning thresholds
        public const int LOW_FUEL_THRESHOLD = 30;
        public const int LOW_MEDICAL_THRESHOLD = 10;

        // Passive income (per strategic day)
        public const int DAILY_FUEL_INCOME = 5;
        public const int DAILY_CURRENCY_INCOME = 10;

    }

}
```

## Step 1.4: Initialize Starting Resources

Update `ResourceState` initialization or add a setup method. You can call this from `GameStateManager.startNewGame()`:

```csharp
// Add to ResourceState or call externally:

public void initializeStartingResources() {
    set(ResourceType.Currency, EconomyConstants.STARTING_CURRENCY);
    set(ResourceType.Alloys, EconomyConstants.STARTING_ALLOYS);
    set(ResourceType.TechComponents, EconomyConstants.STARTING_TECH_COMPONENTS);
    set(ResourceType.Intel, EconomyConstants.STARTING_INTEL);
    set(ResourceType.MedicalSupplies, EconomyConstants.STARTING_MEDICAL_SUPPLIES);
    set(ResourceType.Fuel, EconomyConstants.STARTING_FUEL);
}
```

## Step 1.5: Passive Income

Some resources trickle in over time on the strategic layer. Add this to `StrategicLayerManager.processTick()`:

```csharp
// In StrategicLayerManager.processTick(), add after recovery tick:

// Passive resource income (daily)
if (gm.progression.totalElapsedHours % ProgressionConstants.HOURS_PER_DAY == 0) {
    gm.resources.add(ResourceType.Fuel, EconomyConstants.DAILY_FUEL_INCOME);
    gm.resources.add(ResourceType.Currency, EconomyConstants.DAILY_CURRENCY_INCOME);
}
```

This gives the player a slow drip of fuel and currency even if they're just waiting. It prevents a total dead-end where you can't afford to deploy at all, while still making missions the primary income source.

## Step 1.6: Medical Supply Usage

Medical supplies let the player speed up injury recovery. This integrates with the recovery system from Phase 6:

```csharp
// Add to UnitRosterState:

public bool accelerateRecovery(string unitID, ResourceState resources) {
    if (!resources.hasResource(ResourceType.MedicalSupplies,
        EconomyConstants.MED_SUPPLIES_PER_RECOVERY)) {
        return false;
    }

    int index = indexOf(unitID);
    if (index < 0) return false;

    UnitData unit = roster[index];
    if (unit.status != UnitStatus.Injured) return false;

    resources.spend(ResourceType.MedicalSupplies, EconomyConstants.MED_SUPPLIES_PER_RECOVERY);

    unit.recoveryHoursRemaining = Math.Max(0,
        unit.recoveryHoursRemaining - EconomyConstants.ACCELERATED_RECOVERY_HOURS);

    if (unit.recoveryHoursRemaining <= 0) {
        unit.status = UnitStatus.Active;
        unit.recoveryHoursRemaining = 0;
    }

    roster[index] = unit;
    return true;
}
```

The player can spend medical supplies to shave 12 hours off a unit's recovery. Use it once on a minor injury and they're back immediately. Use it on a severe injury and they're still recovering but faster. It's a choice: save supplies for an emergency, or get your best soldier back now.

**Checkpoint:** Update `ResourceType` with the two new values. Create `EconomyConstants`. Add `initializeStartingResources()` to `ResourceState`. Add passive income to the strategic tick. Add medical supply usage to `UnitRosterState`.
