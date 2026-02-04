using System;
using Game.Core.States;
using UnityEngine;

namespace Game.Core {

    [Serializable]
    public class GameState {

        public int version;

        public CampaignState campaign;

        public MaterialState materials;

        public ProgressionState progression;

        public ResourceState resources;

        public TechState technology;

        public UnitRosterState roster;

        public GameState() {
            version = 1;
            campaign = new();
            materials = new();
            progression = new();
            resources = new();
            technology = new();
            roster = new();
        }

        public GameState createDeepCopy() {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<GameState>(json);
        }

        public void reset() {
            campaign.reset();
            materials.reset();
            progression.reset();
            resources.reset();
            technology.reset();
            roster.reset();
        }

    }

}
