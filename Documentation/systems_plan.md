# Unity Tactical RPG — Systems Development Plan

This document outlines every major system required to build the game, organized by priority and dependency. Combat is intentionally abstracted as a **black-box test harness** so development can focus on campaign flow, branching narrative, resource economy, and meta-layer decisions first.

---

## System Overview (Dependency Order)

```
Core Data & State Management
        │
        ▼
Save / Load System
        │
        ▼
Campaign Flow & Act Progression
        │
        ▼
Mission Management ◄──────── Mission Generation (Generic/Random)
        │
        ▼
Squad & Unit Management
        │
        ▼
Resource & Supply Economy
        │
        ▼
Faction & Loyalty Tracking
        │
        ▼
Moral / Ethics Tracking
        │
        ▼
Tech Progression & Unlock Gates
        │
        ▼
AI Escalation & Difficulty Scaling
        │
        ▼
Decision & Branching System
        │
        ▼
Combat Black Box (Test Harness)
        │
        ▼
UI / UX Layer
        │
        ▼
Event & Consequence System
```

---

## 1. Core Data & State Management

**Purpose:** Single source of truth for all game state. Every other system reads from or writes to this.

**Responsibilities:**
- Holds all persistent game data: current act, mission progress, squad state, resources, moral meter, faction loyalty, tech unlocks, and branching flags.
- Provides a clean API so systems don't depend on each other directly — they all talk through this manager.
- Serializable for save/load.

**Key Data Structures:**
- `GameState` — top-level container
- `CampaignState` — current act, current mission index, completed missions, available missions
- `SquadState` — unit roster, levels, status (alive/injured/missing)
- `ResourceState` — all currency and material counts
- `FactionState` — loyalty values, hostility flags per faction
- `MoralState` — moral meter value, choice history log
- `TechState` — unlocked techs, research progress, precursor access flag
- `BranchState` — defection flag, loyalist/defector path active, which factions are defeated

---

## 2. Save / Load System

**Purpose:** Persist full game state between sessions.

**Responsibilities:**
- Serializes `GameState` to disk (JSON or binary).
- Supports multiple save slots.
- Validates save integrity on load (version checks for future patches).
- Hooks into `GameState` — no system-specific logic lives here.

---

## 3. Campaign Flow & Act Progression

**Purpose:** Controls the top-level pacing of the game across three acts. Determines what is available to the player at any point and when act transitions trigger.

**Responsibilities:**
- Tracks which act the player is in (Act 1 / Act 2 / Act 3) and which path (Loyalist or Defector) is active.
- Defines the **gate conditions** for advancing between acts (e.g., required story missions completed, minimum resources met, branching decision made).
- Triggers act-transition events (narrative feedback, UI changes, new mission pools unlocking).
- Manages the **Act 2 → Act 3 decision point** as a hard gate — the game does not proceed until the player makes their loyalist/defector choice.

**Act Transition Gates:**
| Transition | Gate Conditions |
|---|---|
| Act 1 → Act 2 | All required Act 1 story missions completed. Faction climax mission finished. |
| Act 2 → Act 3 | Target enemy faction defeated/crippled. Precursor tech mission completed. Player makes loyalist or defector choice. |
| Act 3 → Ending | All Act 3 objectives resolved based on active path. |

---

## 4. Mission Management

**Purpose:** Core system for presenting, tracking, and resolving individual missions. Separates *story missions* (scripted, ordered) from *generic missions* (pool-based, optional).

**Responsibilities:**
- Maintains two mission pools:
  - **Story Mission Pool** — ordered, act-gated, some mandatory, some optional. Defined in data (scriptable objects or JSON).
  - **Generic Mission Pool** — semi-random, available between story missions, scales with current act.
- Determines which missions are currently available to the player based on act, completed missions, branch state, and faction hostility.
- Tracks mission status: `Available`, `Active`, `Completed`, `Failed`, `Locked`.
- Feeds selected mission into the **Combat Black Box** and receives the outcome.
- Applies mission outcomes (rewards, consequences, narrative flags) back to `GameState`.

**Mission Data Schema (per mission):**
- `ID`, `Type` (Story / Generic), `Act`, `MissionCategory` (Defense, Sabotage, Assassination, Raid, Recon, etc.)
- `PrimaryObjectives[]`, `OptionalObjectives[]`
- `MoralChoices[]` — defined decision points within the mission and their consequence flags
- `Rewards` — resources, tech unlock flags, squad XP
- `Consequences` — moral meter changes, faction loyalty shifts, narrative flags
- `Prerequisites[]` — other missions or conditions that must be met first
- `BranchRestrictions` — which path(s) this mission is available on (e.g., Defector-only Act 3 missions)

---

## 5. Mission Generation (Generic / Supply Missions)

**Purpose:** Populates the optional mission pool between story missions with procedurally selected (or lightly randomized) generic missions. Mimics the XCOM-style "campaign grind" of managing logistics alongside the main objective.

**Responsibilities:**
- Maintains a master list of generic mission templates (Supply Convoy Raid, Resource Salvage, Rescue/Evacuation, Patrol Defense, Recon, Tech Recovery, etc.).
- Selects and presents a rotating pool of generic missions based on:
  - Current act (controls scale and reward magnitude)
  - Player resource levels (prevents over-flooding with supply missions if player is well-stocked)
  - Time / mission cadence (limits how many are available at once so the player has to prioritize)
- Each generated mission is instantiated from a template with minor variation (enemy count, optional objectives, reward amounts).
- Generic missions feed into the same Mission Management pipeline as story missions.

**Scaling Rules:**
| Act | Generic Mission Scale | Reward Tier | Moral Weight |
|---|---|---|---|
| Act 1 | Small squads, stealth-friendly | Low–Medium | Subtle (minor civilian/collateral flags) |
| Act 2 | Medium squads, mixed objectives | Medium–High | Moderate (compounds with story choices) |
| Act 3 | Large squads, high-value targets | High | High (directly feeds endgame operations) |

---

## 6. Squad & Unit Management

**Purpose:** Tracks the player's team composition, progression, and status across the campaign.

**Responsibilities:**
- Manages a roster of units (squad slots, available/injured/dead status).
- Tracks per-unit: level, experience, equipped loadout, injury status, loyalty score.
- **Loyalty** is a per-unit value that affects whether the unit will follow orders during high-moral-cost missions or during the defection event. Low loyalty units may refuse orders or defect on their own.
- Squad size scales with act progression (Act 1: 2–3 units; Act 2: 4–5; Act 3: 5–6).
- Injury/recovery system: units injured in missions require downtime before they can deploy again. This creates resource pressure and forces the player to manage risk.
- Feeds squad composition into the Combat Black Box as an input, receives casualties/injuries as output.

**Unit Data:**
- `ID`, `Level`, `XP`, `EquippedLoadout`, `Status` (Active/Injured/Dead), `LoyaltyScore`
- Loyalty is affected by: moral choices made during missions, act transitions, and cumulative moral meter value.

---

## 7. Resource & Supply Economy

**Purpose:** Manages all currencies and materials. Creates the risk/reward tension that drives mission selection and planning.

**Responsibilities:**
- Tracks multiple resource types (e.g., general currency, tech components, medical supplies, ammunition/fuel, orbital intel points).
- Resources are consumed by: deploying squads, upgrading units, unlocking tech, and certain mission prerequisites.
- Resources are earned by: completing missions (story and generic), optional objectives, and efficient mission outcomes.
- Resource scarcity is a key lever — especially on the Defector path in Act 3, where access to resources is deliberately constrained.

**Resource Types (generic, names TBD):**
| Resource | Use |
|---|---|
| General Currency | Unit upgrades, loadout purchases |
| Tech Components | Research / tech unlock prerequisites |
| Medical Supplies | Unit injury recovery |
| Fuel / Logistics | Mission deployment cost (scales with mission size) |
| Intel Points | Unlocks optional objectives, mission reconnaissance |

---

## 8. Faction & Loyalty Tracking

**Purpose:** Tracks the player's relationship with each faction and the hostility state of enemy factions. Drives mission availability, branching, and narrative tone.

**Responsibilities:**
- Tracks a **loyalty value** with the player's own faction (builds during Act 1–2, becomes irrelevant or inverted on Defector path).
- Tracks **hostility levels** for each enemy faction — which one the player is actively targeting, which one is escalating in the background.
- Hostility affects: enemy aggression in generic missions, which factions appear as threats, and narrative pacing of background extinction tech advancement.
- On the **Defector path**, the player's own former faction becomes hostile — this flips their loyalty tracking and opens new mission types.
- Faction defeat/cripple status is a persistent flag that affects Act 3 structure.

**Faction State Flags:**
- `PlayerFaction` — which faction the player chose at start
- `Faction1Status`, `Faction2Status`, `Faction3Status` — each can be: `Active`, `Targeted`, `Defeated`, `Escalating`
- `PlayerIsDefector` — boolean, set at Act 2 decision point

---

## 9. Moral / Ethics Tracking

**Purpose:** Quantifies the cumulative weight of the player's choices and drives narrative consequences, AI escalation, and ending determination.

**Responsibilities:**
- Maintains a **moral meter** — a numerical value that shifts based on choices made during missions (both story and generic).
- Moral choices are binary or multi-option per mission: ethical (slower, riskier, preserves civilians/environment) vs. efficient (faster, higher moral cost).
- The meter does not directly gate anything in early game — it operates subtly. Its primary effects are:
  - Influences **squad loyalty scores** (high moral cost lowers unit loyalty over time)
  - Feeds into **AI escalation** (see System 11)
  - Affects **ending tone and outcome details** (how much damage has been done, how many civilians survived, etc.)
  - Determines whether the Defector path ending is "heroic" or "pyrrhic" based on cumulative choices
- Moral choice history is logged for use by the Event & Consequence System.

---

## 10. Tech Progression & Unlock Gates

**Purpose:** Controls what technology and abilities the player has access to, tied to act progression and mission completion.

**Responsibilities:**
- Maintains a tech tree (or linear unlock chain, depending on final design) with gated unlock points.
- Techs are unlocked by: completing specific story missions, acquiring tech components (resources), and act progression.
- Key tech gates:
  - **Act 1 Climax:** Player completes a mission that unlocks a critical component — this is flagged internally as contributing to the faction's extinction tech, but is not surfaced to the player.
  - **Act 2 Climax:** Player gains access to **precursor extinction tech** — a limited, mission-specific tool that represents the first real taste of endgame destructive power.
  - **Act 3 (Loyalist):** Full extinction tech unlocked and available for endgame operations.
  - **Act 3 (Defector):** Precursor tech is relinquished. Player must find alternative approaches and may recover partial tech through specific missions.
- Tech unlocks feed into the Combat Black Box as **available tools/options** for the mission.

---

## 11. AI Escalation & Difficulty Scaling

**Purpose:** Makes enemy behavior and mission difficulty adapt to player behavior over the campaign. Rewards careful, ethical play indirectly and punishes reckless efficiency by escalating enemy capability.

**Responsibilities:**
- Tracks a cumulative **aggression/escalation index** that grows based on:
  - Player's efficiency choices (high-collateral missions raise it faster)
  - Player's moral meter (low moral meter = enemies have learned ruthlessness)
  - Act progression (enemies naturally escalate as acts advance)
  - Defection event (sharp spike — former allies become adaptive antagonists)
- Escalation index feeds into the Combat Black Box as a **difficulty parameter**, affecting:
  - Enemy unit count and quality
  - Enemy use of precursor/extinction tech in later acts
  - Tactical complexity of missions
- On the **Defector path Act 3**, former allies use intelligent, adaptive tactics informed by the player's own past behavior — this is a narrative and mechanical callback.

---

## 12. Decision & Branching System

**Purpose:** Manages all player-facing decision points and ensures the correct narrative and mechanical paths are activated.

**Responsibilities:**
- Identifies and presents decision points to the player at defined moments (in-mission moral choices, the Act 2 defection gate, Act 3 sub-choices).
- Records decisions into `BranchState` and `MoralState`.
- Activates or locks downstream systems based on decisions:
  - Loyalist path → full tech access, Act 3 Loyalist mission pool, defector NPCs become enemies
  - Defector path → tech relinquished, Act 3 Defector mission pool, former faction becomes hostile
- Handles the **Act 2 climax decision** as a special cinematic gate — this is the primary branching point of the game and should be clearly flagged in UI.
- Small in-mission choices (ethical vs. efficient) are handled as flags that feed into Moral and Consequence systems without blocking progression.

---

## 13. Combat Black Box (Test Harness)

**Purpose:** Abstracts all combat resolution into a single input/output interface. For now, this is a **developer/designer test tool** that lets the team manually define mission outcomes so the rest of the systems can be built and tested without a finished combat system.

**Inputs (fed by Mission Management + Squad Management + Tech Progression + AI Escalation):**
- Mission type and objectives
- Player squad composition (unit count, levels, loadouts)
- Available tech/tools for this mission
- Enemy difficulty/escalation index
- Moral choice options available in this mission

**Outputs (returned to Mission Management):**
- Mission outcome: `Success` / `Partial Success` / `Failure`
- Casualties/injuries to player squad (unit IDs affected)
- Which optional objectives were completed
- Which moral choice was selected (if applicable)
- Resource costs consumed during the mission

**Test Harness Interface (Dev Tool):**
- Simple UI panel that allows the developer to manually set:
  - Mission outcome (Success / Partial / Failure)
  - Number of casualties / which units
  - Which optional objectives to mark complete
  - Which moral choice to simulate
- Outputs are then injected back into the pipeline exactly as if a real combat system had produced them.
- This lets the entire campaign flow, branching, economy, and narrative systems be tested end-to-end without combat being implemented.

**Future Integration Note:**
When a real combat system is built, it replaces the test harness internals but conforms to the same input/output interface. No other system needs to change.

---

## 14. UI / UX Layer

**Purpose:** All player-facing screens and panels. Reads from `GameState` and presents information clearly.

**Screens / Panels:**
- **Campaign Map / Mission Select** — shows available story and generic missions, current act indicator, faction status indicators
- **Squad Management** — view/equip units, see injury status, loyalty levels
- **Resource Dashboard** — current resource counts, consumption forecasts
- **Tech Panel** — unlocked techs, locked techs with requirements shown
- **Pre-Mission Briefing** — objectives, risks, squad selection, available moral choices previewed (without revealing consequences)
- **Post-Mission Summary** — outcome, casualties, rewards, moral consequence feedback (subtle in early acts, more explicit later)
- **Act Transition Screens** — narrative feedback, consequence summaries at act boundaries
- **Act 2 Decision Screen** — the loyalist/defector choice. This is the most important single UI moment in the game. Presented clearly with weight but without telegraphing the "correct" answer.
- **Combat Black Box Test Panel** — dev-only overlay for setting outcomes during testing

---

## 15. Event & Consequence System

**Purpose:** Manages deferred and compounding consequences from earlier player choices. Ensures that decisions made in Act 1 and Act 2 are felt in Act 2 and Act 3 — not just immediately, but over time.

**Responsibilities:**
- Maintains an **event queue** of pending consequences tied to past choices.
- Consequences can be:
  - **Immediate** — applied right after a mission (resource change, moral meter shift)
  - **Deferred** — triggered at a later act or mission gate (e.g., a faction escalates because the player destroyed their supply line two missions ago; a unit's loyalty drops because of accumulated moral cost)
  - **Narrative** — surface-level feedback like mission report text, squad dialogue logs, or status updates that hint at consequences without being explicit
- Deferred consequences are stored in `GameState` and checked at act transitions and mission availability gates.
- This system is what creates the feeling that the world reacts to the player over time, rather than only in the immediate moment.

**Examples:**
| Player Action | Deferred Consequence | When It Triggers |
|---|---|---|
| High-collateral mission in Act 1 | Enemy faction escalation index increases | Reflected in Act 2 generic mission difficulty |
| Unlocked critical tech component (Act 1 climax) | Faction extinction tech advances one stage | Revealed partially in Act 2, fully at Act 2 decision |
| Chose efficient path repeatedly | Squad loyalty scores drop | Checked at Act 2 decision — low-loyalty units may defect regardless of player choice |
| Defected in Act 2 | Former faction retaliates with adaptive tactics | Act 3 Defector missions feature intelligent former allies |

---

## System Integration Summary

| System | Reads From | Writes To |
|---|---|---|
| GameState | — | All systems |
| Save/Load | GameState | Disk |
| Campaign Flow | GameState, Mission Management | GameState (act flags) |
| Mission Management | Campaign Flow, Mission Gen, Combat BB | GameState (progress, rewards) |
| Mission Generation | GameState (act, resources) | Mission Management (pool) |
| Squad Management | GameState, Combat BB output | GameState (roster) |
| Resource Economy | Mission Management (rewards/costs) | GameState (resources) |
| Faction Tracking | Mission Management, Decision System | GameState (faction status) |
| Moral Tracking | Decision System, Mission choices | GameState (moral meter) |
| Tech Progression | Mission Management, Resource Economy | GameState (unlocks) |
| AI Escalation | Moral Tracking, GameState (act) | Combat BB (difficulty input) |
| Decision System | UI input, Mission Management | GameState (branch flags) |
| Combat Black Box | Mission Mgmt, Squad Mgmt, Tech, AI Escalation | Mission Management (outcome) |
| UI/UX | GameState (read-only) | Decision System (player input) |
| Event/Consequence | GameState (choice history) | GameState (deferred effects) |

---

## Development Priority (Suggested Build Order)

1. **GameState & Save/Load** — foundation, nothing works without it
2. **Campaign Flow & Act Progression** — defines the skeleton of the game
3. **Mission Management** — core loop driver
4. **Combat Black Box (Test Harness)** — lets everything else be tested immediately
5. **Squad & Unit Management** — needed for mission inputs/outputs
6. **Resource Economy** — creates the supply pressure loop
7. **Faction & Loyalty Tracking** — needed before Act 2 branching
8. **Moral Tracking & Decision System** — needed for Act 2 decision gate
9. **Tech Progression** — gates precursor tech and endgame tools
10. **Mission Generation** — fills gaps between story missions
11. **AI Escalation** — polish layer on difficulty
12. **Event & Consequence System** — deferred feedback, polish layer
13. **UI/UX Layer** — built incrementally alongside each system, fully polished last
