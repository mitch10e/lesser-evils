# Phase 2: Save / Load System - Development Tutorial

Welcome to Phase 2! Now that you have a working GameState system, it's time to persist that data between play sessions. This tutorial will guide you through building a complete save/load system with multiple slots, version migration, and auto-save support.

---

## What You'll Build

In this phase, you'll create:
- A `SaveData` wrapper that adds metadata (timestamp, slot info, version)
- A `SaveManager` singleton for all save/load operations
- JSON serialization to persist `GameState` to disk
- Multiple save slot support (3 manual + 1 auto-save)
- Version migration system for save compatibility
- Integration with `GameStateManager`

## Prerequisites

Before starting this phase, ensure you have:
- Completed Phase 1 (Core Data & State Management)
- A working `GameState` class with the `version` field
- All state classes marked with `[Serializable]`
- `GameStateManager` singleton in your scene

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    GameStateManager                      │
│  (Owns current GameState, calls SaveManager for I/O)    │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                      SaveManager                         │
│  - getSaveSlots()                                        │
│  - save(slotIndex)                                       │
│  - load(slotIndex)                                       │
│  - delete(slotIndex)                                     │
│  - autoSave()                                            │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                      SaveData                            │
│  - GameState gameState                                   │
│  - string timestamp                                      │
│  - int slotIndex                                         │
│  - int saveVersion                                       │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                    Disk (JSON)                           │
│  PersistentDataPath/Saves/                               │
│    - slot_0.json                                         │
│    - slot_1.json                                         │
│    - slot_2.json                                         │
│    - autosave.json                                       │
└─────────────────────────────────────────────────────────┘
```

## Design Decisions

**Why JSON over Binary?**
- Human-readable for debugging
- Easy to manually edit during development
- Compatible with Unity's `JsonUtility`
- Binary can be added later for release builds if needed

**Why a SaveData Wrapper?**
- Separates metadata from game data
- `GameState.version` tracks data structure changes
- `SaveData` tracks save file metadata (when saved, which slot)

**Why Singleton SaveManager?**
- Centralizes all file I/O
- Easy to mock for testing
- Consistent error handling
