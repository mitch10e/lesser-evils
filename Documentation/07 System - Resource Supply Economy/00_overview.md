# Phase 7: Resource & Supply Economy - Development Tutorial

Phase 1 built `ResourceState` and `MaterialState` with basic add/spend/get operations. This phase turns resources into a meaningful decision-making layer — missions cost fuel to deploy, injured units need medical supplies to heal faster, tech research consumes components, and scarcity forces the player to prioritize.

---

## What You'll Build

- **Expanded resource types** — Add MedicalSupplies and Fuel alongside existing Currency, Alloys, TechComponents, Intel
- **Deployment costs** — Missions cost resources to launch (fuel, ammo), scaling with squad size
- **Action costs** — Strategic layer actions (research, build, heal) consume specific resources
- **Cost validation** — Check affordability before allowing actions
- **Economy events** — Notify UI when resources change, warn on low supplies

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                   RESOURCE TYPES                          │
│                                                           │
│  Currency ─── Unit upgrades, loadout purchases            │
│  Alloys ───── Construction, equipment crafting            │
│  TechComp ─── Research prerequisites                      │
│  Intel ────── Mission recon, optional objective reveal     │
│  MedSupply ── Faster injury recovery (optional)           │
│  Fuel ─────── Mission deployment cost                     │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                   COST SYSTEM                             │
│                                                           │
│  DeploymentCost ─► Fuel + supplies per mission            │
│  ActionCost ─────► Resources per strategic action         │
│  CostValidator ──► Can the player afford this?            │
└──────────────────────┬────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                  INTEGRATION                              │
│                                                           │
│  MissionManager ─► Deducts deployment cost on start       │
│  StrategicLayer ─► Deducts action cost on queue           │
│  MissionOutcome ─► Adds resource rewards on completion    │
│  EconomyEvents ──► UI notifications for changes/warnings  │
└──────────────────────────────────────────────────────────┘
```

## Prerequisites

- **Phase 1** — ResourceState, MaterialState, ResourceType
- **Phase 3** — StrategicLayerManager (action costs)
- **Phase 4** — MissionManager, MissionOutcomeProcessor (deployment costs, rewards)

## Existing Code This Phase Extends

| File | What It Has |
|------|-------------|
| `ResourceState.cs` | Dictionary-based add/spend/get/hasResource |
| `MaterialState.cs` | Parallel system (placeholder types) |
| `ResourceType.cs` | Currency, Alloys, TechComponents, Intel |

## File Plan

| File | Creates |
|------|---------|
| `01_resource_types.md` | Expanded ResourceType enum, resource role definitions |
| `02_cost_system.md` | ActionCost struct, CostValidator, deployment costs |
| `03_economy_events.md` | Resource change events, scarcity warnings, UI integration |
| `04_summary.md` | File checklist and integration points |
