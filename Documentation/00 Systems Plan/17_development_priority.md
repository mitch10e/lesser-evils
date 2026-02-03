# Development Priority (Suggested Build Order)

1. **GameState & Save/Load** — foundation, nothing works without it
2. **Campaign Flow & Act Progression** — defines the skeleton of the game
3. **Mission Management** — core loop driver
4. **Combat Black Box (Test Harness)** — lets everything else be tested immediately
5. **Squad & Unit Management** — needed for mission inputs/outputs
6. **Resource Economy** — creates the supply pressure loop
7. **Faction & Loyalty Tracking** — needed before Act 2 branching
8. **Moral Tracking & Decision System** — needed for Act 2 decision gate
9. **Tech Progression** — gates precursor tech and endgame tools
10. **Mission Generation** — fills gaps between story missions
11. **AI Escalation** — polish layer on difficulty
12. **Event & Consequence System** — deferred feedback, polish layer
13. **UI/UX Layer** — built incrementally alongside each system, fully polished last
