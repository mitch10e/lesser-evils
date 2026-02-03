# Part 4 (Continued): Campaign and Unit States

## Step 4.4: CampaignState

Create `Assets/Scripts/Core/GameState/CampaignState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class CampaignState {

        public ActType currentAct;

        public int elapsedTime;

        public FactionType startingFaction;

        public FactionType currentFaction;

        public List<string> activeStoryFlags;

        public CampaignState() {
            currentAct = ActType.Act1;
            startingFaction = FactionType.None;
            currentFaction = FactionType.None;
            activeStoryFlags = new List<string>();
            elapsedTime = 0;
        }

        public void setAct(ActType type) {
            this.currentAct = type;
        }

        public void setFaction(FactionType type) {
            this.currentFaction = type;
        }

        public void setStartingFaction(FactionType type) {
            this.startingFaction = type;
        }

        public void addStoryFlag(string flag) {
            if (!activeStoryFlags.Contains(flag)) {
                activeStoryFlags.Add(flag);
            }
        }

        public bool hasStoryFlag(string flag) {
            return activeStoryFlags.Contains(flag);
        }

        public void removeStoryFlag(string flag) {
            activeStoryFlags.Remove(flag);
        }

        public void passTime(int duration) {
            elapsedTime += duration;
        }

        public void reset() {
            currentAct = ActType.Act1;
            startingFaction = FactionType.None;
            currentFaction = FactionType.None;
            activeStoryFlags.Clear();
            elapsedTime = 0;
        }

    }

}
```

**Design Note:** CampaignState tracks both starting faction (for narrative purposes) and current faction (which can change if the player defects). Story flags are generic strings that can be used for any narrative tracking. Mission tracking has been simplified - detailed mission history will be added in a later phase.

## Step 4.5: UnitRosterState

Create `Assets/Scripts/Core/GameState/UnitRosterState.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class UnitRosterState {

        public List<UnitData> roster;

        public int maxSquadSize;

        public List<string> deployedUnitIDs;

        public UnitRosterState() {
            roster = new List<UnitData>();
            maxSquadSize = GameConstants.MAX_SQUAD_SIZE_1;
            deployedUnitIDs = new List<string>();
        }

        public UnitData? get(string unitID) {
            int index = indexOf(unitID);
            return index >= 0 ? roster[index] : null;
        }

        public List<UnitData> get(UnitStatus status) {
            return roster.Where(u => u.status == status).ToList();
        }

        public List<UnitData> getActiveUnits() {
            return get(UnitStatus.Active);
        }

        public List<UnitData> getInjuredUnits() {
            return get(UnitStatus.Injured);
        }

        public List<UnitData> getDeadUnits() {
            return get(UnitStatus.Dead);
        }

        public List<UnitData> getMissingUnits() {
            return get(UnitStatus.Missing);
        }

        public List<UnitData> getCapturedUnits() {
            return get(UnitStatus.Captured);
        }

        public void add(UnitData unit) {
            if (roster.Any(u => u.id == unit.id)) return;
            roster.Add(unit);
        }

        public void remove(string unitID) {
            roster.RemoveAll(u => u.id == unitID);
            deployedUnitIDs.Remove(unitID);
        }

        public void updateStatus(string unitID, UnitStatus status) {
            int index = indexOf(unitID);
            if (index < 0) return;

            UnitData unit = roster[index];
            unit.status = status;
            roster[index] = unit;
        }

        public void addExperience(string unitID, int xp) {
            int index = indexOf(unitID);
            if (index < 0) return;

            UnitData unit = roster[index];
            unit.experience += xp;

            while (
                unit.experience >= unit.experienceToNextLevel
                && unit.level < GameConstants.MAX_UNIT_LEVEL
            ) {
                unit.experience -= unit.experienceToNextLevel;
                unit.level++;
                unit.experienceToNextLevel = calculateXPForLevel(unit.level + 1);
            }

            roster[index] = unit;
        }

        public bool canDeploy(string unitID) {
            UnitData? unit = get(unitID);
            return unit.HasValue
            && unit.Value.status == UnitStatus.Active
            && !deployedUnitIDs.Contains(unitID);
        }

        public bool deploy(string unitID) {
            if (!canDeploy(unitID)) return false;
            if (deployedUnitIDs.Count >= maxSquadSize) return false;

            deployedUnitIDs.Add(unitID);
            return true;
        }

        public void undeploy(string unitID) {
            deployedUnitIDs.Remove(unitID);
        }

        public void undeployAll() {
            deployedUnitIDs.Clear();
        }

        public void reset() {
            roster.Clear();
            maxSquadSize = GameConstants.MAX_SQUAD_SIZE_1;
            deployedUnitIDs.Clear();
        }

        // MARK: - Private Methods

        private int indexOf(string unitID) {
            return roster.FindIndex(u => u.id == unitID);
        }

        private int calculateXPForLevel(int level) {
            return (int)(GameConstants.BASE_XP_TO_LEVEL * Math.Pow(GameConstants.XP_SCALING_PER_LEVEL, level - 1));
        }

    }

}
```

**Design Note:**
- UnitRosterState (renamed from SquadState) manages all units the player has recruited
- `deployedUnitIDs` tracks which units are currently assigned to a mission
- Experience scaling uses an exponential formula for progression
- Units are stored as structs, so modifications require re-assignment to the list
- Injury recovery has been simplified - will be expanded in a later phase

**Checkpoint:** You should now have 5 state class files in `Assets/Scripts/Core/GameState/`. Compile in Unity and fix any errors before continuing.
