# 9. AI Escalation & Difficulty Scaling

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
