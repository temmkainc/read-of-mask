using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int SaveVersion = 1;

    // Progress
    public int CurrentLevelIndex = 0;
    public List<int> CompletedLevelIndices = new List<int>();

    // Unlocked systems (Diary, Mask, ...)
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
