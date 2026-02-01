using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionRecord {

        public string missionID;

        public bool wasSuccessful;

        public List<string> completedOptionalObjectives;

        public List<string> injuredUnitIDs;

        public List<string> deadUnitIDs;

        public Dictionary<ResourceType, int> resourcesGained;

        public Dictionary<ResourceType, int> resourcesSpent;

        public List<Choice> choicesMade;

        public int turnsTaken;

        public static MissionRecord Create(string missionID) {
            return new MissionRecord {
                missionID = missionID,
                wasSuccessful = false,
                completedOptionalObjectives = new List<string>(),
                injuredUnitIDs = new List<string>(),
                deadUnitIDs = new List<string>(),
                resourcesGained = new Dictionary<ResourceType, int>(),
                resourcesSpent = new Dictionary<ResourceType, int>(),
                choicesMade = new List<Choice>(),
                turnsTaken = 0
            };
        }



    }

}
