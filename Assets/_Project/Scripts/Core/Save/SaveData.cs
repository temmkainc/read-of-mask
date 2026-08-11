using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int SaveVersion = 2;

    // Which scene the player is currently in, and their level index within that scene.
    // Empty CurrentSceneName means either a brand new save, or a save from before multi-scene
    // support was added - both cases are treated as "assume the first/current scene" on load.
    public string CurrentSceneName = "";
    public int CurrentLevelIndex = 0;

    // Scene-qualified so levels in different scenes can't collide on the same local index,
    // e.g. "Playground:3" and "NextChapterScene:3" are tracked independently.
    public List<string> CompletedLevelKeys = new List<string>();

    // Unlocked systems (Diary, Mask, ...) - global across the whole game, not scene-specific.
    public List<UnlockedActionEntry> UnlockedActions = new List<UnlockedActionEntry>();

    // Generic bag for future data (e.g. checkpoint position/id) without needing schema changes.
    public List<KeyValueEntry> CustomData = new List<KeyValueEntry>();
}

[Serializable]
public class UnlockedActionEntry
{
    public string ActionName;
    public bool Unlocked;
}

[Serializable]
public class KeyValueEntry
{
    public string Key;
    public string Value;
}
