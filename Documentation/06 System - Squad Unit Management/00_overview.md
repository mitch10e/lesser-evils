# Phase 6: Squad & Unit Management - Development Tutorial

Phase 1 built the core `UnitRosterState` and `UnitData`. This phase adds the systems that make squad management meaningful — loyalty that affects obedience, injury recovery that costs strategic time, loadouts that define combat capability, and squad size that grows with progression.

---

## What You'll Build

- **Loyalty system** — Per-unit loyalty score that shifts based on moral choices and campaign events. Low-loyalty units may refuse orders or defect.
- **Injury recovery** — Injured units require healing time via the strategic layer. Creates roster pressure and forces risk management.
- **Loadout system** — Data structures for equipping units with weapons and gear.
- **Squad upgrades** — Mechanism for increasing max squad size through progression.
- **Recruitment** — Adding new units to the roster mid-campaign.

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                     UNIT DATA                             │
│                                                           │
│  UnitData (existing)         LoadoutData (new)            │
│  + loyaltyScore              (weapon, armor, items)       │
│  + recoveryHoursRemaining                                 │
│                                                           │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│               UNIT ROSTER STATE (existing)                │
│                                                           │
│  + loyalty methods (shift, check thresholds)               │
│  + recovery methods (start healing, tick recovery)         │
│  + recruitment (add mid-campaign)                          │
│  + squad size upgrade                                      │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                   INTEGRATION                             │
│                                                           │
│  StrategicLayerManager ─► Healing TimeActions              │
│  MissionOutcomeProcessor ─► Loyalty shifts from choices    │
│  DifficultyCalculator ─► Squad power includes loyalty      │
└──────────────────────────────────────────────────────────┘
```

## Prerequisites

- **Phase 1** — UnitRosterState, UnitData, GameConstants
- **Phase 3** — StrategicLayerManager (for healing time actions)
- **Phase 4** — MissionOutcomeProcessor (for loyalty shifts)

## Existing Code This Phase Extends

| File | What It Has |
|------|-------------|
| `UnitData.cs` | id, displayName, level, xp, status, loadoutID, isVIP |
| `UnitRosterState.cs` | Roster CRUD, deployment, XP/leveling, status updates |
| `UnitStatus.cs` | Active, Injured, Captured, Missing, Dead |
| `GameConstants.cs` | MAX_SQUAD_SIZE_1 through _5, XP scaling |

## File Plan

| File | Creates |
|------|---------|
| `01_unit_loyalty.md` | Loyalty score, thresholds, shift mechanics |
| `02_injury_recovery.md` | Recovery hours, strategic layer healing integration |
| `03_loadouts.md` | LoadoutData, equipment slots |
| `04_squad_upgrades.md` | Squad size progression, recruitment |
| `05_summary.md` | File checklist and integration points |
