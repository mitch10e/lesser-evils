# Part 4: Creating the State Classes

Now we'll create the individual state classes that track different aspects of the game.

## Step 4.1: ResourceState

Create `Assets/Scripts/Core/GameState/ResourceState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class ResourceState {

        public Dictionary<ResourceType, int> resources;

        public ResourceState() {
            resources = new Dictionary<ResourceType, int>();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) {
                resources[type] = 0;
            }
        }

        public int get(ResourceType type) {
            return resources.TryGetValue(type, out int amount) ? amount : 0;
        }

        public bool hasResource(ResourceType type, int amount) {
            return get(type) >= amount;
        }

        public void add(ResourceType type, int amount) {
            if (!resources.ContainsKey(type)) {
                resources[type] = 0;
            }

            resources[type] += amount;

            if (resources[type] < 0) {
                resources[type] = 0;
            }
        }

        public bool spend(ResourceType type, int amount) {
            if (!hasResource(type, amount)) {
                return false;
            }

            resources[type] -= amount;
            return true;
        }

        public void set(ResourceType type, int amount) {
            resources[type] = Math.Max(0, amount);
        }

        public void reset() {
            resources.Clear();
        }

    }

}
```

**Design Note:** Resources start at 0. Starting resources will be granted when a new game begins, allowing different difficulty modes or faction bonuses.

## Step 4.2: MaterialState

Create `Assets/Scripts/Core/GameState/MaterialState.cs`:

```csharp
using System;
using System.Collections.Generic;
using Game.Core.Data;

namespace Game.Core.States {

    [Serializable]
    public class MaterialState {

        public Dictionary<MaterialType, int> materials;

        public MaterialState() {
            materials = new Dictionary<MaterialType, int>();
        }

        public int get(MaterialType type) {
            return materials.TryGetValue(type, out int amount) ? amount : 0;
        }

        public bool hasMaterial(MaterialType type, int amount) {
            return get(type) >= amount;
        }

        public void add(MaterialType type, int amount) {
            if (!materials.ContainsKey(type)) {
                materials[type] = 0;
            }

            materials[type] += amount;

            if (materials[type] < 0) {
                materials[type] = 0;
            }
        }

        public bool spend(MaterialType type, int amount) {
            if (!hasMaterial(type, amount)) {
                return false;
            }

            materials[type] -= amount;
            return true;
        }

        public void set(MaterialType type, int amount) {
            materials[type] = Math.Max(0, amount);
        }

        public void reset() {
            materials.Clear();
        }

    }

}
```

**Design Note:** MaterialState mirrors ResourceState but is kept separate for special crafting materials that will be gained through missions.
