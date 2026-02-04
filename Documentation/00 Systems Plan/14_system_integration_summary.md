# System Integration Summary

| System | Reads From | Writes To |
|---|---|---|
| GameState | — | All systems |
| Save/Load | GameState | Disk |
| Campaign Flow | GameState, Mission Management | GameState (act flags) |
| Mission Management | Campaign Flow, Mission Gen, Combat BB | GameState (progress, rewards) |
| Mission Generation | GameState (act, resources) | Mission Management (pool) |
| Squad Management | GameState, Combat BB output | GameState (roster) |
| Resource Economy | Mission Management (rewards/costs) | GameState (resources) |
| Faction Tracking | Mission Management, Decision System | GameState (faction status) |
| Moral Tracking | Decision System, Mission choices | GameState (moral meter) |
| Tech Progression | Mission Management, Resource Economy | GameState (unlocks) |
| AI Escalation | Moral Tracking, GameState (act) | Combat BB (difficulty input) |
| Decision System | UI input, Mission Management | GameState (branch flags) |
| Combat Black Box | Mission Mgmt, Squad Mgmt, Tech, AI Escalation | Mission Management (outcome) |
| UI/UX | GameState (read-only) | Decision System (player input) |
| Event/Consequence | GameState (choice history) | GameState (deferred effects) |
