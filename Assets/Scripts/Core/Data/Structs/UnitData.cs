using System;

namespace Game.Core.Data {

    [Serializable]
    public struct UnitData {

        public string id;

        public string displayName;

        public int level;

        public int experience;

        public int experienceToNextLevel;

        public UnitStatus status;

        public string loadoutID;

        /// <summary>
        /// May have "plot armor"
        /// </summary>
        public bool isVIP;

        public static UnitData CreateDefault(string id, string name) {
            return new UnitData {
                id = id,
                displayName = name,
                level = 1,
                experience = 0,
                experienceToNextLevel = 100,
                status = UnitStatus.Active,
                loadoutID = "",
                isVIP = false
            };
        }

    }

}
