# Part 4: Version Migration

When you update your game and change the data structure, old save files need to be upgraded. This is where version migration comes in.

## Step 4.1: VersionMigrator Class

Create `Assets/Scripts/Core/SaveSystem/VersionMigrator.cs`:

```csharp
using UnityEngine;
using Game.Core.Data;

namespace Game.Core.SaveSystem {

    public static class VersionMigrator {

        public static SaveData migrate(SaveData data) {
            int startVersion = data.saveVersion;

            while (data.saveVersion < SaveConstants.CURRENT_SAVE_VERSION) {
                data = migrateOneVersion(data);
            }

            if (startVersion != data.saveVersion) {
                Debug.Log($"Migrated save from version {startVersion} to {data.saveVersion}");
            }

            return data;
        }

        private static SaveData migrateOneVersion(SaveData data) {
            switch (data.saveVersion) {
                case 1:
                    return migrateV1ToV2(data);
                // Add future migrations here:
                // case 2:
                //     return migrateV2ToV3(data);
                default:
                    Debug.LogWarning($"Unknown save version: {data.saveVersion}");
                    return data;
            }
        }

        // MARK: - Migration Methods

        private static SaveData migrateV1ToV2(SaveData data) {
            // Example migration: Add a new field introduced in v2
            // data.gameState.newField = defaultValue;

            data.saveVersion = 2;
            return data;
        }

        // Future migrations follow the same pattern:
        // private static SaveData migrateV2ToV3(SaveData data) {
        //     // Handle v2 -> v3 changes
        //     data.saveVersion = 3;
        //     return data;
        // }

    }

}
```

## How Migration Works

1. `SaveManager.load()` deserializes the JSON into `SaveData`
2. Checks `data.needsMigration()` (compares saveVersion to current)
3. Calls `VersionMigrator.migrate(data)`
4. Migrator walks through each version step-by-step

### Step-by-Step Migration

Migrations are applied one version at a time:
```
v1 -> v2 -> v3 -> v4 (current)
```

This ensures:
- Each migration only handles one set of changes
- Old saves from any version can be upgraded
- Migration logic is isolated and testable

## Example Migrations

### Adding a New Field

```csharp
private static SaveData migrateV1ToV2(SaveData data) {
    // New field added in v2: morale tracking
    // Old saves don't have it, so set a sensible default
    data.gameState.campaign.morale = 50;

    data.saveVersion = 2;
    return data;
}
```

### Renaming a Field

If you rename a field, JsonUtility won't find the old name. You need to:

1. Keep the old field temporarily
2. Copy data to the new field
3. Remove the old field in a later version

```csharp
private static SaveData migrateV2ToV3(SaveData data) {
    // Renamed: squad -> roster
    // The old 'squad' field is still in the JSON
    // but GameState now expects 'roster'

    // If you kept a deprecated field:
    // data.gameState.roster = data.gameState.deprecatedSquad;

    data.saveVersion = 3;
    return data;
}
```

### Restructuring Data

```csharp
private static SaveData migrateV3ToV4(SaveData data) {
    // Split resources into two separate states
    // Old: resources had both currency and materials
    // New: separate ResourceState and MaterialState

    // Copy material values from old location
    // data.gameState.materials.alloys = data.gameState.resources.deprecatedAlloys;

    data.saveVersion = 4;
    return data;
}
```

## When to Increment Version

Increment `CURRENT_SAVE_VERSION` when you:

| Change | Action |
|--------|--------|
| Add a new field | Increment version, add migration to set default |
| Remove a field | Increment version (old field ignored on load) |
| Rename a field | Increment version, add migration to copy data |
| Change field type | Increment version, add migration to convert |
| Add new state class | Increment version, add migration to initialize |

## Testing Migrations

1. Create a save file with the current version
2. Manually edit the JSON to set an older `saveVersion`
3. Remove any fields that didn't exist in that version
4. Load the save and verify migration works correctly

Example test save (simulating v1):
```json
{
    "saveVersion": 1,
    "timestamp": "2024-01-01 12:00:00",
    "slotIndex": 0,
    "gameState": {
        "version": 1,
        "campaign": {
            "currentAct": 0,
            "elapsedTime": 100
        }
    }
}
```

## Best Practices

1. **Never delete migration code** - Players might have saves from any version
2. **Document each migration** - Comment what changed and why
3. **Test migrations** - Create saves at each version for automated testing
4. **Keep migrations simple** - One version jump per method
5. **Set sensible defaults** - New fields should have reasonable initial values
