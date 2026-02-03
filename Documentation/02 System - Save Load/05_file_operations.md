# Part 3: File Operations Deep Dive

This section explains the file I/O patterns used in the SaveManager.

## Unity's JsonUtility

Unity provides `JsonUtility` for serialization:

```csharp
// Serialize to JSON string
string json = JsonUtility.ToJson(saveData, prettyPrint: true);

// Deserialize from JSON string
SaveData loaded = JsonUtility.FromJson<SaveData>(json);
```

### Limitations of JsonUtility

| Supported | Not Supported |
|-----------|---------------|
| Public fields | Private fields |
| `[Serializable]` classes | Properties |
| Structs | Dictionaries |
| Lists | Polymorphism |
| Primitive types | Interfaces |

**Important:** All state classes use public fields specifically for `JsonUtility` compatibility.

### Dictionary Workaround

The `ResourceState` class uses a `Dictionary<ResourceType, int>`, but JsonUtility doesn't serialize dictionaries. The workaround is to convert to/from lists:

```csharp
[Serializable]
public class ResourceState {

    // This won't serialize directly
    [NonSerialized]
    private Dictionary<ResourceType, int> resourceDict;

    // Serialize as parallel lists instead
    public List<ResourceType> resourceKeys;
    public List<int> resourceValues;

    public void prepareForSerialization() {
        resourceKeys = new List<ResourceType>(resourceDict.Keys);
        resourceValues = new List<int>(resourceDict.Values);
    }

    public void restoreAfterDeserialization() {
        resourceDict = new Dictionary<ResourceType, int>();
        for (int i = 0; i < resourceKeys.Count; i++) {
            resourceDict[resourceKeys[i]] = resourceValues[i];
        }
    }

}
```

**Alternative:** If your ResourceState uses an enum with fixed values, consider using individual fields instead:

```csharp
public int currency;
public int alloys;
public int techComponents;
public int intel;
```

## File Paths

### Application.persistentDataPath

This is the correct location for save files:
- Survives app updates
- User-writable on all platforms
- Platform-specific location handled by Unity

```csharp
// Example paths:
// Windows: C:\Users\<user>\AppData\LocalLow\<company>\<product>\
// macOS: ~/Library/Application Support/<company>/<product>/
// Linux: ~/.config/unity3d/<company>/<product>/
```

### Path.Combine

Always use `Path.Combine` for cross-platform compatibility:

```csharp
// Good - handles path separators correctly
string path = Path.Combine(Application.persistentDataPath, "Saves", "slot_0.json");

// Bad - hardcoded separator
string path = Application.persistentDataPath + "/Saves/slot_0.json";
```

## Error Handling

The SaveManager uses try-catch blocks for all file operations:

```csharp
try {
    File.WriteAllText(filePath, json);
    onSaveCompleted?.Invoke(slotIndex);
    return true;
} catch (Exception e) {
    Debug.LogError($"Failed to save: {e.Message}");
    onSaveError?.Invoke(slotIndex, e.Message);
    return false;
}
```

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| `UnauthorizedAccessException` | File is read-only or in use | Check file permissions |
| `DirectoryNotFoundException` | Saves folder doesn't exist | Call `ensureSaveFolderExists()` |
| `IOException` | Disk full or file locked | Show user-friendly error |
| `JsonException` | Corrupted save file | Delete and start fresh |

## Synchronous vs Asynchronous I/O

The current implementation uses synchronous file operations:

```csharp
File.WriteAllText(filePath, json);  // Blocks until complete
```

For small save files (< 1MB), this is fine. For larger files or mobile platforms, consider async:

```csharp
public async Task<bool> saveAsync(GameState state, int slotIndex) {
    try {
        SaveData data = new SaveData(state, slotIndex);
        string json = JsonUtility.ToJson(data, true);
        string filePath = getFilePath(slotIndex);

        await File.WriteAllTextAsync(filePath, json);

        onSaveCompleted?.Invoke(slotIndex);
        return true;
    } catch (Exception e) {
        onSaveError?.Invoke(slotIndex, e.Message);
        return false;
    }
}
```

**Note:** Async file I/O requires careful consideration of Unity's main thread requirements for UI updates.

## Debugging Save Files

During development, you can manually inspect save files:

1. Find the save folder:
   ```csharp
   Debug.Log(Application.persistentDataPath);
   ```

2. Open the JSON file in any text editor

3. Modify values for testing

4. Load the modified save in-game

The pretty-printed JSON makes this easy:
```json
{
    "saveVersion": 1,
    "timestamp": "2024-01-15 14:30:00",
    "slotIndex": 0,
    "gameState": {
        "version": 1,
        "campaign": {
            "currentAct": 0,
            "elapsedTime": 120,
            "currentFaction": 1
        }
    }
}
```
