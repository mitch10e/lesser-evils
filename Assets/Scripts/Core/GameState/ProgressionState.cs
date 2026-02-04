using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class ProgressionState {

        public float worldThreatLevel;

        public List<string> completedMainMissionIDs;

        public List<string> availableMainMissionIDs;

        public int totalElapsedHours;

        public ProgressionState() {
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL;
            completedMainMissionIDs = new();
            availableMainMissionIDs = new();
            totalElapsedHours = 0;
        }

        // MARK: - Story Progress

        public void completeMainMission(string missionID) {
            if (hasCompletedMission(missionID)) return;

            completedMainMissionIDs.Add(missionID);
            availableMainMissionIDs.Remove(missionID);
        }

        public bool hasCompletedMission(string missionID) {
            return completedMainMissionIDs.Contains(missionID);
        }

        public bool isMissionAvailable(string missionID) {
            return availableMainMissionIDs.Contains(missionID);
        }

        public void unlockMission(string missionID) {
            if (hasCompletedMission(missionID)) return;
            if (isMissionAvailable(missionID)) return;

            availableMainMissionIDs.Add(missionID);
        }

        // MARK: - Time

        public void advanceTime(int hours) {
            totalElapsedHours += hours;
            updateWorldThreat();
        }

        public int getElapsedDays() {
            return totalElapsedHours / ProgressionConstants.HOURS_PER_DAY;
        }

        public int getElapsedWeeks() {
            return getElapsedDays() / ProgressionConstants.DAYS_PER_WEEK;
        }

        private void updateWorldThreat() {
            float days = getElapsedDays();
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL + (days * ProgressionConstants.DAILY_THREAT_GROWTH);
        }

        // MARK: - Reset

        public void reset() {
            worldThreatLevel = ProgressionConstants.BASE_THREAT_LEVEL;
            completedMainMissionIDs.Clear();
            availableMainMissionIDs.Clear();
            totalElapsedHours = 0;
        }

    }

}
