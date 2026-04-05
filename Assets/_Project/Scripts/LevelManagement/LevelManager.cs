using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LevelManager : IInitializable, IDisposable
{
    private readonly List<LevelBase> _levels;
    private int _currentIndex = -1;
    private LevelBase CurrentLevel => IsValidIndex(_currentIndex) ? _levels[_currentIndex] : null;

    public int CurrentLevelIndex => _currentIndex;

    public LevelManager(List<LevelBase> levels)
    {
        _levels = levels;
    }

    public void Dispose()
    {
        if (CurrentLevel == null)
            return;

        CurrentLevel.OnLevelComplete -= On_CurrentLevelComplete;
    }

    public void Initialize()
    {
        GoToLevel(0);
    }

    public void GoToLevel(int targetIndex)
    {
        if (!IsValidIndex(targetIndex))
          return;
        
        _currentIndex = targetIndex;
        CurrentLevel.Begin();
        CurrentLevel.OnLevelComplete += On_CurrentLevelComplete;
    }

    public void GoToNextLevel()
    {
        if (_currentIndex >= _levels.Count - 1)
            return;
        
        CurrentLevel.OnLevelComplete -= On_CurrentLevelComplete;

        GoToLevel(_currentIndex + 1);
    }

    private void On_CurrentLevelComplete()
    {
        GoToNextLevel();
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _levels.Count;
}
