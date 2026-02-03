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
