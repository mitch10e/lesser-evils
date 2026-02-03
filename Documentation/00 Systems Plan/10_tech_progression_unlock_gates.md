# 10. Tech Progression & Unlock Gates

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
