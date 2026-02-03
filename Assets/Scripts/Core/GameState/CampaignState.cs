using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class CampaignState {

        public ActType currentAct;

        public int elapsedTime;

        public FactionType startingFaction;

        public FactionType currentFaction;

        public List<string> activeStoryFlags;

        public CampaignState() {
            currentAct = ActType.Act1;
            startingFaction = FactionType.None;
            currentFaction = FactionType.None;
            activeStoryFlags = new List<string>();
            elapsedTime = 0;
        }

        public void setAct(ActType type) {
            this.currentAct = type;
        }

        public void setFaction(FactionType type) {
            this.currentFaction = type;
        }

        public void setStartingFaction(FactionType type) {
            this.startingFaction = type;
        }

        public void addStoryFlag(string flag) {
            if (!activeStoryFlags.Contains(flag)) {
                activeStoryFlags.Add(flag);
            }
        }

        public bool hasStoryFlag(string flag) {
            return activeStoryFlags.Contains(flag);
        }

        public void removeStoryFlag(string flag) {
            activeStoryFlags.Remove(flag);
        }

        public void passTime(int duration) {
            elapsedTime += duration;
        }

        public void reset() {
            currentAct = ActType.Act1;
            startingFaction = FactionType.None;
            currentFaction = FactionType.None;
            activeStoryFlags.Clear();
            elapsedTime = 0;
        }

    }

}
