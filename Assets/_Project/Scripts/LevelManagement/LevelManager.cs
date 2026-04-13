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
    
    private bool _initializeOnStart;

    public LevelManager(List<LevelBase> levels, bool initializeOnStart)
    {
        _levels = levels;
        _initializeOnStart = initializeOnStart;
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
        _currentIndex = 0;
        GoToLevel(_currentIndex);
        TurnOffLevelsExcept(_currentIndex);
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

    private void TurnOffLevelsExcept(int index)
    {
        for (int i = 0; i < _levels.Count; i++)
        {
            if (i != index)
            {
                _levels[i].gameObject.SetActive(false);
            }
        }
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _levels.Count;
}
