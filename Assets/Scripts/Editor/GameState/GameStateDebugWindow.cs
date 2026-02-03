using UnityEngine;
using UnityEditor;
using Game.Core;
using Game.Core.Data;

namespace Game.Editor {
    /// <summary>
    /// Editor window for viewing and debugging game state.
    /// Open via Window > Game Debug > Game State Debug
    /// </summary>
    public class GameStateDebugWindow : EditorWindow {

        private Vector2 scrollPosition;

        private int selectedTab = 0;

        private string[] tabNames = { "Campaign", "Squad", "Resources", "Materials", "Tech" };

        [MenuItem("Window/Game Debug/Game State Debug")]
        public static void showWindow() {
            GetWindow<GameStateDebugWindow>("Game State Debug");
        }

        private void OnGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Enter Play Mode to view game state.", MessageType.Info);
                return;
            }

            if (GameStateManager.instance == null) {
                EditorGUILayout.HelpBox("GameStateManager not found in scene.", MessageType.Warning);
                return;
            }

            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab) {
                case 0: DrawCampaignTab(); break;
                case 1: DrawSquadTab(); break;
                case 2: DrawResourcesTab(); break;
                case 3: DrawMaterialsTab(); break;
                case 4: DrawTechTab(); break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Global controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults")) {
                GameStateManager.instance.resetToDefaults();
            }
            if (GUILayout.Button("Refresh")) {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCampaignTab() {
            var campaign = GameStateManager.instance.campaign;

            EditorGUILayout.LabelField("Campaign State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Act:", campaign.currentAct.ToString());
            EditorGUILayout.LabelField("Current Turn:", campaign.elapsedTime.ToString());

            EditorGUILayout.Space();
        }

        private void DrawSquadTab() {
            var squad = GameStateManager.instance.roster;

            EditorGUILayout.LabelField("Squad State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Squad Size:", squad.maxSquadSize.ToString());
            EditorGUILayout.LabelField("Total Units:", squad.roster.Count.ToString());
            EditorGUILayout.LabelField("Active Units:", squad.getActiveUnits().Count.ToString());
            EditorGUILayout.LabelField("Injured Units:", squad.getInjuredUnits().Count.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unit Roster:", EditorStyles.boldLabel);

            foreach (var unit in squad.roster) {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{unit.displayName} (Lvl {unit.level})");
                EditorGUILayout.LabelField($"Status: {unit.status}");
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Test Unit")) {
                var newUnit = UnitData.CreateDefault(
                    $"test_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                    $"Test Unit {squad.roster.Count + 1}"
                );
                squad.add(newUnit);
            }
        }

        private void DrawResourcesTab() {
            var resources = GameStateManager.instance.resources;

            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType))) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.ToString() + ":", GUILayout.Width(120));

                int current = resources.get(type);
                int newValue = EditorGUILayout.IntField(current);

                if (newValue != current) {
                    resources.set(type, newValue);
                }

                if (GUILayout.Button("+100", GUILayout.Width(50))) {
                    resources.add(type, 100);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawMaterialsTab() {
            var materials = GameStateManager.instance.materials;

            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);

            foreach (MaterialType type in System.Enum.GetValues(typeof(MaterialType))) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.ToString() + ":", GUILayout.Width(150));

                int current = materials.get(type);
                EditorGUILayout.LabelField(current.ToString(), GUILayout.Width(50));

                if (GUILayout.Button("+1", GUILayout.Width(40))) {
                    materials.add(type, 1);
                }
                if (GUILayout.Button("+10", GUILayout.Width(40))) {
                    materials.add(type, 10);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawTechTab() {
            var tech = GameStateManager.instance.technology;

            EditorGUILayout.LabelField("Tech State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Unlocked Techs: {tech.unlockedTechIDs.Count}");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Research:", EditorStyles.boldLabel);
            if (tech.isResearching()) {
                EditorGUILayout.LabelField($"  Researching: {tech.currentResearchID}");
                EditorGUILayout.LabelField($"  Progress: {tech.currentResearchProgress} turns");
            } else {
                EditorGUILayout.LabelField("  (None)");
            }
        }

        private void OnInspectorUpdate() {
            // Repaint during play mode to show live updates
            if (Application.isPlaying) {
                Repaint();
            }
        }
    }
}
