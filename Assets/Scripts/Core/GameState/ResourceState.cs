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
