# Lesser Evils - Project Instructions

Tactical RPG built in Unity 6000.3.4f1 (C#). XCOM 2-inspired campaign structure with squad management, branching narrative, and faction systems.

## Workflow

The project is being built system-by-system following the master plan in `Documentation/00 Systems Plan/`. Each system goes through two stages:

1. **Plan** — A design document in `00 Systems Plan/` (all 15 are written)
2. **Tutorial folder** — A numbered folder (e.g., `01 System - Core Data Plan/`) containing step-by-step implementation guides with code, rationale, and integration notes

When asked to build a new system's tutorial folder:
- Read the corresponding plan from `00 Systems Plan/`
- Read `Documentation/01 System - Core Data Plan/00_overview.md` as the formatting reference
- Create a numbered folder with `00_overview.md` as the first file
- Write tutorial-style markdown files with full code blocks, design rationale, and integration patterns
- Each file should be self-contained enough to implement that piece independently

## Progress

| Phase | System | Plan | Tutorial | Code |
|-------|--------|------|----------|------|
| 01 | Core Data & State Management | Done | Done | Done |
| 02 | Save/Load System | Done | Done | Done |
| 03 | Campaign Flow & Progression | Done | Done | Not yet |
| 04 | Mission Management | Done | Done | Not yet |
| 05 | Mission Generation | Done | Done | Not yet |
| 06 | Squad & Unit Management | Done | Done | Not yet |
| 07 | Resource & Supply Economy | Done | Done | Not yet |
| 08-13 | Remaining systems | Done | Not yet | Not yet |

## Project Structure

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/    # GameConstants, SaveConstants
│   ├── Enums/        # ActType, FactionType, UnitStatus, etc.
│   └── Structs/      # UnitData, TechData, MissionRecord, etc.
├── Events/           # EventBus (generic pub/sub)
├── GameState/        # GameState, sub-states, GameStateManager
└── SaveSystem/       # SaveData, SaveManager, VersionMigrator
```

## Coding Conventions

- **Namespaces**: `Game.Core`, `Game.Core.Data`, `Game.Core.States`, `Game.Core.Events`, `Game.Core.SaveSystem`
- **Methods**: camelCase — `createDeepCopy()`, `addStoryFlag()`
- **Classes/Structs/Enums**: PascalCase — `GameState`, `UnitData`, `ResourceType`
- **Constants**: UPPER_SNAKE_CASE — `MAX_SQUAD_SIZE_1`, `CURRENT_SAVE_VERSION`
- **Private fields**: `_prefixed` — `_instance`
- **Indentation**: 4 spaces (see `.editorconfig`)
- **Section markers**: `// MARK: - SectionName`
- All data structs use `[Serializable]` for `JsonUtility` compatibility
- Static factory methods for struct initialization: `UnitData.CreateDefault()`

## Architecture Patterns

- **MonoBehaviour singletons**: `GameStateManager` — `Awake()` + `DontDestroyOnLoad`
- **Pure C# singletons**: `SaveManager` — private constructor, `_instance ??= new()`
- **EventBus**: Generic `publish<T>()` / `subscribe<T>()` for decoupled communication
- **C# events**: `Action<T>` delegates on managers for direct UI feedback
- **Serialization**: `JsonUtility.ToJson()` — JSON for readability during development
- **Deep copy**: Via JSON round-trip in `GameState.createDeepCopy()`
- **Error handling**: Try/catch with `Debug.LogError()`, methods return `bool` for success/failure

## Key Design Decisions

- **No explicit act transitions** — Story progression is organic (0.0-1.0 scale based on main missions completed + time elapsed)
- **XCOM 2-style time model** — Missions pause time; strategic layer advances time through player choices (research, heal, build, explore)
- **Difficulty pressure** — `worldThreatLevel` grows with time; slow players face harder enemies via `difficultyRatio = worldThreatLevel / playerPower`
- **JSON over binary** for saves — Human-readable, easy to debug, acceptable performance for this game type
- **No external dependencies** — Only Unity built-in packages

## Don'ts

- Don't add comments, docstrings, or type annotations to code that wasn't changed
- Don't over-engineer — no abstractions for one-time operations, no feature flags, no hypothetical future-proofing
- Don't create files unless necessary — prefer editing existing files
- Don't create README or documentation files unless explicitly asked
