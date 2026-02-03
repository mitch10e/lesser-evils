# Folder Structure

## New Files to Create

This phase adds the following files to your project:

```
Assets/Scripts/Core/
├── Data/
│   ├── Constants/
│   │   └── SaveConstants.cs      <- NEW: Save system constants
│   └── Structs/
│       └── SaveSlotInfo.cs       <- NEW: Slot metadata for UI
├── SaveSystem/                    <- NEW FOLDER
│   ├── SaveData.cs               <- Save file wrapper
│   ├── SaveManager.cs            <- Main save/load singleton
│   └── VersionMigrator.cs        <- Handles old save versions
└── GameState/
    └── GameStateManager.cs       <- MODIFIED: Add save/load methods
```

## Runtime Save Location

Save files are stored in Unity's persistent data path:

| Platform | Location |
|----------|----------|
| Windows | `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\Saves\` |
| macOS | `~/Library/Application Support/<Company>/<Product>/Saves/` |
| Linux | `~/.config/unity3d/<Company>/<Product>/Saves/` |

Access this path in code via:
```csharp
string savePath = Path.Combine(Application.persistentDataPath, "Saves");
```

## Save File Naming

| Slot | Filename |
|------|----------|
| Slot 0 | `slot_0.json` |
| Slot 1 | `slot_1.json` |
| Slot 2 | `slot_2.json` |
| Auto-save | `autosave.json` |

## Create the Folder

In Unity:
1. Right-click `Assets/Scripts/Core/`
2. Create > Folder
3. Name it `SaveSystem`

Or create via script - the folder will be created automatically when you save the first file.
