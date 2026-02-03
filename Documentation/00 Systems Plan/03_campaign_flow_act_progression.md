# 3. Campaign Flow & Progression

**Purpose:** Controls the pacing and difficulty curve of the campaign through organic progression rather than explicit act gates. The world evolves based on player actions and time, creating natural pressure to advance.

**Core Design Principle:** The player never sees "Act 2 Begins" or similar notifications. Instead, the story unfolds and difficulty scales based on two factors:
1. **Main Story Missions Completed** - Drives narrative progression and unlocks
2. **Time Elapsed** - The world doesn't wait; enemies grow stronger over time

**Responsibilities:**
- Tracks campaign progress via `storyProgress` (0.0 to 1.0) derived from completed main missions
- Tracks `worldThreatLevel` that increases with time, independent of player progress
- Calculates `difficultyRatio` = worldThreatLevel / playerPower to determine if player is keeping pace
- Unlocks content (missions, tech, story beats) based on storyProgress thresholds
- Scales enemy strength based on worldThreatLevel

**Progression Pressure:**
| Scenario | Effect |
|---|---|
| Player ahead of curve | Enemies feel manageable, breathing room for side content |
| Player on pace | Balanced challenge, intended experience |
| Player falling behind | Enemies outpace growth, missions become harder, creates urgency |

**Story Progress Thresholds:**
| Progress | Unlocks |
|---|---|
| 0.0 - 0.25 | Opening missions, basic tech tree |
| 0.25 - 0.50 | Mid-game factions active, advanced tech available |
| 0.50 - 0.75 | Late-game content, faction climax missions appear |
| 0.75 - 1.0 | Endgame, final confrontations |

**Key Decision Points:**
- Major story choices still exist but are woven into mission outcomes, not explicit "choose now" gates
- The loyalist/defector path emerges from accumulated choices rather than a single decision
