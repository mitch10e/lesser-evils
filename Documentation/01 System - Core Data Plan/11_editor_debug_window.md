# Part 7: Creating the Editor Debug Window

Let's create a simple editor tool to view and modify game state during development.

## Step 7.1: Debug Window

Create `Assets/Scripts/Editor/GameState/GameStateDebugWindow.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using Game.Core;
using Game.Core.Data;

namespace Game.Editor
{
    /// <summary>
    /// Editor window for viewing and debugging game state.
    /// Open via Window > Game Debug > Game State Debug
    /// </summary>
    public class GameStateDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Campaign", "Squad", "Resources", "Materials", "Factions", "Tech", "Branch" };

        [MenuItem("Window/Game Debug/Game State Debug")]
        public static void ShowWindow()
        {
            GetWindow<GameStateDebugWindow>("Game State Debug");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view game state.", MessageType.Info);
                return;
            }

            if (GameStateManager.Instance == null)
            {
                EditorGUILayout.HelpBox("GameStateManager not found in scene.", MessageType.Warning);
                return;
            }

            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0: DrawCampaignTab(); break;
                case 1: DrawSquadTab(); break;
                case 2: DrawResourcesTab(); break;
                case 3: DrawMaterialsTab(); break;
                case 4: DrawFactionsTab(); break;
                case 5: DrawTechTab(); break;
                case 6: DrawBranchTab(); break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Global controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults"))
            {
                GameStateManager.Instance.ResetToDefaults();
            }
            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCampaignTab()
        {
            var campaign = GameStateManager.Instance.Campaign;

            EditorGUILayout.LabelField("Campaign State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Act:", campaign.CurrentAct.ToString());
            EditorGUILayout.LabelField("Current Turn:", campaign.CurrentTurn.ToString());
            EditorGUILayout.LabelField("Mission Index:", campaign.CurrentMissionIndex.ToString());
            EditorGUILayout.LabelField("Missions Completed:", campaign.CompletedMissionIds.Count.ToString());

            EditorGUILayout.Space();

            if (GUILayout.Button("Advance Act"))
            {
                GameStateManager.Instance.TryAdvanceAct();
            }

            if (GUILayout.Button("End Turn"))
            {
                GameStateManager.Instance.EndTurn();
            }
        }

        private void DrawSquadTab()
        {
            var squad = GameStateManager.Instance.Squad;

            EditorGUILayout.LabelField("Squad State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Squad Size:", squad.MaxSquadSize.ToString());
            EditorGUILayout.LabelField("Total Units:", squad.Roster.Count.ToString());
            EditorGUILayout.LabelField("Active Units:", squad.GetActiveUnits().Count.ToString());
            EditorGUILayout.LabelField("Injured Units:", squad.GetInjuredUnits().Count.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unit Roster:", EditorStyles.boldLabel);

            foreach (var unit in squad.Roster)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{unit.DisplayName} (Lvl {unit.Level})");
                EditorGUILayout.LabelField($"Status: {unit.Status}");
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Test Unit"))
            {
                var newUnit = UnitData.CreateDefault(
                    $"test_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                    $"Test Unit {squad.Roster.Count + 1}"
                );
                squad.AddUnit(newUnit);
            }
        }

        private void DrawResourcesTab()
        {
            var resources = GameStateManager.Instance.Resources;

            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.ToString() + ":", GUILayout.Width(120));

                int current = resources.GetResource(type);
                int newValue = EditorGUILayout.IntField(current);

                if (newValue != current)
                {
                    resources.SetResource(type, newValue);
                }

                if (GUILayout.Button("+100", GUILayout.Width(50)))
                {
                    resources.AddResource(type, 100);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFactionsTab()
        {
            var factions = GameStateManager.Instance.Factions;

            EditorGUILayout.LabelField("Faction State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Player Faction:", factions.PlayerFaction.ToString());

            // Future expansion: Add loyalty tracking, defection status, faction relationships here
        }

        private void DrawMaterialsTab()
        {
            var materials = GameStateManager.Instance.Materials;

            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);

            foreach (MaterialType type in System.Enum.GetValues(typeof(MaterialType)))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.ToString() + ":", GUILayout.Width(150));

                int current = materials.GetMaterial(type);
                EditorGUILayout.LabelField(current.ToString(), GUILayout.Width(50));

                if (GUILayout.Button("+1", GUILayout.Width(40)))
                {
                    materials.AddMaterial(type, 1);
                }
                if (GUILayout.Button("+10", GUILayout.Width(40)))
                {
                    materials.AddMaterial(type, 10);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawTechTab()
        {
            var tech = GameStateManager.Instance.Tech;

            EditorGUILayout.LabelField("Tech State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Unlocked Techs: {tech.UnlockedTechIds.Count}");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Research:", EditorStyles.boldLabel);
            if (tech.IsResearching())
            {
                EditorGUILayout.LabelField($"  Researching: {tech.CurrentResearchId}");
                EditorGUILayout.LabelField($"  Progress: {tech.CurrentResearchProgress} turns");
            }
            else
            {
                EditorGUILayout.LabelField("  (None)");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Story Milestones:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Has Precursor Access: {tech.HasPrecursorAccess}");
            EditorGUILayout.LabelField($"  Has Full Extinction Tech: {tech.HasFullExtinctionTech}");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Grant Precursor"))
            {
                tech.GrantPrecursorAccess();
            }
            if (GUILayout.Button("Revoke Precursor"))
            {
                tech.RevokePrecursorAccess();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBranchTab()
        {
            var branch = GameStateManager.Instance.Branch;

            EditorGUILayout.LabelField("Branch State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Path: {(branch.IsLoyalistPath ? "Loyalist" : "Defector")}");
            EditorGUILayout.LabelField($"Act 2 Decision Made: {branch.HasMadeAct2Decision}");
            EditorGUILayout.LabelField($"Defeated Faction: {branch.DefeatedFaction}");
            EditorGUILayout.LabelField($"Story Flags: {branch.ActiveStoryFlags.Count}");

            EditorGUILayout.Space();
            if (branch.ActiveStoryFlags.Count > 0)
            {
                EditorGUILayout.LabelField("Active Flags:", EditorStyles.boldLabel);
                foreach (var flag in branch.ActiveStoryFlags)
                {
                    EditorGUILayout.LabelField($"  - {flag}");
                }
            }
        }

        private void OnInspectorUpdate()
        {
            // Repaint during play mode to show live updates
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
```

## Using the Debug Window

1. In Unity, go to `Window > Game Debug > Game State Debug`
2. Enter Play Mode
3. Use the tabs to explore different state systems
4. Use the buttons to modify state and test your systems
