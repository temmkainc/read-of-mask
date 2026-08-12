using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Persists game progress (current scene + level, completed levels, unlocked systems like
/// Diary/Mask, unlocked diary letters, and any future checkpoint data) to a JSON file on disk
/// so it survives restarts.
/// </summary>
public class SaveService : ISaveService
{
    private const string SaveFileName = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private SaveData _data;

    public event Action OnSaved;

    public SaveData Data
    {
        get
        {
            if (_data == null)
                Load();
            return _data;
        }
    }

    public bool HasSaveFile => File.Exists(SavePath);

    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            else
            {
                _data = new SaveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Failed to load save file at '{SavePath}', starting a fresh save. {e}");
            _data = new SaveData();
        }
    }

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);

            string dir = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Write to a temp file first and then swap, so a crash mid-write can never corrupt the save.
            string tempPath = SavePath + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(SavePath))
                File.Delete(SavePath);
            File.Move(tempPath, SavePath);

            OnSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Failed to save game to '{SavePath}'. {e}");
        }
    }

    public void ResetSave()
    {
        _data = new SaveData();

        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Failed to delete save file at '{SavePath}'. {e}");
        }
    }

    private static string MakeLevelKey(string sceneName, int levelIndex) => $"{sceneName}:{levelIndex}";

    public bool IsLevelCompleted(string sceneName, int levelIndex) =>
        Data.CompletedLevelKeys.Contains(MakeLevelKey(sceneName, levelIndex));

    public void MarkLevelCompleted(string sceneName, int levelIndex)
    {
        string key = MakeLevelKey(sceneName, levelIndex);

        if (Data.CompletedLevelKeys.Contains(key))
            return;

        Data.CompletedLevelKeys.Add(key);
        Save();
    }

    public void SetCurrentProgress(string sceneName, int levelIndex)
    {
        if (Data.CurrentSceneName == sceneName && Data.CurrentLevelIndex == levelIndex)
            return;

        Data.CurrentSceneName = sceneName;
        Data.CurrentLevelIndex = levelIndex;
        Save();
    }

    public bool IsActionUnlocked(LockableActionsManager.LockableActionType action)
    {
        var entry = Data.UnlockedActions.FirstOrDefault(e => e.ActionName == action.ToString());
        return entry != null && entry.Unlocked;
    }

    public void SetActionUnlocked(LockableActionsManager.LockableActionType action, bool unlocked)
    {
        var entry = Data.UnlockedActions.FirstOrDefault(e => e.ActionName == action.ToString());

        if (entry != null && entry.Unlocked == unlocked)
            return;

        if (entry == null)
        {
            entry = new UnlockedActionEntry { ActionName = action.ToString(), Unlocked = unlocked };
            Data.UnlockedActions.Add(entry);
        }
        else
        {
            entry.Unlocked = unlocked;
        }

        Save();
    }

    public IReadOnlyCollection<char> GetUnlockedLetters()
    {
        return new HashSet<char>(Data.UnlockedLetters ?? string.Empty);
    }

    public void SetLetterUnlocked(char letter)
    {
        letter = char.ToUpperInvariant(letter);
        string current = Data.UnlockedLetters ?? string.Empty;

        if (current.IndexOf(letter) >= 0)
            return;

        Data.UnlockedLetters = current + letter;
        Save();
    }

    public string GetCustomValue(string key, string defaultValue = null)
    {
        var entry = Data.CustomData.FirstOrDefault(e => e.Key == key);
        return entry != null ? entry.Value : defaultValue;
    }

    public void SetCustomValue(string key, string value)
    {
        var entry = Data.CustomData.FirstOrDefault(e => e.Key == key);

        if (entry != null && entry.Value == value)
            return;

        if (entry == null)
        {
            entry = new KeyValueEntry { Key = key, Value = value };
            Data.CustomData.Add(entry);
        }
        else
        {
            entry.Value = value;
        }

        Save();
    }
}
