using System;
using Game.Core.States;
using UnityEngine;

namespace Game.Core {

    [Serializable]
    public class GameState {

        public int version;

        public CampaignState campaign;

        public MaterialState materials;

        public ResourceState resources;

        public TechState technology;

        public UnitRosterState roster;

        public GameState() {
            version = 1;
            campaign = new CampaignState();
            materials = new MaterialState();
            resources = new ResourceState();
            technology = new TechState();
            roster = new UnitRosterState();
        }

        public GameState createDeepCopy() {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<GameState>(json);
        }

        public void reset() {
            campaign.reset();
            materials.reset();
            resources.reset();
            technology.reset();
            roster.reset();
        }

    }

}
