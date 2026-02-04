using System;

namespace Game.Core.Data {

    [Serializable]
    public class MissionData {

        public string id;

        public string title;

        public string description;

        public MissionType missionType;

        public MissionCategory missionCategory;

        // MARK: - Requirements

        public string[] prerequisiteMissionIDs;

        public string[] requiredStoryFlags;

        public FactionType factionRestriction;

        // MARK: - Content

        public MissionObjectiveData[] primaryObjectives;

        public MissionObjectiveData[] optionalObjectives;

        // MARK: - Outcomes

        public MissionRewards rewards;

        public MissionConsequences consequences;

        // MARK: - Scaling

        public int baseEnemyCount;

        public int recommendedSquadLevel;

        // MARK: - Initialization

        public static MissionData CreateStory(
            string id,
            string title,
            string description,
            MissionCategory category,
            params string[] prerequisites
        ) {
            return new MissionData {
                id = id,
                title = title,
                description = description,
                missionType = MissionType.Story,
                missionCategory = category,
                prerequisiteMissionIDs = prerequisites ?? new string[0],
                requiredStoryFlags = new string[0],
                factionRestriction = FactionType.None,
                primaryObjectives = new MissionObjectiveData[0],
                optionalObjectives = new MissionObjectiveData[0],
                rewards = MissionRewards.Create(),
                consequences = MissionConsequences.Create(),
                baseEnemyCount = 6,
                recommendedSquadLevel = 1
            };
        }

        public static MissionData CreateGeneric(
            string id,
            string title,
            string description,
            MissionCategory category
        ) {
            return new MissionData {
                id = id,
                title = title,
                description = description,
                missionType = MissionType.Generic,
                missionCategory = category,
                prerequisiteMissionIDs = new string[0],
                requiredStoryFlags = new string[0],
                factionRestriction = FactionType.None,
                primaryObjectives = new MissionObjectiveData[0],
                optionalObjectives = new MissionObjectiveData[0],
                rewards = MissionRewards.Create(),
                consequences = MissionConsequences.Create(),
                baseEnemyCount = 6,
                recommendedSquadLevel = 1
            };
        }


    }

}
