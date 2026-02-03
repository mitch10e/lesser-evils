# Part 1: Setting Up the Folder Structure

Let's start by creating the folder structure for our scripts. Good organization now will save headaches later.

## Step 1.1: Create the Core Folders

1. In Unity's Project window, right-click on `Assets`
2. Create the following folder hierarchy:

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameState/
│   │   ├── States/
│   │   ├── Data/
│   │   │   ├── Enums/
│   │   │   ├── Structs/
│   │   │   └── Constants/
│   │   └── Events/
│   └── Editor/
│       └── GameState/
├── Data/
│   └── ScriptableObjects/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

**Tip:** You can create folders by right-clicking and selecting `Create > Folder`, or use `Ctrl+Shift+N` on Windows.

## Step 1.2: Set Up the Test Assembly

Unity requires special assembly definitions for tests.

1. Navigate to `Assets/Tests/EditMode`
2. Right-click and select `Create > Assembly Definition`
3. Name it `EditModeTests`
4. In the Inspector, check `Test Assemblies` and select `Editor` platform only
5. Under `Assembly Definition References`, add `UnityEngine.TestRunner` and `UnityEditor.TestRunner`

Repeat for `PlayMode`:
1. Navigate to `Assets/Tests/PlayMode`
2. Create an Assembly Definition named `PlayModeTests`
3. Check `Test Assemblies` and include all platforms
