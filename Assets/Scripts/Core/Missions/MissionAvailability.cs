using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Missions {

    public static class MissionAvailability {

        public static bool canUnlock(
            MissionData mission,
            MissionPool pool,
            ProgressionState progression,
            CampaignState campaign
        ) {
            var status = pool.getStatus(mission.id);
            if (status != MissionStatus.Locked) return false;

            foreach (var prerequisiteID in mission.prerequisiteMissionIDs) {
                if (pool.getStatus(prerequisiteID) != MissionStatus.Completed) {
                    return false;
                }
            }

            foreach (var flag in mission.requiredStoryFlags) {
                if (!campaign.activeStoryFlags.Contains(flag)) {
                    return false;
                }
            }

            if (
                mission.factionRestriction != FactionType.None
                && mission.factionRestriction != campaign.currentFaction
            ) {
                return false;
            }

            return true;
        }

        public static List<MissionData> findUnlockable(
            MissionPool pool,
            ProgressionState progression,
            CampaignState campaign
        ) {
            List<MissionData> unlockable = new();

            foreach (var mission in pool.getMissionsByStatus(MissionStatus.Locked)) {
                if (canUnlock(mission, pool, progression, campaign)) {
                    unlockable.Add(mission);
                }
            }

            return unlockable;
        }

    }

}
