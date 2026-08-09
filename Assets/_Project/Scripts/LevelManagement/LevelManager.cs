using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LevelManager : IInitializable, IDisposable
{
    private readonly List<LevelBase> _levels;
    private readonly ISaveService _saveService;
    private int _currentIndex = -1;
    private LevelBase CurrentLevel => IsValidIndex(_currentIndex) ? _levels[_currentIndex] : null;

    public int CurrentLevelIndex => _currentIndex;
    
    private bool _initializeOnStart;

    public LevelManager(List<LevelBase> levels, bool initializeOnStart, ISaveService saveService)
    {
        _levels = levels;
        _initializeOnStart = initializeOnStart;
        _saveService = saveService;
    }

    public void Dispose()
    {
        if (CurrentLevel == null)
            return;

        CurrentLevel.OnLevelComplete -= On_CurrentLevelComplete;
    }

    public void Initialize()
    {
       if(!_initializeOnStart)
           return;

        int startIndex = 0;
        int savedIndex = _saveService.Data.CurrentLevelIndex;
        if (IsValidIndex(savedIndex))
            startIndex = savedIndex;

        // Levels before the resume point are already done - restore their finished state
        // (kept objects on, destroyed objects gone) instead of just switching them off.
        RestoreCompletedLevels(startIndex);

        // Treat entering the resumed level the same as a normal transition from the level
        // right before it, so a destroy-previous-level flag on it behaves consistently
        // whether reached by playing through or by loading a save.
        _currentIndex = startIndex - 1;
        GoToLevel(startIndex);
        TurnOffLevelsAfter(startIndex);

        // Only place the player on load (fresh start or resumed save) - never on normal progression.
        CurrentLevel.SpawnPlayerAtSpawnPoint();

        // We're spawning straight into this level, not walking into it - its own in-level
        // destroy triggers (e.g. DeactivateTriggerLine) shouldn't fire just because we loaded here.
        CurrentLevel.DisableTriggerLines();
    }

    public void GoToLevel(int targetIndex)
    {
        if (!IsValidIndex(targetIndex))
          return;

        int previousIndex = _currentIndex;

        _currentIndex = targetIndex;
        CurrentLevel.Begin();
        CurrentLevel.OnLevelComplete += On_CurrentLevelComplete;

        _saveService.SetCurrentLevel(targetIndex);

        if (CurrentLevel.DestroyPreviousLevelOnEnter)
            DestroyLevel(previousIndex);
    }

    public void GoToNextLevel()
    {
        if (_currentIndex >= _levels.Count - 1)
            return;
        
        CurrentLevel.OnLevelComplete -= On_CurrentLevelComplete;

        GoToLevel(_currentIndex + 1);
    }

    public bool IsLevelCompleted(int index) => _saveService.IsLevelCompleted(index);

    private void On_CurrentLevelComplete()
    {
        _saveService.MarkLevelCompleted(_currentIndex);
        GoToNextLevel();
    }

    private void RestoreCompletedLevels(int beforeIndex)
    {
        for (int i = 0; i < beforeIndex; i++)
        {
            if (!IsValidIndex(i))
                continue;

            var level = _levels[i];
            if (level == null)
                continue; // already destroyed by an earlier level's "destroy previous" flag

            level.RestoreCompleted();

            if (level.DestroyPreviousLevelOnEnter)
                DestroyLevel(i - 1);
        }
    }

    private void DestroyLevel(int index)
    {
        if (!IsValidIndex(index))
            return;

        var level = _levels[index];
        if (level != null)
            UnityEngine.Object.Destroy(level.gameObject);
    }

    private void TurnOffLevelsAfter(int index)
    {
        for (int i = index + 1; i < _levels.Count; i++)
        {
            if (_levels[i] != null)
                _levels[i].gameObject.SetActive(false);
        }
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _levels.Count;
}
