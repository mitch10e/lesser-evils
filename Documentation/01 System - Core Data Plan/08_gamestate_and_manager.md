# Part 5: The Master GameState and Manager

Now we'll create the top-level container and the event system for decoupled communication.

## Step 5.1: GameState Container

Create `Assets/Scripts/Core/GameState/GameState.cs`:

```csharp
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
```

**Design Note:** The `version` field enables save compatibility. When you make breaking changes to the state structure, increment the version and add migration logic in the save/load system.

## Step 5.2: Event Definitions

Create individual event files in `Assets/Scripts/Core/Events/`:

**StateChangedEvent.cs:**
```csharp
namespace Game.Core.Events {

    public struct StateChangedEvent {

        public string changedSystem;

    }

}
```

**ResourceChangedEvent.cs:**
```csharp
using Game.Core.Data;

namespace Game.Core.Events {

    public struct ResourceChangedEvent {

        public ResourceType resourceType;
        public int oldAmount;
        public int newAmount;

    }

}
```

**MaterialChangedEvent.cs:**
```csharp
using Game.Core.Data;

namespace Game.Core.Events {

    public struct MaterialChangedEvent {

        public MaterialType materialType;
        public int oldAmount;
        public int newAmount;

    }

}
```

**MissionCompletedEvent.cs:**
```csharp
namespace Game.Core.Events {

    public struct MissionCompletedEvent {

        public string missionID;
        public bool wasSuccessful;

    }

}
```

**UnitStatusChangedEvent.cs:**
```csharp
using Game.Core.Data;

namespace Game.Core.Events {

    public struct UnitStatusChangedEvent {

        public string unitID;
        public UnitStatus oldStatus;
        public UnitStatus newStatus;

    }

}
```

## Step 5.3: EventBus

Create `Assets/Scripts/Core/Events/EventBus.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Core.Game.Events {

    public static class EventBus {

        private static Dictionary<Type, List<Delegate>> subscribers = new();

        public static void subscribe<T>(Action<T> handler) {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType)) {
                subscribers[eventType] = new();
            }

            subscribers[eventType].Add(handler);
        }

        public static void unsubscribe<T>(Action<T> handler) {
            Type eventType = typeof(T);

            if (subscribers.ContainsKey(eventType)) {
                subscribers[eventType].Remove(handler);
            }
        }

        public static void publish<T>(T eventData) {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType)) return;

            var handlers = new List<Delegate>(subscribers[eventType]);
            foreach (var handler in handlers) {
                ((Action<T>)handler)?.Invoke(eventData);
            }
        }

        public static void clear() {
            subscribers.Clear();
        }

    }

}
```

**Design Note:** The EventBus provides decoupled communication between systems. Any system can publish events without knowing who's listening, and any system can subscribe without knowing who's publishing.
