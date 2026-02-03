# 5. Mission Generation (Generic / Supply Missions)

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
