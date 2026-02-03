# Phase 1: Core Data & State Management - Development Tutorial

Welcome to Phase 1 of building your tactical RPG! This tutorial will guide you through creating the foundational data layer that all other game systems will depend on. By the end, you'll have a robust state management system that tracks everything from squad status to tech research.

---

## What You'll Build

In this phase, you'll create:
- A centralized `GameState` that holds all game data
- Individual state classes: `CampaignState`, `UnitRosterState`, `ResourceState`, `MaterialState`, `TechState`
- A `GameStateManager` singleton that provides easy access to all state
- An `EventBus` for decoupled system communication
- C# `event Action<T>` delegates for direct state change notifications

## Current Implementation Status

**Implemented:**
- 5 State Classes (Campaign, UnitRoster, Resources, Materials, Tech)
- Core enums (ActType, FactionType, UnitStatus, ResourceType, MaterialType, MissionStatus)
- Core structs (UnitData, TechData, Choice, MissionRecord)
- GameStateManager singleton with event support
- EventBus publish/subscribe system

**Deferred to Future Phases:**
- FactionState (loyalty tracking, faction relationships)
- BranchState (defection flags, story branching)
- MoralState (moral meter, choice history)
- Editor Debug Window
- Unit Tests
