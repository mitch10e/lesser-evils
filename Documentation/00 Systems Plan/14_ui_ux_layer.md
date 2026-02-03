# 14. UI / UX Layer

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
