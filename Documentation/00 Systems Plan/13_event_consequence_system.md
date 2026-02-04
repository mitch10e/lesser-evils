# 13. Event & Consequence System

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
