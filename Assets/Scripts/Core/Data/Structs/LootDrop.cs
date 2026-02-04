using System;

namespace Game.Core.Data {

    [Serializable]
    public struct LootDrop {

        public LootCategory category;

        public MaterialType materialType;

        public string equipmentID;

        public int quantity;

        public static LootDrop CreateMaterial(MaterialType type, int quantity) {
            return new LootDrop {
                category = LootCategory.Material,
                materialType = type,
                equipmentID = "",
                quantity = quantity
            };
        }

        public static LootDrop CreateEquipment(string equipmentID) {
            return new LootDrop {
                category = LootCategory.Equipment,
                equipmentID = equipmentID,
                quantity = 1
            };
        }

    }

}
