using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Progression {

    public static class WorldEventTriggers {

        public static List<string> checkTimeEvents(
            ProgressionState progression,
            List<string> alreadyTriggered
        ) {
            List<string> newTriggers = new();
            int weeks = progression.getElapsedWeeks();

            string weekEvent = $"week_{weeks}_event";
            if (!alreadyTriggered.Contains(weekEvent) && weeks > 0) {
                newTriggers.Add(weekEvent);
            }

            if (
                progression.worldThreatLevel >= 1.5
                && !alreadyTriggered.Contains("threat_level_punishing")
            ) {
                newTriggers.Add("threat_level_punishing");
            }

            if (
                 progression.worldThreatLevel >= 2.0
                && !alreadyTriggered.Contains("threat_level_critical")
            ) {
                newTriggers.Add("threat_level_critical");
            }

            return newTriggers;
        }

        public static string checkDifficultyWarning(
            float difficultyRatio,
            List<string> alreadyTriggered
        ) {
            if (
                difficultyRatio >= ProgressionConstants.DIFFICULTY_CHALLENGING
                && !alreadyTriggered.Contains("warning_falling_behind")
            ) {
                return "warning_falling_behind";
            }

            if (
                difficultyRatio >= ProgressionConstants.DIFFICULTY_PUNISHING
                && !alreadyTriggered.Contains("warning_critical_danger")
            ) {
                return "warning_critical_danger";
            }

            return null;
        }

    }

}
