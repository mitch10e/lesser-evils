using System;

namespace Game.Core.Data {

    [Serializable]
    public struct MainMissionData {

        public string id;

        public string title;

        public string[] prerequisiteMissionIDs;

        public static MainMissionData Create(
            string id,
            string title,
            params string[] prerequisites
        ) {
            return new MainMissionData {
                id = id,
                title = title,
                prerequisiteMissionIDs = prerequisites ?? new string[0]
            };
        }

    }

}
