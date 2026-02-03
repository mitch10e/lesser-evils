using System;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGraph;

namespace Game.Core.Data {

    [Serializable]
    public struct TechData {

        public string id;

        public string name;

        public string description;

        public int researchTimeRequired;

        public List<String> prerequisiteTechIDs;

        public Dictionary<ResourceType, int> resourceCosts;

        public Dictionary<MaterialType, int> materialCosts;

        public static TechData Create(
            string id,
            string name,
            string description,
            int researchTime,
            List<string> prerequisites = null,
            Dictionary<ResourceType, int> resourceCosts = null,
            Dictionary<MaterialType, int> materialCosts = null
        ) {
            return new TechData {
                id = id,
                name = name,
                description = description,
                researchTimeRequired = researchTime,
                prerequisiteTechIDs = prerequisites,
                resourceCosts = resourceCosts,
                materialCosts = materialCosts
            };
        }

    }

}
