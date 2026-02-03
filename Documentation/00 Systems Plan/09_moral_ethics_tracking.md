# 9. Moral / Ethics Tracking

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
