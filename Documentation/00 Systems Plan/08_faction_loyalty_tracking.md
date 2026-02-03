# 8. Faction & Loyalty Tracking

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
