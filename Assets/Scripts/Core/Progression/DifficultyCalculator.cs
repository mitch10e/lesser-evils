using System;
using Game.Core.Data;
using Game.Core.States;
using UnityEngine;

namespace Game.Core.Progression {

    public static class DifficultyCalculator {

        public static float getDifficultyRatio(
            ProgressionState progression,
            UnitRosterState roster,
            TechState technology
        ) {
            float playerPower = PlayerPowerEstimator.calculate(roster, technology);
            float worldThreat = progression.worldThreatLevel;

            if (playerPower <= 0) {
                return ProgressionConstants.DIFFICULTY_PUNISHING;
            }

            return worldThreat / playerPower;
        }

        // TODO: Will implement more as systems come online (as ratio gets higher, add more units, more advanced enemies, etc)

    }

}
