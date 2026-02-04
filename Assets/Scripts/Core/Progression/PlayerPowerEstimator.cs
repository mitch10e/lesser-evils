using System.Linq;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class PlayerPowerEstimator {

        public static float calculate(UnitRosterState roster, TechState technology) {
            float squadPower = calculateSquadPower(roster);
            float techPower = calculateTechPower(technology);

            return (squadPower * 0.7f) + (techPower * 0.3f);
        }

        private static float calculateSquadPower(UnitRosterState roster) {
            var activeUnits = roster.getActiveUnits();

            if (activeUnits.Count == 0) {
                return ProgressionConstants.BASE_THREAT_LEVEL;
            }

            // TODO: Replace with current squad size upgrade value
            float sizeScore = activeUnits.Count / GameConstants.MAX_SQUAD_SIZE_1;

            double avgLevel = activeUnits.Average(u => u.level);
            double levelScore = avgLevel / (float)GameConstants.MAX_UNIT_LEVEL;

            return (float)(ProgressionConstants.BASE_THREAT_LEVEL
            + (sizeScore * ProgressionConstants.SQUAD_SIZE_WEIGHT)
            + (levelScore * ProgressionConstants.AVERAGE_ROSTER_LEVEL_WEIGHT));
        }

        private static float calculateTechPower(TechState technology) {
            const float estimatedTotalTechs = 30f;
            float techScore = technology.unlockedTechIDs.Count / estimatedTotalTechs;

            return techScore * ProgressionConstants.TECH_UNLOCKS_WEIGHT;
        }

    }

}
