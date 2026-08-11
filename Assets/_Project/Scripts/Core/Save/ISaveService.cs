using System;

public interface ISaveService
{
    SaveData Data { get; }
    bool HasSaveFile { get; }

    /// <summary>Raised right after a save has been successfully written to disk. Use this to drive UI feedback.</summary>
    event Action OnSaved;

    void Load();
    void Save();
    void ResetSave();

    bool IsLevelCompleted(string sceneName, int levelIndex);
    void MarkLevelCompleted(string sceneName, int levelIndex);

    /// <summary>Sets both the current scene and the local level index within it in one write.</summary>
    void SetCurrentProgress(string sceneName, int levelIndex);

    bool IsActionUnlocked(LockableActionsManager.LockableActionType action);
    void SetActionUnlocked(LockableActionsManager.LockableActionType action, bool unlocked);

    string GetCustomValue(string key, string defaultValue = null);
    void SetCustomValue(string key, string value);
}
