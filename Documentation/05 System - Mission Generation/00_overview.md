# Phase 5: Mission Generation - Development Tutorial

Phase 4 introduced `GenericMissionRotation` as a simple shuffle-and-pick system. This phase replaces it with a proper generator that selects missions based on what the player actually needs, scales them with world threat, and adds procedural variation so repeated templates don't feel identical.

---

## What You'll Build

- **MissionTemplate** — A richer blueprint with variation ranges for enemy count, rewards, and optional objectives
- **MissionGenerator** — Smart selection that weighs templates based on player state (low on resources? more supply missions. High threat? harder encounters)
- **Procedural variation** — Randomized enemy counts, reward amounts, and objective combinations within defined ranges
- **Replaces** `GenericMissionRotation` from Phase 4 with `MissionGenerator`

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                  MISSION TEMPLATES                        │
│                                                           │
│  MissionTemplate         VariationRange                   │
│  (blueprint + ranges)    (min/max for scaling)            │
│                                                           │
│  Each template defines:                                   │
│  - Base mission data (category, objectives)               │
│  - Enemy count range (3-8)                                │
│  - Reward ranges (50-150 currency)                        │
│  - Threat tier (when this template becomes available)     │
│  - Weight modifiers (resource-aware selection)             │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                 MISSION GENERATOR                         │
│                                                           │
│  1. Filter templates by threat tier                       │
│  2. Score each template against player state              │
│     - Low on alloys? → Raid templates score higher        │
│     - Units injured? → Rescue templates score lower       │
│  3. Weighted random pick (up to MAX_AVAILABLE)            │
│  4. Instantiate with procedural variation                 │
│     - Randomize enemy count within range                  │
│     - Randomize rewards within range                      │
│     - Pick optional objectives from pool                  │
│  5. Scale by difficulty ratio                             │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                  MISSION MANAGER                          │
│                                                           │
│  MissionManager.refreshAvailability()                     │
│  calls MissionGenerator.generate() instead of             │
│  GenericMissionRotation.refreshRotation()                  │
└──────────────────────────────────────────────────────────┘
```

## Prerequisites

Before starting this phase, you should have:
- **Phase 1** completed — GameState, ResourceState, UnitRosterState
- **Phase 3** completed — ProgressionState, DifficultyCalculator
- **Phase 4** completed — MissionData, MissionPool, MissionManager

## File Plan

| File | Creates |
|------|---------|
| `01_mission_templates.md` | MissionTemplate class, VariationRange struct, template library |
| `02_mission_generator.md` | MissionGenerator with weighted selection and procedural variation |
| `03_integration.md` | Replacing GenericMissionRotation, connecting to MissionManager |
| `04_summary.md` | File checklist and next steps |
