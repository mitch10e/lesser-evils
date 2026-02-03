# Part 3: Creating Data Structs

Structs are lightweight data containers. We'll use them for individual records like unit data and moral choices.

## Step 3.1: UnitData Struct

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
                EquippedLoadoutId = "",
                InjuryRecoveryTurns = 0,
                IsNamedCharacter = false
            };
        }
    }
}
```

**What you learned:** The `[Serializable]` attribute tells Unity this struct can be saved/loaded. The static factory method `CreateDefault` gives us a convenient way to create units with sensible starting values.

## Step 3.2: Choice Struct

Create `Assets/Scripts/Core/Data/Structs/Choice.cs`:

```csharp
using System;

namespace Game.Core.Data
{
    /// <summary>
    /// Records a single decision made during gameplay.
    /// Used to track player decisions and influence consequences.
    /// </summary>
    [Serializable]
    public struct Choice
    {
        /// <summary>ID of the mission where this choice occurred.</summary>
        public string MissionId;

        /// <summary>Unique identifier for this specific choice.</summary>
        public string ChoiceId;

        /// <summary>Human-readable description of the choice made.</summary>
        public string Description;

        /// <summary>Which option the player selected (e.g., OptionA vs OptionB).</summary>
        public int SelectedOption;

        /// <summary>How much this choice impacts game state.</summary>
        public int Impact;

        /// <summary>Game turn when this choice was made. For timeline tracking.</summary>
        public int TurnNumber;

        /// <summary>
        /// Creates a choice record.
        /// </summary>
        public static Choice Create(
            string missionId,
            string choiceId,
            string description,
            int selectedOption,
            int impact,
            int turn)
        {
            return new Choice
            {
                MissionId = missionId,
                ChoiceId = choiceId,
                Description = description,
                SelectedOption = selectedOption,
                Impact = impact,
                TurnNumber = turn
            };
        }
    }
}
```

## Step 3.3: MissionRecord Struct

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

        /// <summary>Choices made during this mission.</summary>
        public List<Choice> ChoicesMade;

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
                ChoicesMade = new List<Choice>(),
                CompletedOnTurn = 0
            };
        }
    }
}
```

## Step 3.4: TechData Struct

Create `Assets/Scripts/Core/Data/Structs/TechData.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Data
{
    /// <summary>
    /// Defines a technology that can be researched.
    /// Contains all static data about the tech - costs, time, prerequisites.
    /// </summary>
    [Serializable]
    public struct TechData
    {
        /// <summary>Unique identifier for this technology.</summary>
        public string Id;

        /// <summary>Display name shown in UI.</summary>
        public string Name;

        /// <summary>Description of what this tech does or unlocks.</summary>
        public string Description;

        /// <summary>Number of turns required to complete research.</summary>
        public int ResearchTimeRequired;

        /// <summary>IDs of technologies that must be unlocked before this can be researched.</summary>
        public List<string> PrerequisiteTechIds;

        /// <summary>Standard resource costs to begin research (paid upfront).</summary>
        public Dictionary<ResourceType, int> ResourceCosts;

        /// <summary>Special material costs to begin research (mission collectibles).</summary>
        public Dictionary<MaterialType, int> MaterialCosts;

        /// <summary>
        /// Creates a new tech definition.
        /// </summary>
        public static TechData Create(
            string id,
            string name,
            string description,
            int researchTime,
            List<string> prerequisites = null,
            Dictionary<ResourceType, int> resourceCosts = null,
            Dictionary<MaterialType, int> materialCosts = null)
        {
            return new TechData
            {
                Id = id,
                Name = name,
                Description = description,
                ResearchTimeRequired = researchTime,
                PrerequisiteTechIds = prerequisites ?? new List<string>(),
                ResourceCosts = resourceCosts ?? new Dictionary<ResourceType, int>(),
                MaterialCosts = materialCosts ?? new Dictionary<MaterialType, int>()
            };
        }
    }
}
```

**What you learned:** TechData defines WHAT a technology is. This is static data - it doesn't change during gameplay. The actual research progress is tracked in TechState. This separation lets you define your tech tree once (later via ScriptableObjects) while TechState handles the player's progress.

## Step 3.5: Game Constants

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

**Checkpoint:** You should now have 5 files in the Structs and Constants folders. Compile in Unity and fix any errors.
