# Phase 6 Summary: Squad & Unit Management

## Files Created

### Constants
| File | Path |
|------|------|
| LoyaltyConstants | `Assets/Scripts/Core/Data/Constants/LoyaltyConstants.cs` |

### Enums
| File | Path |
|------|------|
| EquipmentSlot | `Assets/Scripts/Core/Data/Enums/EquipmentSlot.cs` |

### Structs
| File | Path |
|------|------|
| EquipmentData | `Assets/Scripts/Core/Data/Structs/EquipmentData.cs` |
| LoadoutData | `Assets/Scripts/Core/Data/Structs/LoadoutData.cs` |

### Classes
| File | Path |
|------|------|
| LoadoutManager | `Assets/Scripts/Core/Squad/LoadoutManager.cs` |
| RecruitmentManager | `Assets/Scripts/Core/Squad/RecruitmentManager.cs` |

### Files Modified

| File | Changes |
|------|---------|
| UnitData.cs | Add `loyalty`, `recoveryHoursRemaining` fields |
| UnitRosterState.cs | Add loyalty methods, recovery methods, squad upgrade methods |
| GameConstants.cs | Add `RECOVERY_HOURS_MINOR/MODERATE/SEVERE` |
| MissionOutcomeProcessor.cs | Add `applyLoyaltyShifts()`, update `applyCasualties()` with recovery hours |
| StrategicLayerManager.cs | Add `gm.roster.tickRecovery(hours)` to `processTick()` |

## Folder Structure After This Phase

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   │   ├── GameConstants.cs          ← MODIFIED
│   │   ├── LoyaltyConstants.cs       ← NEW
│   │   ├── ProgressionConstants.cs
│   │   └── SaveConstants.cs
│   ├── Enums/
│   │   ├── EquipmentSlot.cs          ← NEW
│   │   └── ...
│   └── Structs/
│       ├── EquipmentData.cs          ← NEW
│       ├── LoadoutData.cs            ← NEW
│       ├── UnitData.cs               ← MODIFIED
│       └── ...
├── Squad/                             ← NEW FOLDER
│   ├── LoadoutManager.cs
│   └── RecruitmentManager.cs
└── ...
```

## Integration Points

| This Phase | Connects To | How |
|-----------|-------------|-----|
| Loyalty shifts | MissionOutcomeProcessor | Success/failure/death affect deployed unit loyalty |
| Defection check | MissionManager | Call between missions to handle low-loyalty departures |
| Injury recovery | StrategicLayerManager | `tickRecovery()` in `processTick()` heals passively |
| Recovery hours | MissionOutcomeProcessor | Injury severity based on mission length |
| Loadout stats | Combat System (Phase 13) | `statModifiers` drive combat calculations |
| Squad size | Tech/Story rewards | `upgradeSquadSize()` triggered by progression |
| Scaled recruitment | Rescue missions | New units join at squad average level - 1 |

## What's Stubbed

- **Moral choice loyalty shifts** — `MissionRecord.choicesMade` is tracked but loyalty impact deferred to Phase 9 (Moral/Ethics Tracking)
- **Equipment acquisition** — `EquipmentData` defines items but no shop/crafting system yet
- **Loadout costs** — Equipping gear is free; resource costs can be added when the economy matures
- **Combat stat interpretation** — `statModifiers` dictionary exists but combat system (Phase 13) defines the keys

## Checklist

- [ ] Update UnitData with loyalty and recoveryHoursRemaining
- [ ] Create LoyaltyConstants
- [ ] Add loyalty methods to UnitRosterState
- [ ] Add recovery methods to UnitRosterState
- [ ] Add squad upgrade methods to UnitRosterState
- [ ] Add recovery constants to GameConstants
- [ ] Create EquipmentSlot enum
- [ ] Create EquipmentData and LoadoutData structs
- [ ] Create LoadoutManager
- [ ] Create RecruitmentManager
- [ ] Update MissionOutcomeProcessor with loyalty shifts and recovery hours
- [ ] Update StrategicLayerManager.processTick() to tick recovery
- [ ] Test: Injure a unit, advance time, verify they recover
- [ ] Test: Shift loyalty below threshold, run defection check
- [ ] Test: Recruit a scaled unit, verify level is appropriate
- [ ] Test: Upgrade squad size, deploy more units
