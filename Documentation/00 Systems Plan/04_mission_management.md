# 4. Mission Management

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
