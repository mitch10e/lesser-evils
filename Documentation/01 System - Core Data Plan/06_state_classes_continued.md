# Part 4 (Continued): More State Classes

## Step 4.3: TechState

Create `Assets/Scripts/Core/GameState/TechState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class TechState {

        public List<string> unlockedTechIDs;

        public string currentResearchID;

        public int currentResearchProgress;

        public TechState() {
            unlockedTechIDs = new List<string>();
            currentResearchID = null;
            currentResearchProgress = 0;
        }

        public bool isTechUnlocked(string techID) {
            return unlockedTechIDs.Contains(techID);
        }

        public bool isResearching() {
            return !string.IsNullOrEmpty(currentResearchID);
        }

        public bool arePrerequisitesMet(TechData tech) {
            foreach (var prerequisiteID in tech.prerequisiteTechIDs) {
                if (!isTechUnlocked(prerequisiteID)) {
                    return false;
                }
            }
            return true;
        }

        public bool startResearch(string techID) {
            if (isResearching()) return false;
            if (isTechUnlocked(techID)) return false;

            currentResearchID = techID;
            currentResearchProgress = 0;
            return true;
        }

        public bool advanceResearch(int requiredTime) {
            if (!isResearching()) return false;

            currentResearchProgress++;
            if (currentResearchProgress >= requiredTime) {
                completeCurrentResearch();
                return true;
            }

            return false;
        }

        public void completeCurrentResearch() {
            if (!isResearching()) return;

            unlockedTechIDs.Add(currentResearchID);
            currentResearchID = null;
            currentResearchProgress = 0;
        }

        public void cancelResearch() {
            currentResearchID = null;
            currentResearchProgress = 0;
        }

        public void reset() {
            unlockedTechIDs.Clear();
            currentResearchID = null;
            currentResearchProgress = 0;
        }

    }

}
```

**Design Note:** This is an XCOM-style research system: one tech at a time. The `TechState` tracks progress while `TechData` structs define what techs exist. Precursor access and extinction tech flags have been deferred to a future BranchState implementation.

## Future: BranchState (Not Yet Implemented)

BranchState will track major story branching flags:
- Loyalist vs Defector path
- Act 2 decision point
- Defeated faction
- Story flags for narrative tracking

This will be implemented when the narrative branching system is built.
