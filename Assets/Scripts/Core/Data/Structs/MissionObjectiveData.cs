using System;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionObjectiveData {

        public string id;

        public string description;

        public bool isOptional;

        public static MissionObjectiveData Create(
            string id,
            string description,
            bool optional = false
        ) {
            return new MissionObjectiveData {
                id = id,
                description = description,
                isOptional = optional
            };
        }

    }

}
