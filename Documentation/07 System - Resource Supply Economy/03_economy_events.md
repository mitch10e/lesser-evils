# Part 3: Economy Events and UI Integration

The economy needs to communicate changes to the UI — when resources change, when the player can't afford something, and when supplies are running low.

## Step 3.1: Resource Changed Event

Create `Assets/Scripts/Core/Events/ResourceChangedEvent.cs`:

```csharp
using Game.Core.Data;

namespace Game.Core.Events {

    public struct ResourceChangedEvent {

        public ResourceType type;

        public int previousAmount;

        public int newAmount;

        public int delta;

    }

}
```

## Step 3.2: Update ResourceState to Fire Events

Add event publishing to `ResourceState`. Modify the `add()` and `spend()` methods:

```csharp
// Update ResourceState.add():

public void add(ResourceType type, int amount) {
    if (!resources.ContainsKey(type)) {
        resources[type] = 0;
    }

    int previous = resources[type];
    resources[type] += amount;

    if (resources[type] < 0) {
        resources[type] = 0;
    }

    EventBus.publish(new ResourceChangedEvent {
        type = type,
        previousAmount = previous,
        newAmount = resources[type],
        delta = resources[type] - previous
    });
}

// Update ResourceState.spend():

public bool spend(ResourceType type, int amount) {
    if (!hasResource(type, amount)) {
        return false;
    }

    int previous = resources[type];
    resources[type] -= amount;

    EventBus.publish(new ResourceChangedEvent {
        type = type,
        previousAmount = previous,
        newAmount = resources[type],
        delta = -amount
    });

    return true;
}
```

Every resource change now broadcasts an event. The UI can subscribe once and react to any resource change without polling.

## Step 3.3: Scarcity Warning Event

Create `Assets/Scripts/Core/Events/ResourceScarcityEvent.cs`:

```csharp
using Game.Core.Data;

namespace Game.Core.Events {

    public struct ResourceScarcityEvent {

        public ResourceType type;

        public int currentAmount;

        public int threshold;

    }

}
```

## Step 3.4: Scarcity Checker

Create `Assets/Scripts/Core/Economy/ScarcityChecker.cs`:

```csharp
using Game.Core.Data;
using Game.Core.States;
using Core.Game.Events;

namespace Game.Core.Economy {

    public static class ScarcityChecker {

        public static void check(ResourceState resources) {
            checkThreshold(resources, ResourceType.Fuel,
                EconomyConstants.LOW_FUEL_THRESHOLD);

            checkThreshold(resources, ResourceType.MedicalSupplies,
                EconomyConstants.LOW_MEDICAL_THRESHOLD);
        }

        private static void checkThreshold(
            ResourceState resources,
            ResourceType type,
            int threshold
        ) {
            int current = resources.get(type);

            if (current <= threshold && current > 0) {
                EventBus.publish(new ResourceScarcityEvent {
                    type = type,
                    currentAmount = current,
                    threshold = threshold
                });
            }
        }

    }

}
```

Call `ScarcityChecker.check()` after missions complete or at the start of the strategic layer. It fires a warning event for resources below their threshold — the UI can show "Fuel reserves low" style messages.

## Step 3.5: Economy UI Example

```csharp
using UnityEngine;
using UnityEngine.UI;
using Game.Core.Data;
using Game.Core.Events;
using Game.Core.Economy;
using Core.Game.Events;

public class EconomyUI : MonoBehaviour {

    [SerializeField] private Text currencyText;
    [SerializeField] private Text fuelText;
    [SerializeField] private Text alloysText;
    [SerializeField] private Text techCompText;
    [SerializeField] private Text intelText;
    [SerializeField] private Text medSupplyText;

    [SerializeField] private GameObject scarcityWarning;
    [SerializeField] private Text scarcityText;

    private void Start() {
        EventBus.subscribe<ResourceChangedEvent>(onResourceChanged);
        EventBus.subscribe<ResourceScarcityEvent>(onScarcityWarning);
        EventBus.subscribe<DeploymentCostFailedEvent>(onDeploymentFailed);

        refreshAll();
    }

    private void onResourceChanged(ResourceChangedEvent e) {
        updateResourceDisplay(e.type, e.newAmount);
    }

    private void onScarcityWarning(ResourceScarcityEvent e) {
        scarcityWarning.SetActive(true);
        scarcityText.text = $"{e.type} reserves critical: {e.currentAmount}";
    }

    private void onDeploymentFailed(DeploymentCostFailedEvent e) {
        string missing = string.Join(", ", e.missingResources);
        scarcityText.text = $"Cannot deploy: insufficient {missing}";
        scarcityWarning.SetActive(true);
    }

    private void refreshAll() {
        var resources = GameStateManager.instance.resources;

        updateResourceDisplay(ResourceType.Currency, resources.get(ResourceType.Currency));
        updateResourceDisplay(ResourceType.Fuel, resources.get(ResourceType.Fuel));
        updateResourceDisplay(ResourceType.Alloys, resources.get(ResourceType.Alloys));
        updateResourceDisplay(ResourceType.TechComponents, resources.get(ResourceType.TechComponents));
        updateResourceDisplay(ResourceType.Intel, resources.get(ResourceType.Intel));
        updateResourceDisplay(ResourceType.MedicalSupplies, resources.get(ResourceType.MedicalSupplies));
    }

    private void updateResourceDisplay(ResourceType type, int amount) {
        switch (type) {
            case ResourceType.Currency: currencyText.text = $"Credits: {amount}"; break;
            case ResourceType.Fuel: fuelText.text = $"Fuel: {amount}"; break;
            case ResourceType.Alloys: alloysText.text = $"Alloys: {amount}"; break;
            case ResourceType.TechComponents: techCompText.text = $"Tech: {amount}"; break;
            case ResourceType.Intel: intelText.text = $"Intel: {amount}"; break;
            case ResourceType.MedicalSupplies: medSupplyText.text = $"Medical: {amount}"; break;
        }
    }

    private void OnDestroy() {
        EventBus.unsubscribe<ResourceChangedEvent>(onResourceChanged);
        EventBus.unsubscribe<ResourceScarcityEvent>(onScarcityWarning);
        EventBus.unsubscribe<DeploymentCostFailedEvent>(onDeploymentFailed);
    }

}
```

## Step 3.6: The Economy Loop

```
Mission rewards
    │ ──► Resources added ──► ResourceChangedEvent ──► UI updates
    │
    ▼
Strategic layer
    │ ──► Passive income (daily fuel/currency)
    │ ──► Action costs deducted (research, build)
    │ ──► Medical supply usage (accelerate recovery)
    │
    ▼
Next mission
    │ ──► Deployment cost check
    │       ├── Can afford ──► Deduct, deploy
    │       └── Cannot afford ──► DeploymentCostFailedEvent ──► UI warning
    │
    ▼
ScarcityChecker
    │ ──► Low fuel? ──► ResourceScarcityEvent ──► "Fuel reserves low"
    │ ──► Low medical? ──► ResourceScarcityEvent ──► "Medical supplies critical"
    │
    ▼
Player decision
    ├── Take a generic mission for resources?
    ├── Wait for passive income (but threat grows)?
    └── Deploy shorthanded to save fuel?
```

This loop creates the core economic tension. Resources flow in from missions and trickle in passively. Resources flow out through deployment, construction, research, and healing. The player must balance spending against earning, and the opportunity cost of time against the growing world threat.

**Checkpoint:** Create event structs, update `ResourceState` to publish events, create `ScarcityChecker`. The economy should now communicate all changes to any listening UI.
