# Phase 3: Campaign Flow & Progression - Development Tutorial

Welcome to Phase 3! This system controls the pacing of your tactical RPG through organic progression rather than explicit act transitions. Following the XCOM 2: War of the Chosen model, **missions pause time** while the **strategic layer advances time through player choices**.

---

## Core Design Philosophy

**No "Act 2 Begins" Moments**

Traditional games announce act transitions with fanfare. This system takes a different approach:
- The story unfolds naturally based on completed missions
- Difficulty scales based on time elapsed in the strategic layer
- Players who fall behind feel the pressure without being told "you're in Act 2 now"

**XCOM-Style Time Flow**

```
┌─────────────────────────────────────────┐
│           TACTICAL LAYER                │
│          (During Missions)              │
│                                         │
│    ⏸️  TIME IS PAUSED                   │
│    Focus on combat, no world changes    │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│          STRATEGIC LAYER                │
│         (Base / Geoscape)               │
│                                         │
│    ▶️  TIME ADVANCES via player actions: │
│    • Research tech (costs hours)        │
│    • Heal injured units (costs hours)   │
│    • Build facilities (costs hours)     │
│    • Explore/Expedition (costs hours)   │
│    • Wait for opportunities             │
└─────────────────────────────────────────┘
```

**Two Clocks, One Pressure**

```
Player Progress ────────────────────────►
(Driven by completing main story missions)

World Threat ───────────────────────────►
(Driven by strategic layer time, independent of missions)
```

When player progress outpaces world threat: comfortable difficulty, room for side content.
When world threat outpaces player progress: enemies feel stronger, urgency to advance.

## What You'll Build

In this phase, you'll create:
- `ProgressionState` - Tracks story progress, world threat, and elapsed time
- `StrategicLayerManager` - Controls time flow and player actions
- `TimeAction` - Represents activities that consume time
- `DifficultyCalculator` - Computes enemy scaling based on progression state
- `ContentUnlocker` - Determines what missions/tech are available
- Integration with `GameStateManager`

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                 StrategicLayerManager                    │
│   (Controls time flow, processes player actions)         │
└───────────────────────────┬─────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                   GameStateManager                       │
│              (Owns ProgressionState)                     │
└─────────────────────┬───────────────────────────────────┘
                      │
        ┌─────────────┴─────────────┐
        ▼                           ▼
┌───────────────────┐     ┌───────────────────┐
│  ProgressionState │     │  DifficultyCalc   │
│  - storyProgress  │────►│  - enemyScaling   │
│  - worldThreat    │     │  - difficultyRatio│
│  - elapsedHours   │     └─────────┬─────────┘
└───────────────────┘               │
                          ┌─────────┴─────────┐
                          ▼                   ▼
                   ┌─────────────┐     ┌─────────────┐
                   │  Missions   │     │   Combat    │
                   │  (scaling)  │     │  (modifiers)│
                   └─────────────┘     └─────────────┘
```

## Key Concepts

### Story Progress (0.0 to 1.0)

A normalized value representing how far the player has advanced through the main story:

```csharp
storyProgress = completedMainMissions / totalMainMissions;
```

| Progress | Narrative Phase |
|----------|-----------------|
| 0.00 - 0.25 | Opening - Establishing the conflict |
| 0.25 - 0.50 | Rising Action - Factions clash |
| 0.50 - 0.75 | Climax - Major revelations |
| 0.75 - 1.00 | Resolution - Endgame |

### World Threat Level

A value that increases with strategic layer time, representing how the world evolves:

```csharp
// Only increases when player spends time on strategic actions
worldThreatLevel = baseLevel + (elapsedStrategicHours * threatGrowthRate);
```

This creates pressure without a ticking timer on screen—the player controls the pace, but choices have consequences.

### Strategic Actions and Time

| Action | Typical Duration | Effect |
|--------|------------------|--------|
| Research Tech | 24-72 hours | Unlocks new equipment/abilities |
| Heal Unit | 24-48 hours | Returns injured unit to active |
| Build Facility | 48-168 hours | New base capability |
| Expedition | 12-24 hours | Resources, intel, events |
| Wait | Variable | Skip to next mission/event |

### Difficulty Ratio

The relationship between player power and world threat:

```csharp
difficultyRatio = worldThreatLevel / playerPowerEstimate;

// ratio < 1.0: Player is ahead, comfortable
// ratio ≈ 1.0: Player is on pace, balanced
// ratio > 1.0: Player is behind, challenging
```

## The Game Loop

```
        ┌───────────────────────────────────┐
        │         MISSION BOARD             │
        │   (Choose mission to attempt)     │
        └───────────────┬───────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────┐
        │       TACTICAL COMBAT             │
        │   (Time frozen during mission)    │
        └───────────────┬───────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────┐
        │        MISSION RESULTS            │
        │   (XP, loot, story progress)      │
        └───────────────┬───────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────────────┐
│                  STRATEGIC LAYER                       │
│                                                        │
│   ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐ │
│   │Research │  │  Heal   │  │  Build  │  │ Explore │ │
│   │  Lab    │  │ Medbay  │  │Facility │  │  World  │ │
│   └────┬────┘  └────┬────┘  └────┬────┘  └────┬────┘ │
│        │            │            │            │       │
│        └────────────┴─────┬──────┴────────────┘       │
│                           │                           │
│                    TIME PASSES                        │
│               World Threat Increases                  │
│              New Missions May Appear                  │
└───────────────────────────┬───────────────────────────┘
                            │
                            └──────► Back to Mission Board
```

## Prerequisites

Before starting this phase, ensure you have:
- Completed Phase 1 (Core Data & State Management)
- `CampaignState` with `elapsedTime` field
- A working event system (EventBus)
- Understanding of ScriptableObjects
