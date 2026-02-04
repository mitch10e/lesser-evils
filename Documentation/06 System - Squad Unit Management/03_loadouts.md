# Part 3: Loadouts

A loadout defines what a unit brings into combat — weapon, armor, and utility items. The combat system (Phase 13) will use loadouts to determine unit capabilities. This phase defines the data structures.

## Step 3.1: Equipment Slot Enum

Create `Assets/Scripts/Core/Data/Enums/EquipmentSlot.cs`:

```csharp
namespace Game.Core.Data {

    public enum EquipmentSlot {
        Weapon,
        Armor,
        Utility
    }

}
```

Three slots keeps loadout management simple. A unit has one weapon, one armor piece, and one utility item. Future phases can add more slots if needed.

## Step 3.2: Equipment Data

Create `Assets/Scripts/Core/Data/Structs/EquipmentData.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct EquipmentData {

        public string id;

        public string displayName;

        public EquipmentSlot slot;

        public Dictionary<string, float> statModifiers;

        public static EquipmentData Create(
            string id,
            string name,
            EquipmentSlot slot
        ) {
            return new EquipmentData {
                id = id,
                displayName = name,
                slot = slot,
                statModifiers = new Dictionary<string, float>()
            };
        }

    }

}
```

`statModifiers` is intentionally generic — `{"damage": 10, "accuracy": 0.8, "range": 5}`. The combat system will interpret these keys. Using a dictionary avoids hardcoding stat names into the data layer before combat is designed.

## Step 3.3: Loadout Data

Create `Assets/Scripts/Core/Data/Structs/LoadoutData.cs`:

```csharp
using System;

namespace Game.Core.Data {

    [Serializable]
    public struct LoadoutData {

        public string id;

        public string weaponID;

        public string armorID;

        public string utilityID;

        public static LoadoutData CreateDefault(string id) {
            return new LoadoutData {
                id = id,
                weaponID = "",
                armorID = "",
                utilityID = ""
            };
        }

        public bool hasWeapon => !string.IsNullOrEmpty(weaponID);

        public bool hasArmor => !string.IsNullOrEmpty(armorID);

        public bool hasUtility => !string.IsNullOrEmpty(utilityID);

    }

}
```

A loadout is a named configuration. The `loadoutID` field on `UnitData` points to one of these. This lets you predefine loadout templates (e.g., "Assault", "Sniper", "Medic") and assign them to units.

## Step 3.4: Loadout Manager

Create `Assets/Scripts/Core/Squad/LoadoutManager.cs`:

```csharp
using System.Collections.Generic;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Core.Squad {

    public class LoadoutManager {

        private Dictionary<string, LoadoutData> loadouts = new();
        private Dictionary<string, EquipmentData> equipment = new();

        // MARK: - Equipment Registry

        public void registerEquipment(EquipmentData item) {
            equipment[item.id] = item;
        }

        public EquipmentData? getEquipment(string equipmentID) {
            return equipment.TryGetValue(equipmentID, out var item) ? item : null;
        }

        // MARK: - Loadout Management

        public LoadoutData getLoadout(string loadoutID) {
            return loadouts.TryGetValue(loadoutID, out var loadout)
                ? loadout
                : LoadoutData.CreateDefault(loadoutID);
        }

        public void saveLoadout(LoadoutData loadout) {
            loadouts[loadout.id] = loadout;
        }

        public void equipWeapon(string loadoutID, string weaponID) {
            var loadout = getLoadout(loadoutID);
            loadout.weaponID = weaponID;
            saveLoadout(loadout);
        }

        public void equipArmor(string loadoutID, string armorID) {
            var loadout = getLoadout(loadoutID);
            loadout.armorID = armorID;
            saveLoadout(loadout);
        }

        public void equipUtility(string loadoutID, string utilityID) {
            var loadout = getLoadout(loadoutID);
            loadout.utilityID = utilityID;
            saveLoadout(loadout);
        }

        public void unequip(string loadoutID, EquipmentSlot slot) {
            var loadout = getLoadout(loadoutID);

            switch (slot) {
                case EquipmentSlot.Weapon:
                    loadout.weaponID = "";
                    break;
                case EquipmentSlot.Armor:
                    loadout.armorID = "";
                    break;
                case EquipmentSlot.Utility:
                    loadout.utilityID = "";
                    break;
            }

            saveLoadout(loadout);
        }

        // MARK: - Queries

        public EquipmentData? getWeapon(string loadoutID) {
            var loadout = getLoadout(loadoutID);
            return loadout.hasWeapon ? getEquipment(loadout.weaponID) : null;
        }

        public EquipmentData? getArmor(string loadoutID) {
            var loadout = getLoadout(loadoutID);
            return loadout.hasArmor ? getEquipment(loadout.armorID) : null;
        }

        public EquipmentData? getUtility(string loadoutID) {
            var loadout = getLoadout(loadoutID);
            return loadout.hasUtility ? getEquipment(loadout.utilityID) : null;
        }

    }

}
```

The loadout manager is a plain C# class, not a singleton — it can live on `GameStateManager` or wherever makes sense for your scene setup. Equipment is registered once (from data files), and loadouts are created/modified as units are equipped.

## Step 3.5: How Loadouts Connect

```
UnitData.loadoutID ──► LoadoutManager.getLoadout()
                              │
                              ├── weaponID ──► EquipmentData (stats)
                              ├── armorID  ──► EquipmentData (stats)
                              └── utilityID ──► EquipmentData (stats)
                                                      │
                                                      ▼
                                              Combat System (Phase 13)
                                              reads statModifiers
```

The data layer stores IDs. The loadout manager resolves them to equipment data. The combat system reads the stats. Each layer only knows about the one below it.

**Checkpoint:** Create these files and verify they compile. You should be able to register equipment, create loadouts, and assign them to units via `loadoutID`.
