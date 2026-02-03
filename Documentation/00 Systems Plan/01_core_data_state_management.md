# 1. Core Data & State Management

**Purpose:** Single source of truth for all game state. Every other system reads from or writes to this.

**Responsibilities:**
- Holds all persistent game data: current act, faction affiliation, squad roster, resources, materials, tech unlocks, and story flags.
- Provides a clean API so systems don't depend on each other directly — they all talk through this manager.
- Serializable for save/load via JSON.

**Key Data Structures:**
- `GameState` — top-level container with version tracking
- `CampaignState` — current act, elapsed time, starting/current faction, story flags
- `UnitRosterState` — unit roster, levels, status (Active/Injured/Captured/Missing/Dead), deployment
- `ResourceState` — currency and material counts (Currency, Alloys, TechComponents, Intel)
- `MaterialState` — special crafting materials (placeholder for expansion)
- `TechState` — unlocked techs, current research progress

**Event System:**
- `EventBus` — static publish/subscribe system for decoupled communication
- C# `event Action<T>` — direct events on GameStateManager for tight coupling when needed

