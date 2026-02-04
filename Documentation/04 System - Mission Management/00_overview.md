# Phase 4: Mission Management - Development Tutorial

This phase builds the system for defining, presenting, and resolving missions. By the end, you'll have a data-driven mission pipeline that separates story missions from generic missions, tracks their lifecycle, and feeds results back into GameState.

---

## What You'll Build

- **Mission data types** — Expanded structs for defining missions with objectives, rewards, consequences, and prerequisites
- **New enums** — `MissionType`, `MissionCategory` for classifying missions
- **Mission pools** — Separate story and generic mission pools with availability logic
- **MissionManager** — Singleton that owns the mission lifecycle (available → active → resolved)
- **Outcome processing** — Applies mission results (rewards, casualties, narrative flags) back to GameState

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    MISSION DATA                           │
│                                                           │
│  MissionData          ObjectiveData      MissionRewards   │
│  (definition)         (per-objective)    (what you gain)  │
│                                                           │
│  MissionType          MissionCategory    MissionConseq.   │
│  (Story/Generic)      (Raid/Recon/etc)   (what changes)   │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                   MISSION POOL                            │
│                                                           │
│  Story missions:  Ordered, prerequisite-gated             │
│  Generic missions: Rotating pool, scales with threat      │
│                                                           │
│  ContentUnlocker → determines availability                │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                  MISSION MANAGER                          │
│                                                           │
│  Available → Player selects → Active → Combat → Resolved  │
│                                                           │
│  enterMission() ─► StrategicLayerManager (time pauses)    │
│  exitMission()  ─► Process outcomes, resume strategic     │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                OUTCOME PROCESSING                         │
│                                                           │
│  MissionRecord ─► Apply rewards to ResourceState          │
│                ─► Apply XP to UnitRosterState              │
│                ─► Apply casualties (injured/dead)          │
│                ─► Add story flags to CampaignState         │
│                ─► Update ProgressionState                  │
└──────────────────────────────────────────────────────────┘
```

## Prerequisites

Before starting this phase, you should have:
- **Phase 1** completed — GameState, all state classes, EventBus
- **Phase 3** completed — ProgressionState, StrategicLayerManager, ContentUnlocker

## Existing Code This Phase Builds On

| File | What It Provides |
|------|-----------------|
| `MissionStatus.cs` | `Locked`, `Unlocked`, `Active`, `Completed`, `Failed` enum |
| `MainMissionData.cs` | Basic mission struct (id, title, prerequisites) — will be expanded |
| `MissionRecord.cs` | Outcome tracking (casualties, resources, choices) |
| `Choice.cs` | In-mission decision tracking |
| `MissionCompletedEvent.cs` | EventBus event for mission completion |
| `ContentUnlocker.cs` | Prerequisite-based mission availability |
| `StrategicLayerManager.cs` | `enterMission()` / `exitMission()` for time pause |

## File Plan

| File | Creates |
|------|---------|
| `01_mission_data.md` | MissionType, MissionCategory, ObjectiveData, MissionRewards, MissionConsequences, expanded MissionData |
| `02_mission_pool.md` | MissionPool, story/generic pool management, availability checks |
| `03_mission_manager.md` | MissionManager singleton, mission lifecycle, strategic layer integration |
| `04_mission_outcomes.md` | MissionOutcomeProcessor, reward/consequence application |
| `05_summary.md` | File checklist and next steps |
