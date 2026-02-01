using System;

namespace Game.Core.Data {

    [Serializable]
    public struct Choice {

        public string missionID;

        public string choiceID;

        public string description;

        public int selectedOption;

        public int impact;

        public int turnNumber;

        public static Choice Create(
            string missionID,
            string choiceID,
            string description,
            int selectedOption,
            int impact,
            int turn
        ) {
            return new Choice {
                missionID = missionID,
                choiceID = choiceID,
                description = description,
                selectedOption = selectedOption,
                impact = impact,
                turnNumber = turn
            };
        }

    }

}
