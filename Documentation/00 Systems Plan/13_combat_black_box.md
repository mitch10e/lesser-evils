# 13. Combat Black Box (Test Harness)

**Purpose:** Abstracts all combat resolution into a single input/output interface. For now, this is a **developer/designer test tool** that lets the team manually define mission outcomes so the rest of the systems can be built and tested without a finished combat system.

**Inputs (fed by Mission Management + Squad Management + Tech Progression + AI Escalation):**
- Mission type and objectives
- Player squad composition (unit count, levels, loadouts)
- Available tech/tools for this mission
- Enemy difficulty/escalation index
- Moral choice options available in this mission

**Outputs (returned to Mission Management):**
- Mission outcome: `Success` / `Partial Success` / `Failure`
- Casualties/injuries to player squad (unit IDs affected)
- Which optional objectives were completed
- Which moral choice was selected (if applicable)
- Resource costs consumed during the mission

**Test Harness Interface (Dev Tool):**
- Simple UI panel that allows the developer to manually set:
  - Mission outcome (Success / Partial / Failure)
  - Number of casualties / which units
  - Which optional objectives to mark complete
  - Which moral choice to simulate
- Outputs are then injected back into the pipeline exactly as if a real combat system had produced them.
- This lets the entire campaign flow, branching, economy, and narrative systems be tested end-to-end without combat being implemented.

**Future Integration Note:**
When a real combat system is built, it replaces the test harness internals but conforms to the same input/output interface. No other system needs to change.
