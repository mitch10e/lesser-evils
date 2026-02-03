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
