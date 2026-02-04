using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionRewards {

        public Dictionary<ResourceType, int> resources;

        public Dictionary<MaterialType, int> materials;

        public int baseXPPool;

        public LootDrop[] potentialDrops;

        public static MissionRewards Create(
            int baseXPPool = 0,
            LootDrop[] drops = null
        ) {
            return new MissionRewards {
                resources = new(),
                materials = new(),
                baseXPPool = baseXPPool,
                potentialDrops = drops ?? new LootDrop[0]
            };
        }

    }

}
