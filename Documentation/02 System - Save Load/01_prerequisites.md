# Prerequisites

Before building the Save/Load system, ensure your Phase 1 implementation is complete.

## Required Files from Phase 1

Verify these files exist and compile without errors:

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   │   └── GameConstants.cs
│   ├── Enums/
│   │   ├── ActType.cs
│   │   ├── FactionType.cs
│   │   ├── MaterialType.cs
│   │   ├── MissionStatus.cs
│   │   ├── ResourceType.cs
│   │   └── UnitStatus.cs
│   └── Structs/
│       ├── Choice.cs
│       ├── MissionRecord.cs
│       ├── TechData.cs
│       └── UnitData.cs
├── Events/
│   ├── EventBus.cs
│   ├── MaterialChangedEvent.cs
│   ├── MissionCompletedEvent.cs
│   ├── ResourceChangedEvent.cs
│   ├── StateChangedEvent.cs
│   └── UnitStatusChangedEvent.cs
└── GameState/
    ├── CampaignState.cs
    ├── GameState.cs
    ├── GameStateManager.cs
    ├── MaterialState.cs
    ├── ResourceState.cs
    ├── TechState.cs
    └── UnitRosterState.cs
```

## Serialization Requirements

All state classes must be marked `[Serializable]` for JSON serialization to work:

```csharp
using System;

namespace Game.Core.States {

    [Serializable]
    public class CampaignState {
        // ...
    }

}
```

**Check each file:**
- `GameState.cs` - `[Serializable]`
- `CampaignState.cs` - `[Serializable]`
- `UnitRosterState.cs` - `[Serializable]`
- `ResourceState.cs` - `[Serializable]`
- `MaterialState.cs` - `[Serializable]`
- `TechState.cs` - `[Serializable]`
- `UnitData.cs` - `[Serializable]` (struct)
- `TechData.cs` - `[Serializable]` (struct)

## Version Field

Your `GameState` class should have a version field for migration support:

```csharp
[Serializable]
public class GameState {

    public int version;  // <- This is critical for save compatibility

    // ... rest of state
}
```

## Scene Setup

Ensure you have a `GameStateManager` in your scene:
1. Create an empty GameObject named "GameStateManager"
2. Add the `GameStateManager` component
3. The singleton pattern handles `DontDestroyOnLoad`

## Checkpoint

Before proceeding, verify:
- [ ] All state classes compile without errors
- [ ] All state classes have `[Serializable]` attribute
- [ ] `GameState.version` field exists
- [ ] `GameStateManager` is in your scene and working
