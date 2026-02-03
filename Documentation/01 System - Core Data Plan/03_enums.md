# Part 2: Creating the Enums

Enums define the valid values for various game concepts. Let's create them first since other classes will depend on them.

## Step 2.1: ActType Enum

Create a new C# script at `Assets/Scripts/Core/Data/Enums/ActType.cs`:

```csharp
namespace Game.Core.Data {

    public enum ActType {
        Act1,
        Act2,
        Act3
    }

}
```

**Note:** We've simplified the enum values. Descriptive names like `Act1_Firefighting` can be added later if needed, but simple names work well for now.

## Step 2.2: FactionType Enum

Create `Assets/Scripts/Core/Data/Enums/FactionType.cs`:

```csharp
namespace Game.Core.Data {
    /// <summary>
    /// The factions in the game.
    /// </summary>
    public enum FactionType {
        None,
        Stratocracy,
        Totalitarian,
        Technocracy
    }
}
```

**Design Note:** The three factions each represent different approaches to governance:
- **Stratocracy** - Military rule, values order and hierarchy
- **Totalitarian** - Ideological control, values unity through conformity
- **Technocracy** - Rule by expertise, values efficiency and progress

## Step 2.3: UnitStatus Enum

Create `Assets/Scripts/Core/Data/Enums/UnitStatus.cs`:

```csharp
namespace Game.Core.Data {

    public enum UnitStatus {
        Active,
        Injured,
        Captured,
        Missing,
        Dead
    }

}
```

**Note:** `Captured` and `Missing` are narrative states that can drive story events. Dead units are permanently lost.

## Step 2.4: MissionStatus Enum

Create `Assets/Scripts/Core/Data/Enums/MissionStatus.cs`:

```csharp
namespace Game.Core.Data {

    public enum MissionStatus {
        Locked,
        Available,
        Active,
        Completed,
        Failed
    }

}
```

## Step 2.5: ResourceType Enum

Create `Assets/Scripts/Core/Data/Enums/ResourceType.cs`:

```csharp
namespace Game.Core.Data {

    public enum ResourceType {
        Currency,
        Alloys,
        TechComponents,
        Intel
    }

}
```

**Design Note:** We've streamlined resources to four core types:
- **Currency** - General purchasing power
- **Alloys** - Construction and upgrade materials
- **TechComponents** - Research requirements
- **Intel** - Information for missions and unlocks

## Step 2.6: MaterialType Enum

Create `Assets/Scripts/Core/Data/Enums/MaterialType.cs`:

```csharp
namespace Game.Core.Data
{

    public enum MaterialType
    {
        Placeholder
    }

}
```

**Design Note:** MaterialType is a placeholder for future expansion. Special crafting materials will be added as the tech tree is designed.

## Step 2.7: FactionStatus Enum (Optional)

Create `Assets/Scripts/Core/Data/Enums/FactionStatus.cs`:

```csharp
namespace Game.Core.Data {

    public enum FactionStatus {
        Active,
        Weakened,
        Defeated
    }

}
```

**Checkpoint:** You should now have 7 enum files in `Assets/Scripts/Core/Data/Enums/`. Save all files and return to Unity to let it compile. Fix any errors before continuing.
