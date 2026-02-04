# Unity Tactical RPG — Systems Overview

This document outlines every major system required to build the game, organized by priority and dependency. Combat is intentionally abstracted as a **black-box test harness** so development can focus on campaign flow, branching narrative, resource economy, and meta-layer decisions first.

---

## System Dependency Order

```
Core Data & State Management
        │
        ▼
Save / Load System
        │
        ▼
Campaign Flow & Act Progression
        │
        ▼
Mission Management ◄──────── Mission Generation (Generic/Random)
        │
        ▼
Squad & Unit Management
        │
        ▼
Resource & Supply Economy
        │
        ▼
Tech Progression & Unlock Gates
        │
        ▼
AI Escalation & Difficulty Scaling
        │
        ▼
Decision & Branching System
        │
        ▼
Combat Black Box (Test Harness)
        │
        ▼
UI / UX Layer
        │
        ▼
Event & Consequence System
```
