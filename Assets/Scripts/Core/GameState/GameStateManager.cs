using System;
using UnityEngine;
using Game.Core.Data;
using Game.Core.States;
using Game.Core.Events;
using Core.Game.Events;

namespace Game.Core {

    public class GameStateManager : MonoBehaviour {

        public static GameStateManager instance { get; private set; }

        public GameState current { get; private set; }

        // MARK: - States

        public CampaignState campaign => current.campaign;

        public MaterialState materials => current.materials;

        public ResourceState resources => current.resources;

        public TechState technology => current.technology;

        public UnitRosterState roster => current.roster;

        // MARK: - Lifecycle

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        public void Initialize(GameState state = null) {
            current = state ?? new();
            notifyStateChanged("All");

            Debug.Log("GameStateManager initialized");
        }

        public void resetToDefaults() {
            current.reset();
            notifyStateChanged("All");
        }

        public GameState getStateCopy() {
            return current.createDeepCopy();
        }

        public void load(GameState state) {
            current = state;
            notifyStateChanged("All");
        }

        public void startNewGame(FactionType factionType) {
            resetToDefaults();
            campaign.setFaction(factionType);
            createStartingSquad(factionType);

            notifyStateChanged("All");
        }

        // MARK: - Private Methods

        private void notifyStateChanged(string system) {
            EventBus.publish(new StateChangedEvent { changedSystem = system });
        }

        private void createStartingSquad(FactionType faction) {
            string prefix = faction.ToString().Substring(0, 3);
            roster.add(UnitData.CreateDefault($"{prefix}_001.debug", $"Recruit 1"));
            roster.add(UnitData.CreateDefault($"{prefix}_002.debug", $"Recruit 2"));
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
        }

    }

}

