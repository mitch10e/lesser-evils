using System;
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Missions {

    public class MissionPool {

        private List<MissionData> allMissions = new();
        private Dictionary<string, MissionStatus> missionStatuses = new();

        public void registerMission(MissionData mission) {
            allMissions.Add(mission);
            missionStatuses[mission.id] = MissionStatus.Locked;
        }

        public void registerMissions(List<MissionData> missions) {
            foreach (var mission in missions) {
                registerMission(mission);
            }
        }

        public MissionStatus getStatus(string missionID) {
            return missionStatuses.TryGetValue(missionID, out var status)
                ? status
                : MissionStatus.Locked;
        }

        public void setStatus(string missionID, MissionStatus status) {
            if (missionStatuses.ContainsKey(missionID)) {
                missionStatuses[missionID] = status;
            }
        }

        public MissionData getMission(string missionID) {
            foreach (var mission in allMissions) {
                if (mission.id == missionID) return mission;
            }

            return null;
        }

        public List<MissionData> getMissionsByStatus(MissionStatus status) {
            List<MissionData> result = new();

            foreach (var mission in allMissions) {
                if (getStatus(mission.id) == status) {
                    result.Add(mission);
                }
            }

            return result;
        }

        public List<MissionData> getMissionsByType(MissionType type) {
            List<MissionData> result = new();

            foreach (var mission in allMissions) {
                if (mission.missionType == type) {
                    result.Add(mission);
                }
            }

            return result;
        }

        public List<MissionData> getAvailableMissions() {
            return getMissionsByStatus(MissionStatus.Unlocked);
        }

        public List<MissionData> getCompletedMissions() {
            return getMissionsByStatus(MissionStatus.Completed);
        }

    }

}
