# 2. Save / Load System

**Purpose:** Persist full game state between sessions.

**Responsibilities:**
- Serializes `GameState` to disk (JSON or binary).
- Supports multiple save files.
- Validates save integrity on load (version checks for future patches).
- Hooks into `GameState` — no system-specific logic lives here.
