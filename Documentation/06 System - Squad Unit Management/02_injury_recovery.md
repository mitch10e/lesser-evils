# Part 2: Injury Recovery

When a unit is injured in a mission, they can't deploy again until they've recovered. Recovery consumes time on the strategic layer — the player must queue a healing action and wait it out, or go into the next mission shorthanded.

## Step 2.1: Recovery Constants

Add to `Assets/Scripts/Core/Data/Constants/GameConstants.cs`:

```csharp
// Add to existing GameConstants class:

// Injury Recovery
public const int RECOVERY_HOURS_MINOR = 24;   // 1 day
public const int RECOVERY_HOURS_MODERATE = 48; // 2 days
public const int RECOVERY_HOURS_SEVERE = 72;   // 3 days
```

## Step 2.2: Recovery Methods on UnitRosterState

Add recovery management to the existing `UnitRosterState.cs`:

```csharp
// Add these methods to UnitRosterState:

// MARK: - Injury Recovery

public void injureUnit(string unitID, int recoveryHours) {
    int index = indexOf(unitID);
    if (index < 0) return;

    UnitData unit = roster[index];
    unit.status = UnitStatus.Injured;
    unit.recoveryHoursRemaining = recoveryHours;
    roster[index] = unit;

    // Can't stay deployed while injured
    deployedUnitIDs.Remove(unitID);
}

public void tickRecovery(int hours) {
    for (int i = 0; i < roster.Count; i++) {
        UnitData unit = roster[i];

        if (unit.status != UnitStatus.Injured) continue;
        if (unit.recoveryHoursRemaining <= 0) continue;

        unit.recoveryHoursRemaining -= hours;

        if (unit.recoveryHoursRemaining <= 0) {
            unit.recoveryHoursRemaining = 0;
            unit.status = UnitStatus.Active;
        }

        roster[i] = unit;
    }
}

public List<UnitData> getRecoveringUnits() {
    return roster.Where(u =>
        u.status == UnitStatus.Injured &&
        u.recoveryHoursRemaining > 0
    ).ToList();
}

public int getRecoveryHours(string unitID) {
    UnitData? unit = get(unitID);
    return unit.HasValue ? unit.Value.recoveryHoursRemaining : 0;
}
```

## Step 2.3: Integration with Strategic Layer

Recovery ticks happen automatically when time passes on the strategic layer. Update `StrategicLayerManager.processTick()`:

```csharp
// In StrategicLayerManager.processTick(), add after advancing world time:

private void processTick(int hours) {
    var gm = GameStateManager.instance;

    // Advance world time and threat
    gm.progression.advanceTime(hours);

    // Tick unit recovery                    ← ADD THIS
    gm.roster.tickRecovery(hours);

    // Tick each active action and handle completions
    foreach (var action in activeActions) {
        bool justCompleted = action.tick(hours);
        if (justCompleted) {
            completeAction(action);
        }
    }

    // Check for world events
    checkWorldEvents();

    // Notify listeners
    onTick?.Invoke(hours);
    EventBus.publish(new TimeAdvancedEvent { hoursAdvanced = hours });
}
```

Recovery ticks alongside everything else — no need for explicit healing TimeActions unless you want to gate recovery behind a specific player choice. With this approach, injured units heal passively as time passes on the strategic layer.

## Step 2.4: Update MissionOutcomeProcessor

Replace the simple status update with proper injury assignment:

```csharp
// In MissionOutcomeProcessor, update applyCasualties():

private static void applyCasualties(MissionRecord record, UnitRosterState roster) {
    foreach (var unitID in record.injuredUnitIDs) {
        // Assign recovery time based on mission severity
        // More turns taken = harsher injuries
        int recoveryHours = record.turnsTaken > 10
            ? GameConstants.RECOVERY_HOURS_SEVERE
            : record.turnsTaken > 6
                ? GameConstants.RECOVERY_HOURS_MODERATE
                : GameConstants.RECOVERY_HOURS_MINOR;

        roster.injureUnit(unitID, recoveryHours);
    }

    foreach (var unitID in record.deadUnitIDs) {
        roster.updateStatus(unitID, UnitStatus.Dead);
    }
}
```

Longer missions produce harsher injuries. A quick 5-turn engagement causes minor wounds (24h recovery). A grueling 12-turn slog causes severe wounds (72h recovery).

## Step 2.5: The Roster Pressure Loop

```
Mission with 6 units deployed
         │
         ▼
2 units injured (48h recovery each)
         │
         ▼
Strategic layer: time passes
         │
         ├── 24h: units still recovering (1 day down)
         ├── 48h: units recovered → Active
         │
         ▼
Next mission: all 6 available again
         OR
Player rushes: deploys with only 4 units
         │
         ▼
Higher risk → more injuries → deeper roster hole
```

This creates the XCOM-style tension: do you wait for everyone to heal, or push forward shorthanded? Waiting means threat grows. Rushing means more casualties.

**Checkpoint:** Add recovery constants to `GameConstants`. Add recovery methods to `UnitRosterState`. Update `StrategicLayerManager.processTick()` to tick recovery. Update `MissionOutcomeProcessor` to assign recovery hours.
