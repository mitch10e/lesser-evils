using System;

namespace Game.Core.Data {

    [Serializable]
    public struct UnitPerformance {

        public string unitID;

        public int kills;

        public int damageDealt;

        public int damageTaken;

        public int damageHealed;

        public int objectivesCompleted;

        public const int POINTS_PER_KILL = 10;
        public const int POINTS_PER_DAMAGE_DEALT = 2;
        public const int POINTS_PER_DAMAGE_TAKEN = -1;
        public const int POINTS_PER_DAMAGE_HEALED = 2;
        public const int POINTS_PER_OBJECTIVE = 15;
        public const float MIN_PARTICIPATION_SHARE = 0.25f;

        public int getContributionScore() {
            return (kills * POINTS_PER_KILL)
            + (damageDealt * POINTS_PER_DAMAGE_DEALT)
            - (damageTaken * POINTS_PER_DAMAGE_TAKEN)
            + (damageHealed * POINTS_PER_DAMAGE_HEALED)
            + (objectivesCompleted * POINTS_PER_OBJECTIVE);
        }

        public static UnitPerformance Create(string unitID) {
            return new UnitPerformance {
                unitID = unitID,
                kills = 0,
                damageDealt = 0,
                damageTaken = 0,
                damageHealed = 0,
                objectivesCompleted = 0
            };
        }

    }

}
