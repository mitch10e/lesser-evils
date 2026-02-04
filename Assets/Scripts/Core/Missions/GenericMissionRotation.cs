using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Progression;
using UnityEngine;

namespace Game.Core.Missions {

    public class GenericMissionRotation {

        private readonly List<MissionData> templates = new();

        private readonly List<MissionData> currentRotation = new();

        public const int MAX_AVAILABLE_GENERIC = 2;

        public void registerTemplate(MissionData template) {
            templates.Add(template);
        }

        public IReadOnlyList<MissionData> currentMissions => currentRotation;

        public void refreshRotation(
            ProgressionState progression,
            UnitRosterState roster,
            TechState technology
        ) {
            currentRotation.Clear();

            if (templates.Count == 0) return;

            float difficultyRatio = DifficultyCalculator.getDifficultyRatio(
                progression, roster, technology
            );

            var shuffled = new List<MissionData>(templates);
            shuffleList(shuffled);

            int count = Mathf.Min(MAX_AVAILABLE_GENERIC, shuffled.Count);

            for (int i = 0; i < count; i++) {
                var instance = instantiateFromTemplate(shuffled[i], difficultyRatio);
                currentRotation.Add(instance);
            }
        }

        private MissionData instantiateFromTemplate(
            MissionData template,
            float difficultyRatio
        ) {
            var instance = MissionData.CreateGeneric(
                $"{template.id}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                template.title,
                template.description,
                template.missionCategory
            );

            instance.baseEnemyCount = template.baseEnemyCount;
            instance.rewards = template.rewards;

            instance.primaryObjectives = template.primaryObjectives;
            instance.optionalObjectives = template.optionalObjectives;

            return instance;
        }

        // MARK: - Utility

        public void removeFromRotation(string missionID) {
            currentRotation.RemoveAll(m => m.id == missionID);
        }

        private void shuffleList<T>(List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

}
