using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class ContentUnlocker {

        public static bool canUnlockMission(
            MainMissionData mission,
            ProgressionState progression
        ) {
            foreach (var prerequisite in mission.prerequisiteMissionIDs) {
                if (!progression.hasCompletedMission(prerequisite)) {
                    return false;
                }
            }

            if (progression.hasCompletedMission(mission.id)) {
                return false;
            }

            return true;
        }

        public static List<string> getNewlyAvailableMissions(
            List<MainMissionData> allMissions,
            ProgressionState progression
        ) {
            List<string> newMissions = new();

            foreach (var mission in allMissions) {
                if (canUnlockMission(mission, progression) && !progression.isMissionAvailable(mission.id)) {
                    newMissions.Add(mission.id);
                }
            }

            return newMissions;
        }

        public static float getFactionThreatLevel(
            FactionType faction,
            ProgressionState progression
        ) {
            return progression.worldThreatLevel;
        }

    }

}
