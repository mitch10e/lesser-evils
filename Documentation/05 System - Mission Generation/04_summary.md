# Phase 5 Summary: Mission Generation

## Files Created

### New Folder: `Assets/Scripts/Core/Missions/Generation/`

| File | Purpose |
|------|---------|
| VariationRange.cs | Min/max range struct with `roll()` and `scale()` |
| MissionTemplate.cs | Blueprint with variation ranges, threat gating, selection weight |
| MissionTemplateLibrary.cs | Static library of all generic mission templates |
| MissionGenerator.cs | Weighted selection, resource-aware scoring, procedural instantiation |

### Files Modified

| File | Change |
|------|--------|
| MissionManager.cs | Replace `GenericMissionRotation` with `MissionGenerator`, add `ResourceState` to generation call |

### Files Deleted

| File | Reason |
|------|--------|
| GenericMissionRotation.cs | Replaced by MissionGenerator |

## Folder Structure After This Phase

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   ├── Enums/
│   │   ├── MissionCategory.cs
│   │   ├── MissionStatus.cs
│   │   └── MissionType.cs
│   ├── Structs/
│   │   ├── MissionConsequences.cs
│   │   ├── MissionRecord.cs
│   │   ├── MissionRewards.cs
│   │   └── ObjectiveData.cs
│   └── MissionData.cs
├── Events/
├── GameState/
├── Missions/
│   ├── Generation/                    ← NEW FOLDER
│   │   ├── MissionGenerator.cs
│   │   ├── MissionTemplate.cs
│   │   ├── MissionTemplateLibrary.cs
│   │   └── VariationRange.cs
│   ├── MissionAvailability.cs
│   ├── MissionManager.cs             ← MODIFIED
│   ├── MissionOutcomeProcessor.cs
│   └── MissionPool.cs
├── Progression/
└── SaveSystem/
```

## Template Inventory

| Template ID | Category | Threat | Base Weight | Primary Reward |
|-------------|----------|--------|-------------|----------------|
| tmpl_supply_raid | Raid | 1.0+ | 1.5 (common) | Currency |
| tmpl_recon | Recon | 1.0+ | 1.2 | Intel |
| tmpl_rescue | Rescue | 1.0+ | 0.8 | Currency |
| tmpl_tech_recovery | TechRecovery | 1.2+ | 1.0 | TechComponents |
| tmpl_defense | Defense | 1.2+ | 1.0 | Currency |
| tmpl_sabotage | Sabotage | 1.4+ | 0.7 | Intel |
| tmpl_assassination | Assassination | 1.5+ | 0.5 (rare) | Intel |

## What The Generator Does That GenericMissionRotation Didn't

| Feature | GenericMissionRotation (Phase 4) | MissionGenerator (Phase 5) |
|---------|----------------------------------|----------------------------|
| Template selection | Random shuffle | Weighted by player needs |
| Resource awareness | None | Favors what player is low on |
| Roster awareness | None | Rescue relevance check |
| Threat gating | None | Templates unlock with worldThreatLevel |
| Enemy count | Fixed from template | Rolled from range + difficulty scaled |
| Rewards | Fixed from template | Rolled from range + difficulty scaled |
| Optional objectives | Fixed | Random subset from pool |

## Checklist

- [ ] Create Generation folder under Missions
- [ ] Create VariationRange, MissionTemplate, MissionTemplateLibrary
- [ ] Create MissionGenerator
- [ ] Update MissionManager to use MissionGenerator
- [ ] Delete GenericMissionRotation.cs
- [ ] Test: Generate rotation, verify 3 missions appear
- [ ] Test: Set low currency, verify raid missions are favored
- [ ] Test: Increase world threat, verify new templates unlock
- [ ] Test: Generate twice with same state, verify variation in enemy counts and rewards
