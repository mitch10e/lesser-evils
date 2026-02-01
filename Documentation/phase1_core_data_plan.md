# Phase 1: Core Data & State Management - Development Tutorial

Welcome to Phase 1 of building your tactical RPG! This tutorial will guide you through creating the foundational data layer that all other game systems will depend on. By the end, you'll have a robust state management system that tracks everything from squad status to moral decisions.

---

## What You'll Build

In this phase, you'll create:
- A centralized `GameState` that holds all game data
- Individual state classes for Campaign, Squad, Resources, Factions, Morals, Tech, and Branching
- A manager singleton that provides easy access to all state
- Editor tools for debugging and testing
- Automated tests to verify everything works

---

## Prerequisites

Before starting, make sure you have:
- Unity 2021.3 LTS or newer installed
- Visual Studio Code with C# extension (or Visual Studio)
- A new Unity project created (or the existing LesserEvils project)

---

## Part 1: Setting Up the Folder Structure

Let's start by creating the folder structure for our scripts. Good organization now will save headaches later.

### Step 1.1: Create the Core Folders

1. In Unity's Project window, right-click on `Assets`
2. Create the following folder hierarchy:

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameState/
│   │   ├── States/
│   │   ├── Data/
│   │   │   ├── Enums/
│   │   │   ├── Structs/
│   │   │   └── Constants/
│   │   └── Events/
│   └── Editor/
│       └── GameState/
├── Data/
│   └── ScriptableObjects/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

**Tip:** You can create folders by right-clicking and selecting `Create > Folder`, or use `Ctrl+Shift+N` on Windows.

### Step 1.2: Set Up the Test Assembly

Unity requires special assembly definitions for tests.

1. Navigate to `Assets/Tests/EditMode`
2. Right-click and select `Create > Assembly Definition`
3. Name it `EditModeTests`
4. In the Inspector, check `Test Assemblies` and select `Editor` platform only
5. Under `Assembly Definition References`, add `UnityEngine.TestRunner` and `UnityEditor.TestRunner`

Repeat for `PlayMode`:
1. Navigate to `Assets/Tests/PlayMode`
2. Create an Assembly Definition named `PlayModeTests`
3. Check `Test Assemblies` and include all platforms

---

## Part 2: Creating the Enums

Enums define the valid values for various game concepts. Let's create them first since other classes will depend on them.

### Step 2.1: ActType Enum

Create a new C# script at `Assets/Scripts/Core/Data/Enums/ActType.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Represents the three acts of the campaign.
    /// Each act has distinct gameplay characteristics and narrative focus.
    /// </summary>
    public enum ActType
    {
        /// <summary>
        /// Act 1: Defensive gameplay, small squad (2-3 units), survival focus.
        /// Player unknowingly contributes to extinction tech development.
        /// </summary>
        Act1_Firefighting,

        /// <summary>
        /// Act 2: Offensive gameplay, larger squad (4-5 units), faction targeting.
        /// Player gains access to precursor extinction tech at climax.
        /// Defection decision point at end of act.
        /// </summary>
        Act2_PowerScaling,

        /// <summary>
        /// Act 3: Endgame, full squad (5-6 units).
        /// Loyalist path: Power fantasy with moral consequences.
        /// Defector path: High difficulty heroic struggle.
        /// </summary>
        Act3_Climax
    }
}
```

**What you learned:** We use XML documentation comments (`///`) to explain what each value means. This helps when hovering over values in VSCode.

### Step 2.2: FactionType Enum

Create `Assets/Scripts/Core/Data/Enums/FactionType.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// The three factions vying for control in the civil war.
    /// Each faction has a fatal flaw leading to an extinction-level outcome.
    /// </summary>
    public enum FactionType
    {
        /// <summary>No faction assigned.</summary>
        None,

        /// <summary>
        /// Militarized, centralized control. Values order and security.
        /// Fatal flaw: Sacrifices freedom for efficiency.
        /// Extinction path: Rogue AI controlling population via drones.
        /// Tech focus: Heavy armor, orbital strikes, AI targeting.
        /// </summary>
        Authoritarian,

        /// <summary>
        /// Religious/ideological unity, zealous devotion. Values faith and purity.
        /// Fatal flaw: Dogmatic, intolerant, willing to commit atrocities in the name of faith.
        /// Extinction path: Psionic mind control, mass indoctrination, loss of free will.
        /// Tech focus: Psionics, energy weapons, zeal-fueled abilities, morale buffs.
        /// </summary>
        Covenant,

        /// <summary>
        /// Rule by logic and expertise. Values efficiency and research.
        /// Fatal flaw: Cold, detached, willing to sacrifice lives for metrics.
        /// Extinction path: Planetary destruction via extreme experimentation.
        /// Tech focus: Energy weapons, hacking, psionics, orbital operations.
        /// </summary>
        Technocratic
    }
}
```

### Step 2.3: UnitStatus Enum

Create `Assets/Scripts/Core/Data/Enums/UnitStatus.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Possible states for a squad unit.
    /// </summary>
    public enum UnitStatus
    {
        /// <summary>Healthy and available for deployment.</summary>
        Active,

        /// <summary>Recovering from wounds. Cannot deploy until healed.</summary>
        Injured,

        /// <summary>Permanently lost. Cannot be recovered.</summary>
        Dead,

        /// <summary>Unknown status. Used for story purposes.</summary>
        Missing,

        /// <summary>Left the squad during Act 2/3 branching.</summary>
        Defected
    }
}
```

### Step 2.4: MissionStatus Enum

Create `Assets/Scripts/Core/Data/Enums/MissionStatus.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Tracks the state of a mission in the campaign.
    /// </summary>
    public enum MissionStatus
    {
        /// <summary>Prerequisites not met. Cannot be selected.</summary>
        Locked,

        /// <summary>Available for selection.</summary>
        Available,

        /// <summary>Currently being played.</summary>
        Active,

        /// <summary>Successfully completed all primary objectives.</summary>
        Completed,

        /// <summary>Failed to complete primary objectives.</summary>
        Failed
    }
}
```

### Step 2.5: FactionRelationStatus Enum

Create `Assets/Scripts/Core/Data/Enums/FactionRelationStatus.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Describes the relationship between the player and a faction.
    /// </summary>
    public enum FactionRelationStatus
    {
        /// <summary>Normal operations. No special relationship.</summary>
        Active,

        /// <summary>Player is actively fighting this faction.</summary>
        Targeted,

        /// <summary>Faction was crippled in Act 2. Extinction tech destroyed.</summary>
        Defeated,

        /// <summary>Faction is building extinction tech in background while player focuses elsewhere.</summary>
        Escalating,

        /// <summary>Former ally turned enemy. Only applies after defection.</summary>
        Hostile
    }
}
```

### Step 2.6: ResourceType Enum

Create `Assets/Scripts/Core/Data/Enums/ResourceType.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Types of resources the player must manage.
    /// Scarcity creates tension and forces difficult choices.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>General currency for unit upgrades and loadout purchases.</summary>
        Currency,

        /// <summary>Components required for tech research and unlocks.</summary>
        TechComponents,

        /// <summary>Supplies for healing injured units. Reduces recovery time.</summary>
        MedicalSupplies,

        /// <summary>Fuel for mission deployment. Larger missions cost more.</summary>
        Fuel,

        /// <summary>Intelligence points. Unlocks optional objectives and recon info.</summary>
        Intel
    }
}
```

**Checkpoint:** You should now have 6 enum files in `Assets/Scripts/Core/Data/Enums/`. Save all files and return to Unity to let it compile. Fix any errors before continuing.

---

## Part 3: Creating Data Structs

Structs are lightweight data containers. We'll use them for individual records like unit data and moral choices.

### Step 3.1: UnitData Struct

Create `Assets/Scripts/Core/Data/Structs/UnitData.cs`:

```csharp
using System;

namespace Game.Core.Data
{
    /// <summary>
    /// Represents a single squad member with all their stats and state.
    /// This is a struct for performance - units are value types copied on assignment.
    /// </summary>
    [Serializable]
    public struct UnitData
    {
        /// <summary>Unique identifier for this unit.</summary>
        public string Id;

        /// <summary>Display name shown in UI.</summary>
        public string DisplayName;

        /// <summary>Current level (1-10 typical range).</summary>
        public int Level;

        /// <summary>Current experience points.</summary>
        public int Experience;

        /// <summary>XP needed to reach next level.</summary>
        public int ExperienceToNextLevel;

        /// <summary>Current availability status.</summary>
        public UnitStatus Status;

        /// <summary>
        /// Loyalty to the player's faction (-100 to 100).
        /// Low loyalty units may refuse orders or defect.
        /// </summary>
        public int LoyaltyScore;

        /// <summary>ID of currently equipped loadout (weapons, armor, items).</summary>
        public string EquippedLoadoutId;

        /// <summary>Turns remaining until injury heals. 0 if not injured.</summary>
        public int InjuryRecoveryTurns;

        /// <summary>
        /// True for story-important characters (bosses, key NPCs).
        /// Named characters have special dialogue and cannot be randomly generated.
        /// </summary>
        public bool IsNamedCharacter;

        /// <summary>
        /// Creates a new unit with default values.
        /// </summary>
        public static UnitData CreateDefault(string id, string name)
        {
            return new UnitData
            {
                Id = id,
                DisplayName = name,
                Level = 1,
                Experience = 0,
                ExperienceToNextLevel = 100,
                Status = UnitStatus.Active,
                LoyaltyScore = 50, // Neutral starting loyalty
                EquippedLoadoutId = "",
                InjuryRecoveryTurns = 0,
                IsNamedCharacter = false
            };
        }
    }
}
```

**What you learned:** The `[Serializable]` attribute tells Unity this struct can be saved/loaded. The static factory method `CreateDefault` gives us a convenient way to create units with sensible starting values.

### Step 3.2: MoralChoice Struct

Create `Assets/Scripts/Core/Data/Structs/MoralChoice.cs`:

```csharp
using System;

namespace Game.Core.Data
{
    /// <summary>
    /// Records a single moral decision made during gameplay.
    /// Used to track player's ethical journey and influence endings.
    /// </summary>
    [Serializable]
    public struct MoralChoice
    {
        /// <summary>ID of the mission where this choice occurred.</summary>
        public string MissionId;

        /// <summary>Unique identifier for this specific choice.</summary>
        public string ChoiceId;

        /// <summary>Human-readable description of the choice made.</summary>
        public string Description;

        /// <summary>True if player chose the ethical option, false if efficient.</summary>
        public bool WasEthical;

        /// <summary>How much this choice shifted the moral meter (positive = ethical).</summary>
        public int MoralImpact;

        /// <summary>Game turn when this choice was made. For timeline tracking.</summary>
        public int TurnNumber;

        /// <summary>
        /// Creates a moral choice record.
        /// </summary>
        public static MoralChoice Create(
            string missionId,
            string choiceId,
            string description,
            bool wasEthical,
            int impact,
            int turn)
        {
            return new MoralChoice
            {
                MissionId = missionId,
                ChoiceId = choiceId,
                Description = description,
                WasEthical = wasEthical,
                MoralImpact = impact,
                TurnNumber = turn
            };
        }
    }
}
```

### Step 3.3: MissionRecord Struct

Create `Assets/Scripts/Core/Data/Structs/MissionRecord.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Data
{
    /// <summary>
    /// Records the outcome of a completed mission.
    /// Used for history tracking and deferred consequences.
    /// </summary>
    [Serializable]
    public struct MissionRecord
    {
        /// <summary>ID of the completed mission.</summary>
        public string MissionId;

        /// <summary>Whether primary objectives were completed.</summary>
        public bool WasSuccessful;

        /// <summary>IDs of optional objectives that were completed.</summary>
        public List<string> CompletedOptionalObjectives;

        /// <summary>IDs of units that were injured during the mission.</summary>
        public List<string> InjuredUnitIds;

        /// <summary>IDs of units that died during the mission.</summary>
        public List<string> DeadUnitIds;

        /// <summary>Resources gained from the mission.</summary>
        public Dictionary<ResourceType, int> ResourcesGained;

        /// <summary>Resources spent during the mission.</summary>
        public Dictionary<ResourceType, int> ResourcesSpent;

        /// <summary>Moral choices made during this mission.</summary>
        public List<MoralChoice> MoralChoicesMade;

        /// <summary>Game turn when mission was completed.</summary>
        public int CompletedOnTurn;

        /// <summary>
        /// Creates an empty mission record ready to be filled in.
        /// </summary>
        public static MissionRecord Create(string missionId)
        {
            return new MissionRecord
            {
                MissionId = missionId,
                WasSuccessful = false,
                CompletedOptionalObjectives = new List<string>(),
                InjuredUnitIds = new List<string>(),
                DeadUnitIds = new List<string>(),
                ResourcesGained = new Dictionary<ResourceType, int>(),
                ResourcesSpent = new Dictionary<ResourceType, int>(),
                MoralChoicesMade = new List<MoralChoice>(),
                CompletedOnTurn = 0
            };
        }
    }
}
```

### Step 3.4: Game Constants

Create `Assets/Scripts/Core/Data/Constants/GameConstants.cs`:

```csharp
namespace Game.Core.Data
{
    /// <summary>
    /// Central location for game balance values and magic numbers.
    /// Adjust these to tune game difficulty and pacing.
    /// </summary>
    public static class GameConstants
    {
        // Moral System
        public const int MORAL_METER_MIN = -100;
        public const int MORAL_METER_MAX = 100;
        public const int MORAL_METER_START = 0;

        // Loyalty System
        public const int LOYALTY_MIN = -100;
        public const int LOYALTY_MAX = 100;
        public const int LOYALTY_START = 50;
        public const int LOYALTY_DEFECTION_THRESHOLD = -25; // Units below this may defect

        // Squad Sizes per Act
        public const int SQUAD_SIZE_ACT1 = 3;
        public const int SQUAD_SIZE_ACT2 = 5;
        public const int SQUAD_SIZE_ACT3 = 6;

        // Experience and Leveling
        public const int BASE_XP_TO_LEVEL = 100;
        public const float XP_SCALING_PER_LEVEL = 1.5f;
        public const int MAX_UNIT_LEVEL = 10;

        // Injury Recovery
        public const int BASE_INJURY_RECOVERY_TURNS = 3;

        // Starting Resources
        public const int STARTING_CURRENCY = 500;
        public const int STARTING_TECH_COMPONENTS = 0;
        public const int STARTING_MEDICAL_SUPPLIES = 10;
        public const int STARTING_FUEL = 50;
        public const int STARTING_INTEL = 0;
    }
}
```

**Checkpoint:** You should now have 4 files in the Structs and Constants folders. Compile in Unity and fix any errors.

---

## Part 4: Creating the State Classes

Now we'll create the individual state classes that track different aspects of the game.

### Step 4.1: ResourceState

Let's start with a simpler one. Create `Assets/Scripts/Core/States/ResourceState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks all currencies and materials.
    /// Resources create the risk/reward tension that drives mission selection.
    /// </summary>
    [Serializable]
    public class ResourceState
    {
        /// <summary>Current amount of each resource type.</summary>
        public Dictionary<ResourceType, int> Resources;

        /// <summary>
        /// Creates a new ResourceState with starting values.
        /// </summary>
        public ResourceState()
        {
            Resources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Currency, GameConstants.STARTING_CURRENCY },
                { ResourceType.TechComponents, GameConstants.STARTING_TECH_COMPONENTS },
                { ResourceType.MedicalSupplies, GameConstants.STARTING_MEDICAL_SUPPLIES },
                { ResourceType.Fuel, GameConstants.STARTING_FUEL },
                { ResourceType.Intel, GameConstants.STARTING_INTEL }
            };
        }

        /// <summary>
        /// Gets the current amount of a resource.
        /// </summary>
        public int GetResource(ResourceType type)
        {
            return Resources.TryGetValue(type, out int amount) ? amount : 0;
        }

        /// <summary>
        /// Checks if player has at least the specified amount.
        /// </summary>
        public bool HasResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }

        /// <summary>
        /// Adds resources. Amount can be negative to subtract.
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            if (!Resources.ContainsKey(type))
            {
                Resources[type] = 0;
            }

            Resources[type] += amount;

            // Prevent negative resources
            if (Resources[type] < 0)
            {
                Resources[type] = 0;
            }
        }

        /// <summary>
        /// Attempts to spend resources. Returns false if insufficient.
        /// This is an atomic operation - either all is spent or nothing.
        /// </summary>
        public bool SpendResource(ResourceType type, int amount)
        {
            if (!HasResource(type, amount))
            {
                return false;
            }

            Resources[type] -= amount;
            return true;
        }

        /// <summary>
        /// Sets a resource to an exact amount. Use sparingly.
        /// </summary>
        public void SetResource(ResourceType type, int amount)
        {
            Resources[type] = Math.Max(0, amount);
        }

        /// <summary>
        /// Resets all resources to starting values.
        /// </summary>
        public void Reset()
        {
            Resources[ResourceType.Currency] = GameConstants.STARTING_CURRENCY;
            Resources[ResourceType.TechComponents] = GameConstants.STARTING_TECH_COMPONENTS;
            Resources[ResourceType.MedicalSupplies] = GameConstants.STARTING_MEDICAL_SUPPLIES;
            Resources[ResourceType.Fuel] = GameConstants.STARTING_FUEL;
            Resources[ResourceType.Intel] = GameConstants.STARTING_INTEL;
        }
    }
}
```

**What you learned:** Notice how `SpendResource` returns a boolean to indicate success/failure. This prevents bugs where you might accidentally spend resources the player doesn't have.

### Step 4.2: MoralState

Create `Assets/Scripts/Core/States/MoralState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks the player's moral journey through the campaign.
    /// The moral meter influences squad loyalty, AI escalation, and endings.
    /// </summary>
    [Serializable]
    public class MoralState
    {
        /// <summary>
        /// Current moral standing (-100 = ruthless, 100 = ethical).
        /// </summary>
        public int MoralMeter;

        /// <summary>
        /// Complete history of moral choices for consequence tracking.
        /// </summary>
        public List<MoralChoice> ChoiceHistory;

        /// <summary>
        /// Count of choices where player chose the ethical path.
        /// </summary>
        public int TotalEthicalChoices;

        /// <summary>
        /// Count of choices where player chose efficiency over ethics.
        /// </summary>
        public int TotalEfficientChoices;

        public MoralState()
        {
            MoralMeter = GameConstants.MORAL_METER_START;
            ChoiceHistory = new List<MoralChoice>();
            TotalEthicalChoices = 0;
            TotalEfficientChoices = 0;
        }

        /// <summary>
        /// Records a moral choice and updates the meter.
        /// </summary>
        public void RecordChoice(MoralChoice choice)
        {
            ChoiceHistory.Add(choice);
            ModifyMoral(choice.MoralImpact);

            if (choice.WasEthical)
            {
                TotalEthicalChoices++;
            }
            else
            {
                TotalEfficientChoices++;
            }
        }

        /// <summary>
        /// Adjusts the moral meter by the given delta.
        /// Positive values push toward ethical, negative toward ruthless.
        /// </summary>
        public void ModifyMoral(int delta)
        {
            MoralMeter += delta;
            MoralMeter = Math.Clamp(MoralMeter, GameConstants.MORAL_METER_MIN, GameConstants.MORAL_METER_MAX);
        }

        /// <summary>
        /// Returns true if player is on the ethical side of the meter.
        /// </summary>
        public bool IsEthicalPath()
        {
            return MoralMeter > 0;
        }

        /// <summary>
        /// Returns true if player is on the ruthless side of the meter.
        /// </summary>
        public bool IsRuthlessPath()
        {
            return MoralMeter < 0;
        }

        /// <summary>
        /// Returns the moral ratio from -1 (fully ruthless) to 1 (fully ethical).
        /// Useful for scaling consequences or UI displays.
        /// </summary>
        public float GetMoralRatio()
        {
            return MoralMeter / 100f;
        }

        /// <summary>
        /// Gets moral choices from a specific mission.
        /// </summary>
        public List<MoralChoice> GetChoicesFromMission(string missionId)
        {
            return ChoiceHistory.FindAll(c => c.MissionId == missionId);
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            MoralMeter = GameConstants.MORAL_METER_START;
            ChoiceHistory.Clear();
            TotalEthicalChoices = 0;
            TotalEfficientChoices = 0;
        }
    }
}
```

### Step 4.3: FactionState

Create `Assets/Scripts/Core/States/FactionState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks relationships with all factions.
    /// Drives mission availability, branching, and narrative tone.
    /// </summary>
    [Serializable]
    public class FactionState
    {
        /// <summary>The faction the player chose at game start.</summary>
        public FactionType PlayerFaction;

        /// <summary>Loyalty values for each faction (-100 to 100).</summary>
        public Dictionary<FactionType, int> LoyaltyValues;

        /// <summary>Current relationship status with each faction.</summary>
        public Dictionary<FactionType, FactionRelationStatus> FactionStatuses;

        /// <summary>The faction currently being actively fought.</summary>
        public FactionType TargetedFaction;

        /// <summary>True if player has defected from their original faction.</summary>
        public bool PlayerIsDefector;

        public FactionState()
        {
            PlayerFaction = FactionType.None;
            PlayerIsDefector = false;
            TargetedFaction = FactionType.None;

            LoyaltyValues = new Dictionary<FactionType, int>
            {
                { FactionType.Authoritarian, 0 },
                { FactionType.Covenant, 0 },
                { FactionType.Technocratic, 0 }
            };

            FactionStatuses = new Dictionary<FactionType, FactionRelationStatus>
            {
                { FactionType.Authoritarian, FactionRelationStatus.Active },
                { FactionType.Covenant, FactionRelationStatus.Active },
                { FactionType.Technocratic, FactionRelationStatus.Active }
            };
        }

        /// <summary>
        /// Sets the player's starting faction and initializes relationships.
        /// Call this at game start after player makes their choice.
        /// </summary>
        public void SetPlayerFaction(FactionType faction)
        {
            PlayerFaction = faction;

            // Start with high loyalty to chosen faction
            LoyaltyValues[faction] = 75;

            // Other factions start neutral
            foreach (var f in LoyaltyValues.Keys)
            {
                if (f != faction)
                {
                    LoyaltyValues[f] = 0;
                }
            }
        }

        /// <summary>
        /// Gets loyalty value for a faction.
        /// </summary>
        public int GetLoyalty(FactionType faction)
        {
            return LoyaltyValues.TryGetValue(faction, out int value) ? value : 0;
        }

        /// <summary>
        /// Modifies loyalty with a faction. Clamps to valid range.
        /// </summary>
        public void ModifyLoyalty(FactionType faction, int delta)
        {
            if (!LoyaltyValues.ContainsKey(faction)) return;

            LoyaltyValues[faction] += delta;
            LoyaltyValues[faction] = Math.Clamp(
                LoyaltyValues[faction],
                GameConstants.LOYALTY_MIN,
                GameConstants.LOYALTY_MAX
            );
        }

        /// <summary>
        /// Gets the relationship status with a faction.
        /// </summary>
        public FactionRelationStatus GetStatus(FactionType faction)
        {
            return FactionStatuses.TryGetValue(faction, out var status)
                ? status
                : FactionRelationStatus.Active;
        }

        /// <summary>
        /// Sets the relationship status with a faction.
        /// </summary>
        public void SetStatus(FactionType faction, FactionRelationStatus status)
        {
            FactionStatuses[faction] = status;
        }

        /// <summary>
        /// Marks a faction as the current target of player operations.
        /// </summary>
        public void SetTargetFaction(FactionType faction)
        {
            // Clear previous target
            if (TargetedFaction != FactionType.None)
            {
                FactionStatuses[TargetedFaction] = FactionRelationStatus.Active;
            }

            TargetedFaction = faction;

            if (faction != FactionType.None)
            {
                FactionStatuses[faction] = FactionRelationStatus.Targeted;
            }
        }

        /// <summary>
        /// Triggers player defection. Called at Act 2 decision point.
        /// This makes the player's original faction hostile.
        /// </summary>
        public void TriggerDefection()
        {
            PlayerIsDefector = true;

            // Former faction becomes hostile
            FactionStatuses[PlayerFaction] = FactionRelationStatus.Hostile;
            LoyaltyValues[PlayerFaction] = GameConstants.LOYALTY_MIN;
        }

        /// <summary>
        /// Checks if a faction has been defeated.
        /// </summary>
        public bool IsFactionDefeated(FactionType faction)
        {
            return GetStatus(faction) == FactionRelationStatus.Defeated;
        }

        /// <summary>
        /// Gets the non-targeted enemy faction (the one escalating in background).
        /// </summary>
        public FactionType GetEscalatingFaction()
        {
            foreach (var kvp in FactionStatuses)
            {
                if (kvp.Key != PlayerFaction &&
                    kvp.Key != TargetedFaction &&
                    kvp.Value != FactionRelationStatus.Defeated)
                {
                    return kvp.Key;
                }
            }
            return FactionType.None;
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            PlayerFaction = FactionType.None;
            PlayerIsDefector = false;
            TargetedFaction = FactionType.None;

            foreach (var faction in new[] { FactionType.Authoritarian, FactionType.Covenant, FactionType.Technocratic })
            {
                LoyaltyValues[faction] = 0;
                FactionStatuses[faction] = FactionRelationStatus.Active;
            }
        }
    }
}
```

### Step 4.4: TechState

Create `Assets/Scripts/Core/States/TechState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks technology unlocks and research progress.
    /// Tied to act progression and faction support.
    /// </summary>
    [Serializable]
    public class TechState
    {
        /// <summary>IDs of all unlocked technologies.</summary>
        public List<string> UnlockedTechIds;

        /// <summary>Research progress for technologies being developed.</summary>
        public Dictionary<string, int> ResearchProgress;

        /// <summary>
        /// True when player gains access to precursor extinction tech at Act 2 climax.
        /// </summary>
        public bool HasPrecursorAccess;

        /// <summary>
        /// True when player has full extinction tech (Act 3 loyalist path).
        /// </summary>
        public bool HasFullExtinctionTech;

        public TechState()
        {
            UnlockedTechIds = new List<string>();
            ResearchProgress = new Dictionary<string, int>();
            HasPrecursorAccess = false;
            HasFullExtinctionTech = false;
        }

        /// <summary>
        /// Checks if a technology has been unlocked.
        /// </summary>
        public bool IsTechUnlocked(string techId)
        {
            return UnlockedTechIds.Contains(techId);
        }

        /// <summary>
        /// Unlocks a technology. No effect if already unlocked.
        /// </summary>
        public void UnlockTech(string techId)
        {
            if (!UnlockedTechIds.Contains(techId))
            {
                UnlockedTechIds.Add(techId);
            }
        }

        /// <summary>
        /// Gets current research progress for a technology.
        /// </summary>
        public int GetResearchProgress(string techId)
        {
            return ResearchProgress.TryGetValue(techId, out int progress) ? progress : 0;
        }

        /// <summary>
        /// Adds research progress. Returns true if tech should unlock.
        /// </summary>
        public void AddResearchProgress(string techId, int points)
        {
            if (!ResearchProgress.ContainsKey(techId))
            {
                ResearchProgress[techId] = 0;
            }

            ResearchProgress[techId] += points;
        }

        /// <summary>
        /// Grants access to precursor extinction tech.
        /// Called at Act 2 climax mission.
        /// </summary>
        public void GrantPrecursorAccess()
        {
            HasPrecursorAccess = true;
        }

        /// <summary>
        /// Revokes precursor access. Called when player defects.
        /// </summary>
        public void RevokePrecursorAccess()
        {
            HasPrecursorAccess = false;
        }

        /// <summary>
        /// Grants full extinction tech. Loyalist Act 3 only.
        /// </summary>
        public void GrantFullExtinctionTech()
        {
            HasFullExtinctionTech = true;
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            UnlockedTechIds.Clear();
            ResearchProgress.Clear();
            HasPrecursorAccess = false;
            HasFullExtinctionTech = false;
        }
    }
}
```

### Step 4.5: BranchState

Create `Assets/Scripts/Core/States/BranchState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks major story branching flags and decisions.
    /// Controls which narrative paths are active.
    /// </summary>
    [Serializable]
    public class BranchState
    {
        /// <summary>True = loyalist path (power fantasy), False = defector path (true ending).</summary>
        public bool IsLoyalistPath;

        /// <summary>True after the Act 2 decision point has been reached.</summary>
        public bool HasMadeAct2Decision;

        /// <summary>Which faction was defeated/crippled in Act 2.</summary>
        public FactionType DefeatedFaction;

        /// <summary>Generic story flags for narrative tracking.</summary>
        public List<string> ActiveStoryFlags;

        /// <summary>IDs of units that defected (either with or against the player).</summary>
        public List<string> DefectorUnitIds;

        public BranchState()
        {
            IsLoyalistPath = true; // Default to loyalist until decision
            HasMadeAct2Decision = false;
            DefeatedFaction = FactionType.None;
            ActiveStoryFlags = new List<string>();
            DefectorUnitIds = new List<string>();
        }

        /// <summary>
        /// Sets the player's path at the Act 2 decision point.
        /// </summary>
        public void SetPath(bool isLoyalist)
        {
            IsLoyalistPath = isLoyalist;
            HasMadeAct2Decision = true;
        }

        /// <summary>
        /// Adds a story flag. Used for conditional content and consequences.
        /// </summary>
        public void AddStoryFlag(string flag)
        {
            if (!ActiveStoryFlags.Contains(flag))
            {
                ActiveStoryFlags.Add(flag);
            }
        }

        /// <summary>
        /// Checks if a story flag is active.
        /// </summary>
        public bool HasStoryFlag(string flag)
        {
            return ActiveStoryFlags.Contains(flag);
        }

        /// <summary>
        /// Removes a story flag.
        /// </summary>
        public void RemoveStoryFlag(string flag)
        {
            ActiveStoryFlags.Remove(flag);
        }

        /// <summary>
        /// Marks a unit as having defected.
        /// </summary>
        public void MarkUnitAsDefector(string unitId)
        {
            if (!DefectorUnitIds.Contains(unitId))
            {
                DefectorUnitIds.Add(unitId);
            }
        }

        /// <summary>
        /// Checks if a unit has defected.
        /// </summary>
        public bool IsUnitDefector(string unitId)
        {
            return DefectorUnitIds.Contains(unitId);
        }

        /// <summary>
        /// Records which faction was defeated in Act 2.
        /// </summary>
        public void SetDefeatedFaction(FactionType faction)
        {
            DefeatedFaction = faction;
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            IsLoyalistPath = true;
            HasMadeAct2Decision = false;
            DefeatedFaction = FactionType.None;
            ActiveStoryFlags.Clear();
            DefectorUnitIds.Clear();
        }
    }
}
```

### Step 4.6: SquadState

Create `Assets/Scripts/Core/States/SquadState.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Manages the player's squad roster, unit progression, and deployment.
    /// </summary>
    [Serializable]
    public class SquadState
    {
        /// <summary>All units in the player's roster.</summary>
        public List<UnitData> Roster;

        /// <summary>Maximum deployable squad size. Scales with act.</summary>
        public int MaxSquadSize;

        /// <summary>IDs of units currently on a mission.</summary>
        public List<string> DeployedUnitIds;

        public SquadState()
        {
            Roster = new List<UnitData>();
            MaxSquadSize = GameConstants.SQUAD_SIZE_ACT1;
            DeployedUnitIds = new List<string>();
        }

        /// <summary>
        /// Gets a unit by ID. Returns null if not found.
        /// </summary>
        public UnitData? GetUnit(string unitId)
        {
            int index = Roster.FindIndex(u => u.Id == unitId);
            if (index >= 0)
            {
                return Roster[index];
            }
            return null;
        }

        /// <summary>
        /// Gets all units that are available for deployment.
        /// </summary>
        public List<UnitData> GetActiveUnits()
        {
            return Roster.Where(u => u.Status == UnitStatus.Active).ToList();
        }

        /// <summary>
        /// Gets all units currently recovering from injuries.
        /// </summary>
        public List<UnitData> GetInjuredUnits()
        {
            return Roster.Where(u => u.Status == UnitStatus.Injured).ToList();
        }

        /// <summary>
        /// Gets all units that have died.
        /// </summary>
        public List<UnitData> GetDeadUnits()
        {
            return Roster.Where(u => u.Status == UnitStatus.Dead).ToList();
        }

        /// <summary>
        /// Adds a new unit to the roster.
        /// </summary>
        public void AddUnit(UnitData unit)
        {
            // Check for duplicate ID
            if (Roster.Any(u => u.Id == unit.Id))
            {
                return;
            }

            Roster.Add(unit);
        }

        /// <summary>
        /// Removes a unit from the roster entirely.
        /// </summary>
        public void RemoveUnit(string unitId)
        {
            Roster.RemoveAll(u => u.Id == unitId);
            DeployedUnitIds.Remove(unitId);
        }

        /// <summary>
        /// Updates a unit's status.
        /// </summary>
        public void UpdateUnitStatus(string unitId, UnitStatus status)
        {
            int index = Roster.FindIndex(u => u.Id == unitId);
            if (index >= 0)
            {
                UnitData unit = Roster[index];
                unit.Status = status;

                // If injured, set recovery time
                if (status == UnitStatus.Injured)
                {
                    unit.InjuryRecoveryTurns = GameConstants.BASE_INJURY_RECOVERY_TURNS;
                }
                else if (status == UnitStatus.Active)
                {
                    unit.InjuryRecoveryTurns = 0;
                }

                Roster[index] = unit;
            }
        }

        /// <summary>
        /// Adds experience to a unit. Handles level ups automatically.
        /// </summary>
        public void AddExperience(string unitId, int xp)
        {
            int index = Roster.FindIndex(u => u.Id == unitId);
            if (index < 0) return;

            UnitData unit = Roster[index];
            unit.Experience += xp;

            // Check for level up
            while (unit.Experience >= unit.ExperienceToNextLevel &&
                   unit.Level < GameConstants.MAX_UNIT_LEVEL)
            {
                unit.Experience -= unit.ExperienceToNextLevel;
                unit.Level++;
                unit.ExperienceToNextLevel = CalculateXPForLevel(unit.Level + 1);
            }

            Roster[index] = unit;
        }

        /// <summary>
        /// Modifies a unit's loyalty score.
        /// </summary>
        public void ModifyLoyalty(string unitId, int delta)
        {
            int index = Roster.FindIndex(u => u.Id == unitId);
            if (index < 0) return;

            UnitData unit = Roster[index];
            unit.LoyaltyScore += delta;
            unit.LoyaltyScore = Math.Clamp(
                unit.LoyaltyScore,
                GameConstants.LOYALTY_MIN,
                GameConstants.LOYALTY_MAX
            );
            Roster[index] = unit;
        }

        /// <summary>
        /// Checks if a unit can be deployed.
        /// </summary>
        public bool CanDeploy(string unitId)
        {
            UnitData? unit = GetUnit(unitId);
            if (!unit.HasValue) return false;

            return unit.Value.Status == UnitStatus.Active &&
                   !DeployedUnitIds.Contains(unitId);
        }

        /// <summary>
        /// Deploys a unit for a mission.
        /// </summary>
        public bool DeployUnit(string unitId)
        {
            if (!CanDeploy(unitId)) return false;
            if (DeployedUnitIds.Count >= MaxSquadSize) return false;

            DeployedUnitIds.Add(unitId);
            return true;
        }

        /// <summary>
        /// Returns a unit from deployment.
        /// </summary>
        public void ReturnFromDeployment(string unitId)
        {
            DeployedUnitIds.Remove(unitId);
        }

        /// <summary>
        /// Clears all deployments.
        /// </summary>
        public void ClearDeployments()
        {
            DeployedUnitIds.Clear();
        }

        /// <summary>
        /// Processes injury recovery at end of turn.
        /// </summary>
        public void ProcessInjuryRecovery()
        {
            for (int i = 0; i < Roster.Count; i++)
            {
                UnitData unit = Roster[i];
                if (unit.Status == UnitStatus.Injured && unit.InjuryRecoveryTurns > 0)
                {
                    unit.InjuryRecoveryTurns--;

                    if (unit.InjuryRecoveryTurns <= 0)
                    {
                        unit.Status = UnitStatus.Active;
                    }

                    Roster[i] = unit;
                }
            }
        }

        /// <summary>
        /// Updates max squad size based on current act.
        /// </summary>
        public void UpdateSquadSizeForAct(ActType act)
        {
            MaxSquadSize = act switch
            {
                ActType.Act1_Firefighting => GameConstants.SQUAD_SIZE_ACT1,
                ActType.Act2_PowerScaling => GameConstants.SQUAD_SIZE_ACT2,
                ActType.Act3_Climax => GameConstants.SQUAD_SIZE_ACT3,
                _ => GameConstants.SQUAD_SIZE_ACT1
            };
        }

        /// <summary>
        /// Gets units with low loyalty (potential defectors).
        /// </summary>
        public List<UnitData> GetPotentialDefectors()
        {
            return Roster
                .Where(u => u.LoyaltyScore <= GameConstants.LOYALTY_DEFECTION_THRESHOLD &&
                           u.Status == UnitStatus.Active)
                .ToList();
        }

        /// <summary>
        /// Calculates XP needed for a specific level.
        /// </summary>
        private int CalculateXPForLevel(int level)
        {
            return (int)(GameConstants.BASE_XP_TO_LEVEL *
                        Math.Pow(GameConstants.XP_SCALING_PER_LEVEL, level - 1));
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            Roster.Clear();
            MaxSquadSize = GameConstants.SQUAD_SIZE_ACT1;
            DeployedUnitIds.Clear();
        }
    }
}
```

### Step 4.7: CampaignState

Create `Assets/Scripts/Core/States/CampaignState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States
{
    /// <summary>
    /// Tracks campaign progression, act state, and mission history.
    /// </summary>
    [Serializable]
    public class CampaignState
    {
        /// <summary>Current act of the campaign.</summary>
        public ActType CurrentAct;

        /// <summary>Index of current mission within the act.</summary>
        public int CurrentMissionIndex;

        /// <summary>Current game turn (increments each time player takes action).</summary>
        public int CurrentTurn;

        /// <summary>IDs of completed missions.</summary>
        public List<string> CompletedMissionIds;

        /// <summary>IDs of failed missions.</summary>
        public List<string> FailedMissionIds;

        /// <summary>IDs of missions currently available for selection.</summary>
        public List<string> AvailableMissionIds;

        /// <summary>Detailed history of all completed missions.</summary>
        public List<MissionRecord> MissionHistory;

        public CampaignState()
        {
            CurrentAct = ActType.Act1_Firefighting;
            CurrentMissionIndex = 0;
            CurrentTurn = 1;
            CompletedMissionIds = new List<string>();
            FailedMissionIds = new List<string>();
            AvailableMissionIds = new List<string>();
            MissionHistory = new List<MissionRecord>();
        }

        /// <summary>
        /// Checks if a mission has been completed.
        /// </summary>
        public bool IsMissionCompleted(string missionId)
        {
            return CompletedMissionIds.Contains(missionId);
        }

        /// <summary>
        /// Checks if a mission has been failed.
        /// </summary>
        public bool IsMissionFailed(string missionId)
        {
            return FailedMissionIds.Contains(missionId);
        }

        /// <summary>
        /// Checks if a mission is available for selection.
        /// </summary>
        public bool IsMissionAvailable(string missionId)
        {
            return AvailableMissionIds.Contains(missionId);
        }

        /// <summary>
        /// Records a completed mission.
        /// </summary>
        public void CompleteMission(string missionId, MissionRecord record)
        {
            record.CompletedOnTurn = CurrentTurn;

            if (!CompletedMissionIds.Contains(missionId))
            {
                CompletedMissionIds.Add(missionId);
            }

            AvailableMissionIds.Remove(missionId);
            MissionHistory.Add(record);
            CurrentMissionIndex++;
        }

        /// <summary>
        /// Records a failed mission.
        /// </summary>
        public void FailMission(string missionId)
        {
            if (!FailedMissionIds.Contains(missionId))
            {
                FailedMissionIds.Add(missionId);
            }

            AvailableMissionIds.Remove(missionId);
        }

        /// <summary>
        /// Makes a mission available for selection.
        /// </summary>
        public void UnlockMission(string missionId)
        {
            if (!AvailableMissionIds.Contains(missionId) &&
                !CompletedMissionIds.Contains(missionId))
            {
                AvailableMissionIds.Add(missionId);
            }
        }

        /// <summary>
        /// Advances to the next act if possible.
        /// Returns true if advanced successfully.
        /// </summary>
        public bool AdvanceAct()
        {
            switch (CurrentAct)
            {
                case ActType.Act1_Firefighting:
                    CurrentAct = ActType.Act2_PowerScaling;
                    CurrentMissionIndex = 0;
                    return true;

                case ActType.Act2_PowerScaling:
                    CurrentAct = ActType.Act3_Climax;
                    CurrentMissionIndex = 0;
                    return true;

                case ActType.Act3_Climax:
                    // Already at final act
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Increments the turn counter.
        /// </summary>
        public void AdvanceTurn()
        {
            CurrentTurn++;
        }

        /// <summary>
        /// Gets the mission record for a completed mission.
        /// </summary>
        public MissionRecord? GetMissionRecord(string missionId)
        {
            int index = MissionHistory.FindIndex(r => r.MissionId == missionId);
            if (index >= 0)
            {
                return MissionHistory[index];
            }
            return null;
        }

        /// <summary>
        /// Gets total count of missions completed.
        /// </summary>
        public int GetTotalMissionsCompleted()
        {
            return CompletedMissionIds.Count;
        }

        /// <summary>
        /// Resets to starting state.
        /// </summary>
        public void Reset()
        {
            CurrentAct = ActType.Act1_Firefighting;
            CurrentMissionIndex = 0;
            CurrentTurn = 1;
            CompletedMissionIds.Clear();
            FailedMissionIds.Clear();
            AvailableMissionIds.Clear();
            MissionHistory.Clear();
        }
    }
}
```

**Checkpoint:** You should now have 7 state class files. Compile in Unity and fix any errors before continuing.

---

## Part 5: The Master GameState and Manager

Now we'll create the top-level container and the manager that ties everything together.

### Step 5.1: GameState Container

Create `Assets/Scripts/Core/GameState/GameState.cs`:

```csharp
using System;
using Game.Core.States;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Top-level container for all game state.
    /// This is the single source of truth for the entire game.
    /// Serializable for save/load functionality.
    /// </summary>
    [Serializable]
    public class GameState
    {
        /// <summary>
        /// Version number for save compatibility.
        /// Increment when making breaking changes to state structure.
        /// </summary>
        public int Version;

        /// <summary>Campaign progression and mission tracking.</summary>
        public CampaignState Campaign;

        /// <summary>Squad roster and unit management.</summary>
        public SquadState Squad;

        /// <summary>Resources and economy.</summary>
        public ResourceState Resources;

        /// <summary>Faction relationships and status.</summary>
        public FactionState Factions;

        /// <summary>Moral meter and choice history.</summary>
        public MoralState Moral;

        /// <summary>Tech unlocks and research.</summary>
        public TechState Tech;

        /// <summary>Story branching flags.</summary>
        public BranchState Branch;

        /// <summary>
        /// Creates a new GameState with default values.
        /// </summary>
        public GameState()
        {
            Version = 1;
            Campaign = new CampaignState();
            Squad = new SquadState();
            Resources = new ResourceState();
            Factions = new FactionState();
            Moral = new MoralState();
            Tech = new TechState();
            Branch = new BranchState();
        }

        /// <summary>
        /// Creates a deep copy of this state.
        /// Useful for undo functionality or comparing states.
        /// </summary>
        public GameState CreateDeepCopy()
        {
            // Use Unity's JSON serialization for a simple deep copy
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<GameState>(json);
        }

        /// <summary>
        /// Resets all sub-states to their defaults.
        /// </summary>
        public void Reset()
        {
            Campaign.Reset();
            Squad.Reset();
            Resources.Reset();
            Factions.Reset();
            Moral.Reset();
            Tech.Reset();
            Branch.Reset();
        }
    }
}
```

### Step 5.2: Event Definitions

Create `Assets/Scripts/Core/Events/GameStateEvents.cs`:

```csharp
using Game.Core.Data;

namespace Game.Core.Events
{
    /// <summary>
    /// Event fired when any part of the game state changes.
    /// </summary>
    public struct StateChangedEvent
    {
        public string ChangedSystem; // e.g., "Campaign", "Squad", "Resources"
    }

    /// <summary>
    /// Event fired when the campaign advances to a new act.
    /// </summary>
    public struct ActChangedEvent
    {
        public ActType PreviousAct;
        public ActType NewAct;
    }

    /// <summary>
    /// Event fired when a mission is completed.
    /// </summary>
    public struct MissionCompletedEvent
    {
        public string MissionId;
        public bool WasSuccessful;
    }

    /// <summary>
    /// Event fired when a faction is defeated.
    /// </summary>
    public struct FactionDefeatedEvent
    {
        public FactionType Faction;
    }

    /// <summary>
    /// Event fired when the player triggers defection.
    /// </summary>
    public struct DefectionTriggeredEvent
    {
        public FactionType FormerFaction;
    }

    /// <summary>
    /// Event fired when resources change.
    /// </summary>
    public struct ResourceChangedEvent
    {
        public ResourceType ResourceType;
        public int OldAmount;
        public int NewAmount;
    }

    /// <summary>
    /// Event fired when moral meter changes.
    /// </summary>
    public struct MoralChangedEvent
    {
        public int OldValue;
        public int NewValue;
        public int Delta;
    }

    /// <summary>
    /// Event fired when a unit's status changes.
    /// </summary>
    public struct UnitStatusChangedEvent
    {
        public string UnitId;
        public UnitStatus OldStatus;
        public UnitStatus NewStatus;
    }
}
```

### Step 5.3: Simple Event Bus

Create `Assets/Scripts/Core/Events/EventBus.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    /// <summary>
    /// Simple publish/subscribe event system for decoupled communication.
    /// Systems can subscribe to events without knowing about each other.
    /// </summary>
    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe to events of type T.
        /// </summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from events of type T.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                return;
            }

            // Create a copy to avoid issues if handlers modify the list
            var handlers = new List<Delegate>(_subscribers[eventType]);

            foreach (var handler in handlers)
            {
                ((Action<T>)handler)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Clears all subscribers. Useful for testing or scene transitions.
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}
```

### Step 5.4: GameStateManager

Create `Assets/Scripts/Core/GameState/GameStateManager.cs`:

```csharp
using System;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;

namespace Game.Core
{
    /// <summary>
    /// Singleton manager providing centralized access to all game state.
    /// Place this on a GameObject in your first scene.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        /// <summary>Singleton instance.</summary>
        public static GameStateManager Instance { get; private set; }

        /// <summary>The current game state.</summary>
        public GameState CurrentState { get; private set; }

        // Convenience accessors for sub-states
        public CampaignState Campaign => CurrentState.Campaign;
        public SquadState Squad => CurrentState.Squad;
        public ResourceState Resources => CurrentState.Resources;
        public FactionState Factions => CurrentState.Factions;
        public MoralState Moral => CurrentState.Moral;
        public TechState Tech => CurrentState.Tech;
        public BranchState Branch => CurrentState.Branch;

        // C# events for those who prefer them over EventBus
        public event Action<GameState> OnStateChanged;
        public event Action<ActType, ActType> OnActChanged;
        public event Action<string, bool> OnMissionCompleted;
        public event Action<FactionType> OnFactionDefeated;
        public event Action OnDefectionTriggered;

        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize with default state
            Initialize();
        }

        /// <summary>
        /// Initializes the manager with a new or provided state.
        /// </summary>
        public void Initialize(GameState state = null)
        {
            CurrentState = state ?? new GameState();
            NotifyStateChanged("All");
        }

        /// <summary>
        /// Resets the game state to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentState.Reset();
            NotifyStateChanged("All");
        }

        /// <summary>
        /// Gets a deep copy of current state (for save system).
        /// </summary>
        public GameState GetStateCopy()
        {
            return CurrentState.CreateDeepCopy();
        }

        /// <summary>
        /// Loads a state (from save system).
        /// </summary>
        public void LoadState(GameState state)
        {
            CurrentState = state;
            NotifyStateChanged("All");
        }

        // ==========================================
        // High-level game operations
        // These wrap sub-state operations and fire events
        // ==========================================

        /// <summary>
        /// Starts a new game with the chosen faction.
        /// </summary>
        public void StartNewGame(FactionType chosenFaction)
        {
            ResetToDefaults();
            Factions.SetPlayerFaction(chosenFaction);

            // Add starting units based on faction
            CreateStartingSquad(chosenFaction);

            NotifyStateChanged("All");
        }

        /// <summary>
        /// Records a completed mission and applies consequences.
        /// </summary>
        public void RecordMissionComplete(string missionId, MissionRecord record)
        {
            Campaign.CompleteMission(missionId, record);

            // Apply resource changes
            foreach (var gain in record.ResourcesGained)
            {
                Resources.AddResource(gain.Key, gain.Value);
            }

            // Apply moral choices
            foreach (var choice in record.MoralChoicesMade)
            {
                Moral.RecordChoice(choice);
            }

            // Update unit statuses
            foreach (var unitId in record.InjuredUnitIds)
            {
                Squad.UpdateUnitStatus(unitId, UnitStatus.Injured);
            }
            foreach (var unitId in record.DeadUnitIds)
            {
                Squad.UpdateUnitStatus(unitId, UnitStatus.Dead);
            }

            // Fire events
            OnMissionCompleted?.Invoke(missionId, record.WasSuccessful);
            EventBus.Publish(new MissionCompletedEvent
            {
                MissionId = missionId,
                WasSuccessful = record.WasSuccessful
            });

            NotifyStateChanged("Campaign");
        }

        /// <summary>
        /// Advances to the next act.
        /// </summary>
        public bool TryAdvanceAct()
        {
            ActType previousAct = Campaign.CurrentAct;

            if (Campaign.AdvanceAct())
            {
                ActType newAct = Campaign.CurrentAct;

                // Update squad size for new act
                Squad.UpdateSquadSizeForAct(newAct);

                // Fire events
                OnActChanged?.Invoke(previousAct, newAct);
                EventBus.Publish(new ActChangedEvent
                {
                    PreviousAct = previousAct,
                    NewAct = newAct
                });

                NotifyStateChanged("Campaign");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Triggers the defection at Act 2 climax.
        /// </summary>
        public void TriggerDefection()
        {
            Factions.TriggerDefection();
            Branch.SetPath(isLoyalist: false);
            Tech.RevokePrecursorAccess();

            // Handle unit defections based on loyalty
            foreach (var unit in Squad.GetPotentialDefectors())
            {
                // Low loyalty units stay with faction (become enemies)
                Squad.UpdateUnitStatus(unit.Id, UnitStatus.Defected);
                Branch.MarkUnitAsDefector(unit.Id);
            }

            OnDefectionTriggered?.Invoke();
            EventBus.Publish(new DefectionTriggeredEvent
            {
                FormerFaction = Factions.PlayerFaction
            });

            NotifyStateChanged("All");
        }

        /// <summary>
        /// Marks a faction as defeated at end of Act 2.
        /// </summary>
        public void DefeatFaction(FactionType faction)
        {
            Factions.SetStatus(faction, FactionRelationStatus.Defeated);
            Branch.SetDefeatedFaction(faction);

            OnFactionDefeated?.Invoke(faction);
            EventBus.Publish(new FactionDefeatedEvent { Faction = faction });

            NotifyStateChanged("Factions");
        }

        /// <summary>
        /// Advances the turn and processes end-of-turn effects.
        /// </summary>
        public void EndTurn()
        {
            Campaign.AdvanceTurn();
            Squad.ProcessInjuryRecovery();

            NotifyStateChanged("Campaign");
        }

        // ==========================================
        // Private helpers
        // ==========================================

        private void CreateStartingSquad(FactionType faction)
        {
            // Create 2 starting units (Act 1 squad size is 3, but start with 2)
            string prefix = faction.ToString().Substring(0, 3);

            Squad.AddUnit(UnitData.CreateDefault($"{prefix}_001", $"Recruit Alpha"));
            Squad.AddUnit(UnitData.CreateDefault($"{prefix}_002", $"Recruit Beta"));
        }

        private void NotifyStateChanged(string system)
        {
            OnStateChanged?.Invoke(CurrentState);
            EventBus.Publish(new StateChangedEvent { ChangedSystem = system });
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
```

**Checkpoint:** Compile everything. You should have no errors. The core state system is now complete!

---

## Part 6: Creating Unit Tests

Now let's write tests to verify our state classes work correctly.

### Step 6.1: ResourceState Tests

Create `Assets/Tests/EditMode/ResourceStateTests.cs`:

```csharp
using NUnit.Framework;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class ResourceStateTests
    {
        private ResourceState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new ResourceState();
        }

        [Test]
        public void Constructor_InitializesWithStartingValues()
        {
            Assert.AreEqual(GameConstants.STARTING_CURRENCY, _state.GetResource(ResourceType.Currency));
            Assert.AreEqual(GameConstants.STARTING_FUEL, _state.GetResource(ResourceType.Fuel));
        }

        [Test]
        public void AddResource_IncreasesAmount()
        {
            int initial = _state.GetResource(ResourceType.Currency);
            _state.AddResource(ResourceType.Currency, 100);
            Assert.AreEqual(initial + 100, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void SpendResource_DecreasesAmount_WhenSufficient()
        {
            _state.SetResource(ResourceType.Currency, 500);
            bool result = _state.SpendResource(ResourceType.Currency, 200);

            Assert.IsTrue(result);
            Assert.AreEqual(300, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void SpendResource_ReturnsFalse_WhenInsufficient()
        {
            _state.SetResource(ResourceType.Currency, 100);
            bool result = _state.SpendResource(ResourceType.Currency, 200);

            Assert.IsFalse(result);
            Assert.AreEqual(100, _state.GetResource(ResourceType.Currency)); // Unchanged
        }

        [Test]
        public void HasResource_ReturnsTrue_WhenSufficient()
        {
            _state.SetResource(ResourceType.Fuel, 50);
            Assert.IsTrue(_state.HasResource(ResourceType.Fuel, 50));
            Assert.IsTrue(_state.HasResource(ResourceType.Fuel, 25));
        }

        [Test]
        public void HasResource_ReturnsFalse_WhenInsufficient()
        {
            _state.SetResource(ResourceType.Fuel, 50);
            Assert.IsFalse(_state.HasResource(ResourceType.Fuel, 100));
        }

        [Test]
        public void AddResource_PreventsNegativeValues()
        {
            _state.SetResource(ResourceType.Currency, 100);
            _state.AddResource(ResourceType.Currency, -200);
            Assert.AreEqual(0, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void Reset_RestoresToStartingValues()
        {
            _state.SetResource(ResourceType.Currency, 9999);
            _state.Reset();
            Assert.AreEqual(GameConstants.STARTING_CURRENCY, _state.GetResource(ResourceType.Currency));
        }
    }
}
```

### Step 6.2: MoralState Tests

Create `Assets/Tests/EditMode/MoralStateTests.cs`:

```csharp
using NUnit.Framework;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class MoralStateTests
    {
        private MoralState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new MoralState();
        }

        [Test]
        public void Constructor_StartsAtZero()
        {
            Assert.AreEqual(0, _state.MoralMeter);
            Assert.AreEqual(0, _state.TotalEthicalChoices);
            Assert.AreEqual(0, _state.TotalEfficientChoices);
        }

        [Test]
        public void RecordChoice_Ethical_IncrementsCounter()
        {
            var choice = MoralChoice.Create("m1", "c1", "Save civilians", true, 10, 1);
            _state.RecordChoice(choice);

            Assert.AreEqual(1, _state.TotalEthicalChoices);
            Assert.AreEqual(0, _state.TotalEfficientChoices);
        }

        [Test]
        public void RecordChoice_Efficient_IncrementsCounter()
        {
            var choice = MoralChoice.Create("m1", "c1", "Rush objective", false, -10, 1);
            _state.RecordChoice(choice);

            Assert.AreEqual(0, _state.TotalEthicalChoices);
            Assert.AreEqual(1, _state.TotalEfficientChoices);
        }

        [Test]
        public void RecordChoice_AddsMoralImpact()
        {
            var choice = MoralChoice.Create("m1", "c1", "Test", true, 25, 1);
            _state.RecordChoice(choice);

            Assert.AreEqual(25, _state.MoralMeter);
        }

        [Test]
        public void ModifyMoral_ClampsToMax()
        {
            _state.ModifyMoral(200);
            Assert.AreEqual(GameConstants.MORAL_METER_MAX, _state.MoralMeter);
        }

        [Test]
        public void ModifyMoral_ClampsToMin()
        {
            _state.ModifyMoral(-200);
            Assert.AreEqual(GameConstants.MORAL_METER_MIN, _state.MoralMeter);
        }

        [Test]
        public void IsEthicalPath_ReturnsTrue_WhenPositive()
        {
            _state.ModifyMoral(10);
            Assert.IsTrue(_state.IsEthicalPath());
            Assert.IsFalse(_state.IsRuthlessPath());
        }

        [Test]
        public void IsRuthlessPath_ReturnsTrue_WhenNegative()
        {
            _state.ModifyMoral(-10);
            Assert.IsTrue(_state.IsRuthlessPath());
            Assert.IsFalse(_state.IsEthicalPath());
        }

        [Test]
        public void GetMoralRatio_ReturnsCorrectValue()
        {
            _state.MoralMeter = 50;
            Assert.AreEqual(0.5f, _state.GetMoralRatio(), 0.001f);

            _state.MoralMeter = -50;
            Assert.AreEqual(-0.5f, _state.GetMoralRatio(), 0.001f);
        }
    }
}
```

### Step 6.3: SquadState Tests

Create `Assets/Tests/EditMode/SquadStateTests.cs`:

```csharp
using NUnit.Framework;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class SquadStateTests
    {
        private SquadState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new SquadState();
        }

        [Test]
        public void AddUnit_IncreasesRosterCount()
        {
            var unit = UnitData.CreateDefault("u1", "Test Unit");
            _state.AddUnit(unit);

            Assert.AreEqual(1, _state.Roster.Count);
        }

        [Test]
        public void AddUnit_PreventsDuplicateIds()
        {
            var unit1 = UnitData.CreateDefault("u1", "Unit 1");
            var unit2 = UnitData.CreateDefault("u1", "Unit 2");

            _state.AddUnit(unit1);
            _state.AddUnit(unit2);

            Assert.AreEqual(1, _state.Roster.Count);
        }

        [Test]
        public void RemoveUnit_DecreasesRosterCount()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.RemoveUnit("u1");

            Assert.AreEqual(0, _state.Roster.Count);
        }

        [Test]
        public void GetActiveUnits_ExcludesInjuredAndDead()
        {
            _state.AddUnit(UnitData.CreateDefault("u1", "Active"));
            _state.AddUnit(UnitData.CreateDefault("u2", "Injured"));
            _state.AddUnit(UnitData.CreateDefault("u3", "Dead"));

            _state.UpdateUnitStatus("u2", UnitStatus.Injured);
            _state.UpdateUnitStatus("u3", UnitStatus.Dead);

            var active = _state.GetActiveUnits();
            Assert.AreEqual(1, active.Count);
            Assert.AreEqual("u1", active[0].Id);
        }

        [Test]
        public void AddExperience_TriggersLevelUp()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);

            // Add enough XP to level up (default is 100 XP to level)
            _state.AddExperience("u1", 150);

            var updated = _state.GetUnit("u1");
            Assert.AreEqual(2, updated.Value.Level);
        }

        [Test]
        public void ModifyLoyalty_ClampsToValidRange()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);

            _state.ModifyLoyalty("u1", 200);
            Assert.AreEqual(GameConstants.LOYALTY_MAX, _state.GetUnit("u1").Value.LoyaltyScore);

            _state.ModifyLoyalty("u1", -300);
            Assert.AreEqual(GameConstants.LOYALTY_MIN, _state.GetUnit("u1").Value.LoyaltyScore);
        }

        [Test]
        public void CanDeploy_ReturnsFalse_WhenUnitInjured()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.UpdateUnitStatus("u1", UnitStatus.Injured);

            Assert.IsFalse(_state.CanDeploy("u1"));
        }

        [Test]
        public void DeployUnit_AddsToDeployedList()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);

            bool result = _state.DeployUnit("u1");

            Assert.IsTrue(result);
            Assert.Contains("u1", _state.DeployedUnitIds);
        }

        [Test]
        public void DeployUnit_FailsWhenSquadFull()
        {
            _state.MaxSquadSize = 1;
            _state.AddUnit(UnitData.CreateDefault("u1", "Unit 1"));
            _state.AddUnit(UnitData.CreateDefault("u2", "Unit 2"));

            _state.DeployUnit("u1");
            bool result = _state.DeployUnit("u2");

            Assert.IsFalse(result);
        }

        [Test]
        public void ProcessInjuryRecovery_DecrementsRecoveryTurns()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.UpdateUnitStatus("u1", UnitStatus.Injured);

            int initialTurns = _state.GetUnit("u1").Value.InjuryRecoveryTurns;
            _state.ProcessInjuryRecovery();

            Assert.AreEqual(initialTurns - 1, _state.GetUnit("u1").Value.InjuryRecoveryTurns);
        }
    }
}
```

### Running the Tests

1. In Unity, go to `Window > General > Test Runner`
2. Click on the `EditMode` tab
3. Click `Run All` to run your tests
4. All tests should pass (green checkmarks)

If any tests fail, read the error message and fix the corresponding code.

---

## Part 7: Creating the Editor Debug Window

Let's create a simple editor tool to view and modify game state during development.

### Step 7.1: Debug Window

Create `Assets/Scripts/Editor/GameState/GameStateDebugWindow.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using Game.Core;
using Game.Core.Data;

namespace Game.Editor
{
    /// <summary>
    /// Editor window for viewing and debugging game state.
    /// Open via Window > Game Debug > Game State Debug
    /// </summary>
    public class GameStateDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Campaign", "Squad", "Resources", "Factions", "Moral", "Tech", "Branch" };

        [MenuItem("Window/Game Debug/Game State Debug")]
        public static void ShowWindow()
        {
            GetWindow<GameStateDebugWindow>("Game State Debug");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view game state.", MessageType.Info);
                return;
            }

            if (GameStateManager.Instance == null)
            {
                EditorGUILayout.HelpBox("GameStateManager not found in scene.", MessageType.Warning);
                return;
            }

            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0: DrawCampaignTab(); break;
                case 1: DrawSquadTab(); break;
                case 2: DrawResourcesTab(); break;
                case 3: DrawFactionsTab(); break;
                case 4: DrawMoralTab(); break;
                case 5: DrawTechTab(); break;
                case 6: DrawBranchTab(); break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Global controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults"))
            {
                GameStateManager.Instance.ResetToDefaults();
            }
            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCampaignTab()
        {
            var campaign = GameStateManager.Instance.Campaign;

            EditorGUILayout.LabelField("Campaign State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Act:", campaign.CurrentAct.ToString());
            EditorGUILayout.LabelField("Current Turn:", campaign.CurrentTurn.ToString());
            EditorGUILayout.LabelField("Mission Index:", campaign.CurrentMissionIndex.ToString());
            EditorGUILayout.LabelField("Missions Completed:", campaign.CompletedMissionIds.Count.ToString());

            EditorGUILayout.Space();

            if (GUILayout.Button("Advance Act"))
            {
                GameStateManager.Instance.TryAdvanceAct();
            }

            if (GUILayout.Button("End Turn"))
            {
                GameStateManager.Instance.EndTurn();
            }
        }

        private void DrawSquadTab()
        {
            var squad = GameStateManager.Instance.Squad;

            EditorGUILayout.LabelField("Squad State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Squad Size:", squad.MaxSquadSize.ToString());
            EditorGUILayout.LabelField("Total Units:", squad.Roster.Count.ToString());
            EditorGUILayout.LabelField("Active Units:", squad.GetActiveUnits().Count.ToString());
            EditorGUILayout.LabelField("Injured Units:", squad.GetInjuredUnits().Count.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unit Roster:", EditorStyles.boldLabel);

            foreach (var unit in squad.Roster)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{unit.DisplayName} (Lvl {unit.Level})");
                EditorGUILayout.LabelField($"Status: {unit.Status}, Loyalty: {unit.LoyaltyScore}");
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Test Unit"))
            {
                var newUnit = UnitData.CreateDefault(
                    $"test_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                    $"Test Unit {squad.Roster.Count + 1}"
                );
                squad.AddUnit(newUnit);
            }
        }

        private void DrawResourcesTab()
        {
            var resources = GameStateManager.Instance.Resources;

            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.ToString() + ":", GUILayout.Width(120));

                int current = resources.GetResource(type);
                int newValue = EditorGUILayout.IntField(current);

                if (newValue != current)
                {
                    resources.SetResource(type, newValue);
                }

                if (GUILayout.Button("+100", GUILayout.Width(50)))
                {
                    resources.AddResource(type, 100);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFactionsTab()
        {
            var factions = GameStateManager.Instance.Factions;

            EditorGUILayout.LabelField("Faction State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Player Faction:", factions.PlayerFaction.ToString());
            EditorGUILayout.LabelField("Is Defector:", factions.PlayerIsDefector.ToString());
            EditorGUILayout.LabelField("Target Faction:", factions.TargetedFaction.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Faction Relations:", EditorStyles.boldLabel);

            foreach (FactionType type in new[] { FactionType.Authoritarian, FactionType.Covenant, FactionType.Technocratic })
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(type.ToString());
                EditorGUILayout.LabelField($"  Loyalty: {factions.GetLoyalty(type)}");
                EditorGUILayout.LabelField($"  Status: {factions.GetStatus(type)}");
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Trigger Defection"))
            {
                GameStateManager.Instance.TriggerDefection();
            }
        }

        private void DrawMoralTab()
        {
            var moral = GameStateManager.Instance.Moral;

            EditorGUILayout.LabelField("Moral State", EditorStyles.boldLabel);

            // Moral meter with slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moral Meter:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{moral.MoralMeter} ({(moral.IsEthicalPath() ? "Ethical" : moral.IsRuthlessPath() ? "Ruthless" : "Neutral")})");
            EditorGUILayout.EndHorizontal();

            // Visual bar
            Rect rect = GUILayoutUtility.GetRect(100, 20);
            float normalizedValue = (moral.MoralMeter + 100) / 200f;
            EditorGUI.ProgressBar(rect, normalizedValue, "");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Ethical Choices: {moral.TotalEthicalChoices}");
            EditorGUILayout.LabelField($"Efficient Choices: {moral.TotalEfficientChoices}");
            EditorGUILayout.LabelField($"Total Choices: {moral.ChoiceHistory.Count}");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Ethical (+10)"))
            {
                var choice = MoralChoice.Create("debug", "d1", "Debug ethical", true, 10, GameStateManager.Instance.Campaign.CurrentTurn);
                moral.RecordChoice(choice);
            }
            if (GUILayout.Button("Add Efficient (-10)"))
            {
                var choice = MoralChoice.Create("debug", "d2", "Debug efficient", false, -10, GameStateManager.Instance.Campaign.CurrentTurn);
                moral.RecordChoice(choice);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTechTab()
        {
            var tech = GameStateManager.Instance.Tech;

            EditorGUILayout.LabelField("Tech State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Has Precursor Access: {tech.HasPrecursorAccess}");
            EditorGUILayout.LabelField($"Has Full Extinction Tech: {tech.HasFullExtinctionTech}");
            EditorGUILayout.LabelField($"Unlocked Techs: {tech.UnlockedTechIds.Count}");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Grant Precursor"))
            {
                tech.GrantPrecursorAccess();
            }
            if (GUILayout.Button("Revoke Precursor"))
            {
                tech.RevokePrecursorAccess();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBranchTab()
        {
            var branch = GameStateManager.Instance.Branch;

            EditorGUILayout.LabelField("Branch State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Path: {(branch.IsLoyalistPath ? "Loyalist" : "Defector")}");
            EditorGUILayout.LabelField($"Act 2 Decision Made: {branch.HasMadeAct2Decision}");
            EditorGUILayout.LabelField($"Defeated Faction: {branch.DefeatedFaction}");
            EditorGUILayout.LabelField($"Story Flags: {branch.ActiveStoryFlags.Count}");
            EditorGUILayout.LabelField($"Defector Units: {branch.DefectorUnitIds.Count}");

            EditorGUILayout.Space();
            if (branch.ActiveStoryFlags.Count > 0)
            {
                EditorGUILayout.LabelField("Active Flags:", EditorStyles.boldLabel);
                foreach (var flag in branch.ActiveStoryFlags)
                {
                    EditorGUILayout.LabelField($"  - {flag}");
                }
            }
        }

        private void OnInspectorUpdate()
        {
            // Repaint during play mode to show live updates
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
```

### Using the Debug Window

1. In Unity, go to `Window > Game Debug > Game State Debug`
2. Enter Play Mode
3. Use the tabs to explore different state systems
4. Use the buttons to modify state and test your systems

---

## Part 8: Setting Up the Scene

Now let's create a scene with the GameStateManager.

### Step 8.1: Create the Manager GameObject

1. Create a new scene or use your existing main scene
2. Create an empty GameObject: `GameObject > Create Empty`
3. Name it `GameStateManager`
4. Add the `GameStateManager` component to it
5. Save the scene

### Step 8.2: Test the System

1. Enter Play Mode
2. Open the Game State Debug window (`Window > Game Debug > Game State Debug`)
3. Try these actions:
   - Add test units in the Squad tab
   - Modify resources in the Resources tab
   - Make moral choices in the Moral tab
   - Advance the act in the Campaign tab
4. Verify all changes are reflected in the debug window

---

## Summary

Congratulations! You've completed Phase 1 of the game development. You now have:

- **7 State Classes** that track all game data
- **Enums and Structs** for type safety
- **GameStateManager** singleton for centralized access
- **EventBus** for decoupled communication
- **Unit Tests** to verify correctness
- **Editor Debug Window** for development testing

### What's Next (Phase 2: Save/Load System)

In the next phase, you'll add:
- JSON serialization for saving game state to disk
- Multiple save slots
- Version migration for save compatibility
- Auto-save functionality

### Files Created in This Phase

```
Assets/Scripts/Core/Data/Enums/
  - ActType.cs
  - FactionType.cs
  - UnitStatus.cs
  - MissionStatus.cs
  - FactionRelationStatus.cs
  - ResourceType.cs

Assets/Scripts/Core/Data/Structs/
  - UnitData.cs
  - MoralChoice.cs
  - MissionRecord.cs

Assets/Scripts/Core/Data/Constants/
  - GameConstants.cs

Assets/Scripts/Core/States/
  - CampaignState.cs
  - SquadState.cs
  - ResourceState.cs
  - FactionState.cs
  - MoralState.cs
  - TechState.cs
  - BranchState.cs

Assets/Scripts/Core/GameState/
  - GameState.cs
  - GameStateManager.cs

Assets/Scripts/Core/Events/
  - GameStateEvents.cs
  - EventBus.cs

Assets/Scripts/Editor/GameState/
  - GameStateDebugWindow.cs

Assets/Tests/EditMode/
  - ResourceStateTests.cs
  - MoralStateTests.cs
  - SquadStateTests.cs
```

Good luck with your tactical RPG!
