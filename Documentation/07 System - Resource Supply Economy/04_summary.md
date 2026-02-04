# Phase 7 Summary: Resource & Supply Economy

## Files Created

### Constants
| File | Path |
|------|------|
| EconomyConstants | `Assets/Scripts/Core/Data/Constants/EconomyConstants.cs` |

### Economy Classes
| File | Path |
|------|------|
| ActionCost | `Assets/Scripts/Core/Economy/ActionCost.cs` |
| CostValidator | `Assets/Scripts/Core/Economy/CostValidator.cs` |
| DeploymentCostCalculator | `Assets/Scripts/Core/Economy/DeploymentCostCalculator.cs` |
| ScarcityChecker | `Assets/Scripts/Core/Economy/ScarcityChecker.cs` |

### Events
| File | Path |
|------|------|
| ResourceChangedEvent | `Assets/Scripts/Core/Events/ResourceChangedEvent.cs` |
| ResourceScarcityEvent | `Assets/Scripts/Core/Events/ResourceScarcityEvent.cs` |
| DeploymentCostFailedEvent | `Assets/Scripts/Core/Events/DeploymentCostFailedEvent.cs` |

### Files Modified

| File | Changes |
|------|---------|
| ResourceType.cs | Add `MedicalSupplies`, `Fuel` |
| ResourceState.cs | Add `initializeStartingResources()`, publish `ResourceChangedEvent` on add/spend |
| UnitRosterState.cs | Add `accelerateRecovery()` for medical supply usage |
| MissionManager.cs | Add deployment cost check/deduction in `startMission()` |
| TimeAction.cs | Add `cost` field (ActionCost) |
| StrategicLayerManager.cs | Add cost validation in `startAction()`, passive income in `processTick()` |

## Folder Structure After This Phase

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   │   ├── EconomyConstants.cs        ← NEW
│   │   ├── GameConstants.cs
│   │   ├── LoyaltyConstants.cs
│   │   ├── ProgressionConstants.cs
│   │   └── SaveConstants.cs
│   ├── Enums/
│   │   └── ResourceType.cs            ← MODIFIED
│   └── Structs/
├── Economy/                            ← NEW FOLDER
│   ├── ActionCost.cs
│   ├── CostValidator.cs
│   ├── DeploymentCostCalculator.cs
│   └── ScarcityChecker.cs
├── Events/
│   ├── DeploymentCostFailedEvent.cs   ← NEW
│   ├── ResourceChangedEvent.cs        ← NEW
│   └── ResourceScarcityEvent.cs       ← NEW
├── GameState/
├── Missions/
├── Progression/
├── Squad/
└── SaveSystem/
```

## Resource Flow Summary

```
        INCOME                              EXPENSES
        ──────                              ────────
Mission rewards ────┐              ┌──── Deployment (Fuel)
Passive daily ──────┤              ├──── Research (TechComp)
Optional obj. ──────┤   Resources  ├──── Building (Currency + Alloys)
Difficulty bonus ───┤   ────────►  ├──── Healing (MedSupplies)
                    │              ├──── Equipment (Currency + Alloys)
                    │              └──── Recon/Sabotage costs
```

## Integration Points

| This Phase | Connects To | How |
|-----------|-------------|-----|
| ActionCost on TimeAction | StrategicLayerManager | Validates and deducts before queuing |
| DeploymentCostCalculator | MissionManager | Checks fuel/resources before mission start |
| ResourceChangedEvent | EventBus → UI | Every add/spend broadcasts for live display |
| ResourceScarcityEvent | EventBus → UI | Warns when fuel or medical supplies are low |
| DeploymentCostFailedEvent | EventBus → UI | Shows "cannot afford" messages |
| MedicalSupplies | UnitRosterState | `accelerateRecovery()` spends supplies for faster healing |
| Passive income | StrategicLayerManager | Daily fuel/currency trickle in `processTick()` |
| initializeStartingResources | GameStateManager | Called on new game start |

## What's Stubbed

- **Equipment purchasing** — `ActionCost` can represent buy prices, but no shop UI or inventory system yet
- **Facility construction costs** — `TimeAction.createBuild()` has a cost, but facility definitions don't exist yet
- **Scarcity on defector path** — Plan mentions reduced resource access on the defector path; will be implemented in Phase 8 (Faction & Loyalty) or Phase 12 (Decision & Branching)
- **MaterialType expansion** — Still using `Placeholder`. Will be populated when crafting/equipment systems are designed.

## Checklist

- [ ] Add MedicalSupplies and Fuel to ResourceType enum
- [ ] Create EconomyConstants
- [ ] Add `initializeStartingResources()` to ResourceState
- [ ] Create ActionCost struct
- [ ] Create CostValidator
- [ ] Create DeploymentCostCalculator
- [ ] Create ScarcityChecker
- [ ] Create ResourceChangedEvent, ResourceScarcityEvent, DeploymentCostFailedEvent
- [ ] Update ResourceState.add() and spend() to publish events
- [ ] Add `cost` field to TimeAction, update factory methods
- [ ] Update StrategicLayerManager.startAction() to validate costs
- [ ] Add passive income to StrategicLayerManager.processTick()
- [ ] Add `accelerateRecovery()` to UnitRosterState
- [ ] Update MissionManager.startMission() with deployment cost check
- [ ] Test: Start game, verify starting resources
- [ ] Test: Deploy a mission, verify fuel deducted
- [ ] Test: Try deploying with no fuel, verify rejection event
- [ ] Test: Spend medical supplies, verify recovery acceleration
- [ ] Test: Advance time, verify passive income
