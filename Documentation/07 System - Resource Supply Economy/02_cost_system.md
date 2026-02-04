# Part 2: Cost System

Every meaningful action should cost something. This part creates the validation layer that checks if the player can afford an action before allowing it.

## Step 2.1: Action Cost

Create `Assets/Scripts/Core/Economy/ActionCost.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Economy {

    [Serializable]
    public struct ActionCost {

        public Dictionary<ResourceType, int> resources;

        public Dictionary<MaterialType, int> materials;

        // MARK: - Factory

        public static ActionCost Free() {
            return new ActionCost {
                resources = new Dictionary<ResourceType, int>(),
                materials = new Dictionary<MaterialType, int>()
            };
        }

        public static ActionCost Create(ResourceType type, int amount) {
            var cost = Free();
            cost.resources[type] = amount;
            return cost;
        }

        // MARK: - Builder

        public ActionCost withResource(ResourceType type, int amount) {
            resources[type] = amount;
            return this;
        }

        public ActionCost withMaterial(MaterialType type, int amount) {
            materials[type] = amount;
            return this;
        }

        // MARK: - Queries

        public bool isEmpty() {
            return resources.Count == 0 && materials.Count == 0;
        }

    }

}
```

The builder pattern makes costs easy to define inline:

```csharp
var cost = ActionCost.Create(ResourceType.Fuel, 30)
    .withResource(ResourceType.Currency, 50)
    .withMaterial(MaterialType.Placeholder, 5);
```

## Step 2.2: Cost Validator

Create `Assets/Scripts/Core/Economy/CostValidator.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Economy {

    public static class CostValidator {

        public static bool canAfford(ActionCost cost, ResourceState resources, MaterialState materials) {
            foreach (var kvp in cost.resources) {
                if (!resources.hasResource(kvp.Key, kvp.Value)) {
                    return false;
                }
            }

            foreach (var kvp in cost.materials) {
                if (!materials.hasMaterial(kvp.Key, kvp.Value)) {
                    return false;
                }
            }

            return true;
        }

        public static bool spend(ActionCost cost, ResourceState resources, MaterialState materials) {
            if (!canAfford(cost, resources, materials)) return false;

            foreach (var kvp in cost.resources) {
                resources.spend(kvp.Key, kvp.Value);
            }

            foreach (var kvp in cost.materials) {
                materials.spend(kvp.Key, kvp.Value);
            }

            return true;
        }

        public static List<ResourceType> getMissingResources(
            ActionCost cost,
            ResourceState resources
        ) {
            List<ResourceType> missing = new();

            foreach (var kvp in cost.resources) {
                if (!resources.hasResource(kvp.Key, kvp.Value)) {
                    missing.Add(kvp.Key);
                }
            }

            return missing;
        }

    }

}
```

`canAfford()` checks without spending. `spend()` checks and deducts atomically. `getMissingResources()` returns what the player needs so the UI can highlight it.

## Step 2.3: Deployment Cost Calculator

Create `Assets/Scripts/Core/Economy/DeploymentCostCalculator.cs`:

```csharp
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Economy {

    public static class DeploymentCostCalculator {

        public static ActionCost calculate(MissionData mission, int deployedUnitCount) {
            // Base fuel cost + per-unit cost
            int fuelCost = EconomyConstants.DEPLOY_FUEL_BASE +
                (EconomyConstants.DEPLOY_FUEL_PER_UNIT * deployedUnitCount);

            var cost = ActionCost.Create(ResourceType.Fuel, fuelCost);

            // Specific mission categories have additional costs
            switch (mission.category) {
                case MissionCategory.Recon:
                    // Recon requires intel investment
                    cost = cost.withResource(ResourceType.Intel, 5);
                    break;

                case MissionCategory.Sabotage:
                    // Sabotage needs special equipment
                    cost = cost.withResource(ResourceType.Alloys, 10);
                    break;

                case MissionCategory.Assassination:
                    // High-value ops need intel
                    cost = cost.withResource(ResourceType.Intel, 10);
                    break;
            }

            return cost;
        }

    }

}
```

Deployment costs create a core tension: bigger squads are safer but cost more fuel. Fuel is limited, so the player must balance squad size against mission frequency.

## Step 2.4: Integration with MissionManager

Update `MissionManager.startMission()` to check and deduct deployment costs:

```csharp
// In MissionManager.startMission(), add before setting activeMission:

public bool startMission(string missionID) {
    if (isInMission) {
        Debug.LogWarning("Cannot start mission — already in a mission");
        return false;
    }

    // ... existing mission lookup code ...

    // Check deployment cost
    var gm = GameStateManager.instance;
    var cost = DeploymentCostCalculator.calculate(mission, gm.roster.deployedUnitIDs.Count);

    if (!CostValidator.canAfford(cost, gm.resources, gm.materials)) {
        Debug.LogWarning("Cannot afford deployment cost");
        EventBus.publish(new DeploymentCostFailedEvent {
            missionID = missionID,
            missingResources = CostValidator.getMissingResources(cost, gm.resources)
        });
        return false;
    }

    // Deduct cost
    CostValidator.spend(cost, gm.resources, gm.materials);

    activeMission = mission;
    // ... rest of existing code ...
}
```

## Step 2.5: Deployment Cost Failed Event

Create `Assets/Scripts/Core/Events/DeploymentCostFailedEvent.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.Events {

    public struct DeploymentCostFailedEvent {

        public string missionID;

        public List<ResourceType> missingResources;

    }

}
```

The UI subscribes to this event to show "Insufficient fuel" or "Need more intel" messages.

## Step 2.6: Strategic Action Costs

Update `TimeAction` to carry a cost:

```csharp
// Add to TimeAction class:

public ActionCost cost;

// Update factory methods to include costs, e.g.:

public static TimeAction createResearch(string techID, string name, int hours) {
    return new TimeAction {
        id = $"research_{techID}",
        displayName = $"Research: {name}",
        durationHours = hours,
        elapsedHours = 0,
        actionType = TimeActionType.Research,
        targetID = techID,
        cost = ActionCost.Create(ResourceType.TechComponents, 10)
    };
}

public static TimeAction createBuild(string facilityID, string name, int hours) {
    return new TimeAction {
        id = $"build_{facilityID}",
        displayName = $"Build: {name}",
        durationHours = hours,
        elapsedHours = 0,
        actionType = TimeActionType.Build,
        targetID = facilityID,
        cost = ActionCost.Create(ResourceType.Currency, 50)
            .withResource(ResourceType.Alloys, 20)
    };
}
```

Then validate in `StrategicLayerManager.startAction()`:

```csharp
// Update StrategicLayerManager.startAction():

public void startAction(TimeAction action) {
    if (!canStartAction(action)) return;

    // Check cost
    var gm = GameStateManager.instance;
    if (!action.cost.isEmpty() &&
        !CostValidator.spend(action.cost, gm.resources, gm.materials)) {
        Debug.LogWarning($"Cannot afford action: {action.displayName}");
        return;
    }

    activeActions.Add(action);
    onActionStarted?.Invoke(action);
}
```

Resources are deducted when the action starts, not when it completes. This means the player commits resources upfront — no take-backs once research begins.

**Checkpoint:** Create `ActionCost`, `CostValidator`, `DeploymentCostCalculator`. Update `MissionManager.startMission()` to check costs. Add `cost` field to `TimeAction`. Update `StrategicLayerManager.startAction()` to validate costs.
