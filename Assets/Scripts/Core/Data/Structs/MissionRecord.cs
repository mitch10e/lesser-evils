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

        public List<UnitPerformance> unitPerformances;

        public List<LootDrop> collectedLoot;

        public Dictionary<ResourceType, int> resourcesGained;

        public Dictionary<ResourceType, int> resourcesSpent;

        public List<Choice> choicesMade;

        public int turnsTaken;

        public static MissionRecord Create(string missionID) {
            return new MissionRecord {
                missionID = missionID,
                wasSuccessful = false,
                completedOptionalObjectives = new(),
                injuredUnitIDs = new(),
                unitPerformances = new(),
                collectedLoot = new(),
                deadUnitIDs = new(),
                resourcesGained = new(),
                resourcesSpent = new(),
                choicesMade = new(),
                turnsTaken = 0
            };
        }



    }

}
