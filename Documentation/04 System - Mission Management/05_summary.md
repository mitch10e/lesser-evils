# Phase 4 Summary: Mission Management

## Files Created

### Enums
| File | Path |
|------|------|
| MissionType | `Assets/Scripts/Core/Data/Enums/MissionType.cs` |
| MissionCategory | `Assets/Scripts/Core/Data/Enums/MissionCategory.cs` |

### Structs
| File | Path |
|------|------|
| ObjectiveData | `Assets/Scripts/Core/Data/Structs/ObjectiveData.cs` |
| MissionRewards | `Assets/Scripts/Core/Data/Structs/MissionRewards.cs` |
| MissionConsequences | `Assets/Scripts/Core/Data/Structs/MissionConsequences.cs` |

### Classes
| File | Path |
|------|------|
| MissionData | `Assets/Scripts/Core/Data/MissionData.cs` |
| MissionPool | `Assets/Scripts/Core/Missions/MissionPool.cs` |
| MissionAvailability | `Assets/Scripts/Core/Missions/MissionAvailability.cs` |
| GenericMissionRotation | `Assets/Scripts/Core/Missions/GenericMissionRotation.cs` |
| MissionManager | `Assets/Scripts/Core/Missions/MissionManager.cs` |
| MissionOutcomeProcessor | `Assets/Scripts/Core/Missions/MissionOutcomeProcessor.cs` |

### Events
| File | Path |
|------|------|
| MissionStartedEvent | `Assets/Scripts/Core/Events/MissionStartedEvent.cs` |

### Existing Files (No Changes Needed)
| File | Status |
|------|--------|
| MissionStatus.cs | Used as-is |
| MissionRecord.cs | Used as-is |
| MainMissionData.cs | Kept for ProgressionState compatibility |
| Choice.cs | Used as-is |
| MissionCompletedEvent.cs | Used as-is |
| ContentUnlocker.cs | Still valid for lightweight checks |

## Folder Structure After This Phase

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   ├── Enums/
│   │   ├── MissionCategory.cs     ← NEW
│   │   ├── MissionStatus.cs
│   │   └── MissionType.cs         ← NEW
│   ├── Structs/
│   │   ├── MissionConsequences.cs  ← NEW
│   │   ├── MissionRecord.cs
│   │   ├── MissionRewards.cs       ← NEW
│   │   └── ObjectiveData.cs        ← NEW
│   └── MissionData.cs              ← NEW
├── Events/
│   ├── MissionCompletedEvent.cs
│   └── MissionStartedEvent.cs      ← NEW
├── GameState/
├── Missions/                        ← NEW FOLDER
│   ├── GenericMissionRotation.cs
│   ├── MissionAvailability.cs
│   ├── MissionManager.cs
│   ├── MissionOutcomeProcessor.cs
│   └── MissionPool.cs
├── Progression/
└── SaveSystem/
```

## Integration Points

| This Phase | Connects To | How |
|-----------|-------------|-----|
| MissionManager | StrategicLayerManager | `enterMission()` / `exitMission()` pauses/resumes time |
| MissionAvailability | ProgressionState | Checks completed missions for prerequisites |
| MissionAvailability | CampaignState | Checks story flags and faction for mission gating |
| GenericMissionRotation | DifficultyCalculator | Scales enemy count and rewards |
| MissionOutcomeProcessor | ResourceState | Applies resource rewards |
| MissionOutcomeProcessor | UnitRosterState | Applies XP, injuries, deaths |
| MissionOutcomeProcessor | CampaignState | Applies story flags |
| MissionOutcomeProcessor | ProgressionState | Marks story missions as completed |
| MissionOutcomeProcessor | TechState | Unlocks tech rewards |

## What's Stubbed

These connect to systems that don't exist yet:

- **Faction loyalty shifts** — `MissionConsequences` tracks them, `MissionOutcomeProcessor` has TODO comments. Will integrate with FactionState (Phase 8).
- **Moral choice processing** — `MissionData.moralChoiceIDs` references choice definitions. `MissionRecord.choicesMade` captures what the player chose. The Moral/Ethics Tracking system (Phase 9) will consume this data.
- **Combat scene loading** — `MissionManager.startMission()` fires events but doesn't load a scene. The Combat Black Box (Phase 13) will own scene transitions.
- **Mission database loading** — `MissionDatabase` example uses hardcoded definitions. A future step could load from ScriptableObjects or JSON.

## Checklist

- [ ] Create enum files (MissionType, MissionCategory)
- [ ] Create struct files (ObjectiveData, MissionRewards, MissionConsequences)
- [ ] Create MissionData class
- [ ] Create Missions folder with MissionPool, MissionAvailability, GenericMissionRotation
- [ ] Create MissionManager MonoBehaviour
- [ ] Create MissionOutcomeProcessor
- [ ] Create MissionStartedEvent
- [ ] Add MissionManager to a persistent GameObject in your scene
- [ ] Test: Register story missions, verify availability gating
- [ ] Test: Start a mission, verify time pauses
- [ ] Test: Complete a mission with a record, verify rewards applied
- [ ] Test: Verify new missions unlock after completing prerequisites
