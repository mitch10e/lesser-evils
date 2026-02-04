# Part 1: Mission Data Types

This part defines the data structures that describe what a mission *is* — its type, category, objectives, rewards, and consequences.

## Step 1.1: Mission Type Enum

Create `Assets/Scripts/Core/Data/Enums/MissionType.cs`:

```csharp
namespace Game.Core.Data {

    public enum MissionType {
        Story,
        Generic
    }

}
```

- **Story** — Scripted missions tied to the main narrative. Ordered by prerequisites, advance `storyProgress` when completed.
- **Generic** — Rotating pool of optional missions (supply raids, recon, rescue). Scale with world threat. Don't advance the story but provide resources and XP.

## Step 1.2: Mission Category Enum

Create `Assets/Scripts/Core/Data/Enums/MissionCategory.cs`:

```csharp
namespace Game.Core.Data {

    public enum MissionCategory {
        Assault,
        Defense,
        Sabotage,
        Assassination,
        Raid,
        Recon,
        Rescue,
        TechRecovery
    }

}
```

Categories drive the *feel* of a mission — enemy composition, map type, objective structure. A single `MissionData` entry uses one category, which downstream systems (combat, map generation) can use to configure the encounter.

## Step 1.3: Objective Data

Create `Assets/Scripts/Core/Data/Structs/MissionObjectiveData.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionObjectiveData {

        public string id;

        public string description;

        public bool isOptional;

        public static ObjectiveData Create(string id, string description, bool optional = false) {
            return new ObjectiveData {
                id = id,
                description = description,
                isOptional = optional
            };
        }

    }

}
```

Objectives are intentionally simple — an ID and a description. The combat system is responsible for determining *how* an objective is completed (kill target, reach extraction, hold position). This struct just defines *what* the player needs to do.

## Step 1.4: Mission Rewards & Loot

Rewards break into three categories: **resources** (currency, alloys — guaranteed on success), **performance-based XP** (distributed per-unit based on combat contribution), and **loot drops** (materials and equipment that enemies drop mid-mission and the player can optionally retrieve).

### Loot Category Enum

Create `Assets/Scripts/Core/Data/Enums/LootCategory.cs`:

```csharp
namespace Game.Core.Data {

    public enum LootCategory {
        Material,
        Equipment
    }

}
```

- **Material** — Maps to `MaterialType`. Quantities of crafting/upgrade materials (alloy fragments, power cells, etc.)
- **Equipment** — Individual items referenced by ID. Attachments (scopes, grips), power cores, unique gear. The equipment system doesn't exist yet — this is the hook for it.

### Loot Drop Struct

Create `Assets/Scripts/Core/Data/Structs/LootDrop.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct LootDrop {

        public LootCategory category;

        public MaterialType materialType;

        public string equipmentID;

        public int quantity;

        // MARK: - Factory

        public static LootDrop CreateMaterial(MaterialType type, int quantity) {
            return new LootDrop {
                category = LootCategory.Material,
                materialType = type,
                equipmentID = "",
                quantity = quantity
            };
        }

        public static LootDrop CreateEquipment(string equipmentID) {
            return new LootDrop {
                category = LootCategory.Equipment,
                equipmentID = equipmentID,
                quantity = 1
            };
        }

    }

}
```

**Why two fields instead of a generic ID?** `materialType` stays type-safe against the `MaterialType` enum, while `equipmentID` is a string because the equipment/item system doesn't exist yet. Factory methods hide this — callers never set both.

### Unit Performance Struct

Create `Assets/Scripts/Core/Data/Structs/UnitPerformance.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct UnitPerformance {

        public string unitID;

        public int kills;

        public int damageDealt;

        public int objectivesCompleted;

        // MARK: - XP Weighting

        public const int POINTS_PER_KILL = 10;
        public const int POINTS_PER_DAMAGE = 1;
        public const int POINTS_PER_OBJECTIVE = 15;
        public const float MIN_PARTICIPATION_SHARE = 0.1f;

        public int getContributionScore() {
            return (kills * POINTS_PER_KILL)
                 + (damageDealt * POINTS_PER_DAMAGE)
                 + (objectivesCompleted * POINTS_PER_OBJECTIVE);
        }

        public static UnitPerformance Create(string unitID) {
            return new UnitPerformance {
                unitID = unitID,
                kills = 0,
                damageDealt = 0,
                objectivesCompleted = 0
            };
        }

    }

}
```

The combat system creates one `UnitPerformance` per deployed unit and fills in the stats as the mission plays out. The outcome processor uses `getContributionScore()` to weight XP distribution.

**`MIN_PARTICIPATION_SHARE`** — Units with 0 contribution (held position, missed every shot) still get 10% of an equal share. They were deployed, they learn something.

### Updated MissionRewards

Create `Assets/Scripts/Core/Data/Structs/MissionRewards.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionRewards {

        public Dictionary<ResourceType, int> resources;

        public Dictionary<MaterialType, int> materials;

        public int baseXPPool;

        public string[] techUnlockIDs;

        public LootDrop[] potentialDrops;

        public static MissionRewards Create(
            int baseXPPool = 0,
            string[] techUnlocks = null,
            LootDrop[] drops = null
        ) {
            return new MissionRewards {
                resources = new Dictionary<ResourceType, int>(),
                materials = new Dictionary<MaterialType, int>(),
                baseXPPool = baseXPPool,
                techUnlockIDs = techUnlocks ?? new string[0],
                potentialDrops = drops ?? new LootDrop[0]
            };
        }

    }

}
```

**Key changes from the old flat `squadXP`:**

| Before | After |
|--------|-------|
| `squadXP` — flat amount split equally to all deployed units | `baseXPPool` — total XP available, distributed by each unit's `UnitPerformance` contribution score |
| No loot concept | `potentialDrops` — loot that enemies can drop mid-mission |
| Materials only as guaranteed rewards | Materials can also appear as retrievable enemy drops |

### MissionRecord Additions

The existing `MissionRecord` struct needs two new fields to carry the combat output back to the outcome processor:

```csharp
// Add to MissionRecord:
public List<UnitPerformance> unitPerformances;
public List<LootDrop> collectedLoot;
```

- `unitPerformances` — One per deployed unit. The combat system fills in kills, damage, objectives during the mission.
- `collectedLoot` — Subset of `MissionRewards.potentialDrops` that the player actually retrieved. The combat system tracks which drops the player picked up.

Update the `Create()` factory to initialize both:

```csharp
unitPerformances = new List<UnitPerformance>(),
collectedLoot = new List<LootDrop>()
```

### Loot Retrieval Flow

```
MissionRewards.potentialDrops     (designer defines what CAN drop)
        │
        ▼
Combat spawns drops on the map    (enemies die → loot appears at position)
        │
        ▼
Player chooses to retrieve or not (walk a unit to the drop, costs movement/actions)
        │
        ▼
MissionRecord.collectedLoot       (only retrieved drops reported)
        │
        ▼
MissionOutcomeProcessor           (adds collected materials/equipment to inventory)
```

This creates a tactical tradeoff: go out of your way to grab that power core, or play it safe and extract. High-risk loot positions near enemy clusters reward aggressive play.

### XP Distribution Flow

```
MissionRewards.baseXPPool = 100
        │
        ▼
UnitPerformance scores:
  Unit A: 5 kills, 200 dmg → score 70
  Unit B: 0 kills, 50 dmg  → score 50
  Unit C: 0 kills, 0 dmg   → score 0 (MIN_PARTICIPATION_SHARE)
        │
        ▼
Total contribution = 70 + 50 = 120  (Unit C excluded from pool math)
  Unit A share: 70/120 = 58.3% → 58 XP
  Unit B share: 50/120 = 41.7% → 42 XP
  Unit C share: MIN_PARTICIPATION_SHARE × (100/3) = 10% × 33 ≈ 3 XP
```

Units that contribute more grow faster. Units that sit in the back still get something. On failure, the entire pool is reduced to 25% before distribution (learning from defeat).

**Design note:** Rewards are defined per-mission in the data. The outcome processor scales `baseXPPool` and resource rewards based on difficulty ratio (harder missions → bonus rewards as a catch-up mechanic, as defined in `DifficultyCalculator`).

## Step 1.5: Mission Consequences

Create `Assets/Scripts/Core/Data/Structs/MissionConsequences.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionConsequences {

        public string[] storyFlagsOnSuccess;

        public string[] storyFlagsOnFailure;

        public Dictionary<FactionType, int> factionShiftsOnSuccess;

        public Dictionary<FactionType, int> factionShiftsOnFailure;

        public float storyProgressOnSuccess;

        public static MissionConsequences Create(float storyProgress = 0f) {
            return new MissionConsequences {
                storyFlagsOnSuccess = new string[0],
                storyFlagsOnFailure = new string[0],
                factionShiftsOnSuccess = new Dictionary<FactionType, int>(),
                factionShiftsOnFailure = new Dictionary<FactionType, int>(),
                storyProgressOnSuccess = storyProgress
            };
        }

    }

}
```

Consequences separate success from failure outcomes. Story missions carry `storyProgressOnSuccess` to advance the organic progression. Generic missions typically have `0f` for story progress but may still set narrative flags or shift faction standing.

## Step 1.6: Expanded MissionData

Replace the existing `MainMissionData` struct with a comprehensive `MissionData` class.

Create `Assets/Scripts/Core/Data/MissionData.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public class MissionData {

        public string id;

        public string title;

        public string description;

        public MissionType missionType;

        public MissionCategory category;

        // MARK: - Requirements

        public string[] prerequisiteMissionIDs;

        public string[] requiredStoryFlags;

        public FactionType factionRestriction;

        // MARK: - Content

        public ObjectiveData[] primaryObjectives;

        public ObjectiveData[] optionalObjectives;

        public string[] moralChoiceIDs;

        // MARK: - Outcomes

        public MissionRewards rewards;

        public MissionConsequences consequences;

        // MARK: - Scaling

        public int baseEnemyCount;

        public int recommendedSquadLevel;

        // MARK: - Factory

        public static MissionData CreateStory(
            string id,
            string title,
            string description,
            MissionCategory category,
            float storyProgress,
            params string[] prerequisites
        ) {
            return new MissionData {
                id = id,
                title = title,
                description = description,
                missionType = MissionType.Story,
                category = category,
                prerequisiteMissionIDs = prerequisites ?? new string[0],
                requiredStoryFlags = new string[0],
                factionRestriction = FactionType.None,
                primaryObjectives = new ObjectiveData[0],
                optionalObjectives = new ObjectiveData[0],
                moralChoiceIDs = new string[0],
                rewards = MissionRewards.Create(),
                consequences = MissionConsequences.Create(storyProgress),
                baseEnemyCount = 6,
                recommendedSquadLevel = 1
            };
        }

        public static MissionData CreateGeneric(
            string id,
            string title,
            string description,
            MissionCategory category
        ) {
            return new MissionData {
                id = id,
                title = title,
                description = description,
                missionType = MissionType.Generic,
                category = category,
                prerequisiteMissionIDs = new string[0],
                requiredStoryFlags = new string[0],
                factionRestriction = FactionType.None,
                primaryObjectives = new ObjectiveData[0],
                optionalObjectives = new ObjectiveData[0],
                moralChoiceIDs = new string[0],
                rewards = MissionRewards.Create(),
                consequences = MissionConsequences.Create(),
                baseEnemyCount = 4,
                recommendedSquadLevel = 1
            };
        }

    }

}
```

**Why a class instead of a struct?**
- `MissionData` is a large, complex object with arrays and nested structs
- It gets passed around by reference (not copied on every assignment)
- It's stored in pools and lists, not as a value field on another serialized object
- `MainMissionData` (the existing struct) was fine when it was just id + title + prerequisites — this replaces it for the mission system while keeping `MainMissionData` intact for `ProgressionState` compatibility

**Key fields:**
- `factionRestriction` — `FactionType.None` means any faction can access it. Set to a specific faction for branch-locked missions.
- `requiredStoryFlags` — e.g., `["rescued_scientist"]` means the player must have made a specific narrative choice to see this mission.
- `moralChoiceIDs` — References to choice definitions that the combat system presents during the mission. The choice outcomes get recorded in `MissionRecord.choicesMade`.
- `baseEnemyCount` / `recommendedSquadLevel` — Baseline values that `DifficultyCalculator` scales at runtime.

## Relationship to Existing Types

```
MissionData (definition - what the mission IS)
    │
    ├── MissionType, MissionCategory (classification)
    ├── ObjectiveData[] (what the player does)
    ├── MissionRewards (what the player gains)
    │     ├── baseXPPool (distributed by performance)
    │     └── potentialDrops (loot enemies can drop)
    ├── MissionConsequences (what changes in the world)
    │
    ▼
MissionRecord (outcome - what actually HAPPENED)
    │
    ├── wasSuccessful, turnsTaken
    ├── completedOptionalObjectives
    ├── injuredUnitIDs, deadUnitIDs
    ├── unitPerformances (per-unit kills, damage, objectives)
    ├── collectedLoot (drops the player retrieved)
    ├── resourcesGained/Spent
    └── choicesMade (List<Choice>)
```

`MissionData` is the blueprint. `MissionRecord` is the after-action report. The outcome processor (Part 4) bridges the two.

**Checkpoint:** Create these files and verify they compile. The existing `MainMissionData` stays intact — `MissionData` is the expanded version used by the mission system.
