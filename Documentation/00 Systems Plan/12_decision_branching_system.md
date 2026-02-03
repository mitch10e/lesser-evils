# 12. Decision & Branching System

**Purpose:** Manages all player-facing decision points and ensures the correct narrative and mechanical paths are activated.

**Responsibilities:**
- Identifies and presents decision points to the player at defined moments (in-mission moral choices, the Act 2 defection gate, Act 3 sub-choices).
- Records decisions into `BranchState` and `MoralState`.
- Activates or locks downstream systems based on decisions:
  - Loyalist path → full tech access, Act 3 Loyalist mission pool, defector NPCs become enemies
  - Defector path → tech relinquished, Act 3 Defector mission pool, former faction becomes hostile
- Handles the **Act 2 climax decision** as a special cinematic gate — this is the primary branching point of the game and should be clearly flagged in UI.
- Small in-mission choices (ethical vs. efficient) are handled as flags that feed into Moral and Consequence systems without blocking progression.
