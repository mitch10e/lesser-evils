# 6. Squad & Unit Management

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
