using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class LevelManager : IInitializable, IDisposable
{
    private readonly List<LevelBase> _levels;
    private readonly ISaveService _saveService;
    private readonly EffectsContainer _effectsContainer;
    private readonly string _nextSceneName;
    private string _thisSceneName;
    private int _currentIndex = -1;
    private LevelBase CurrentLevel => IsValidIndex(_currentIndex) ? _levels[_currentIndex] : null;

    public int CurrentLevelIndex => _currentIndex;

    /// <summary>The obligatory sequence belonging to whichever level is currently active, or null if that level doesn't have one.</summary>
    public ObligatorySequence CurrentObligatorySequence => CurrentLevel?.ObligatorySequence;
    
    private bool _initializeOnStart;

    public LevelManager(List<LevelBase> levels, bool initializeOnStart, ISaveService saveService, EffectsContainer effectsContainer, string nextSceneName)
    {
        _levels = levels;
        _initializeOnStart = initializeOnStart;
        _saveService = saveService;
        _effectsContainer = effectsContainer;
        _nextSceneName = nextSceneName;
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

        _thisSceneName = SceneManager.GetActiveScene().name;

        // Only trust the saved level index if the save actually points at THIS scene.
        // An empty saved scene name covers both a brand new save and an old save from before
        // multi-scene support - both are treated as "resume in whichever scene loads first".
        bool resumingThisScene = string.IsNullOrEmpty(_saveService.Data.CurrentSceneName)
            || _saveService.Data.CurrentSceneName == _thisSceneName;

        int startIndex = 0;
        if (resumingThisScene && IsValidIndex(_saveService.Data.CurrentLevelIndex))
            startIndex = _saveService.Data.CurrentLevelIndex;

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
        // This also resyncs (not disables) any DeactivateTriggerLine in this level, so the
        // teleport itself isn't mistaken for the player walking across it - the line still
        // fires normally later if genuinely crossed during this session.
        CurrentLevel.SpawnPlayerAtSpawnPoint();
        CurrentLevel.OnStartedFromLoad();

        // Smooth fade-in transition on load (e.g. coming from the menu), unless this level
        // already plays its own screen transition (like Level00's eye-open intro).
        if (!CurrentLevel.HasCustomLoadTransition)
            _effectsContainer.FadeFromWhite();
    }

    public void GoToLevel(int targetIndex)
    {
        if (!IsValidIndex(targetIndex))
          return;

        int previousIndex = _currentIndex;

        _currentIndex = targetIndex;
        CurrentLevel.Begin();
        CurrentLevel.OnLevelComplete += On_CurrentLevelComplete;

        _saveService.SetCurrentProgress(_thisSceneName, targetIndex);

        DestroyWhatLevelLeavesBehind(targetIndex, previousIndex);
    }

    public void GoToNextLevel()
    {
        if (_currentIndex < _levels.Count - 1)
        {
            CurrentLevel.OnLevelComplete -= On_CurrentLevelComplete;
            GoToLevel(_currentIndex + 1);
            return;
        }

        if (string.IsNullOrEmpty(_nextSceneName))
            return;

        _saveService.SetCurrentProgress(_nextSceneName, 0);
        SceneManager.LoadScene(_nextSceneName);
    }

    public bool IsLevelCompleted(int index) => _saveService.IsLevelCompleted(_thisSceneName, index);

    /// <summary>Restarts the currently active level in-place (see LevelBase.RestartLevel), without a scene reload.</summary>
    public void RestartCurrentLevel()
    {
        CurrentLevel?.RestartLevel();
    }

    private void On_CurrentLevelComplete()
    {
        _saveService.MarkLevelCompleted(_thisSceneName, _currentIndex);
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
            DestroyWhatLevelLeavesBehind(i, i - 1);
        }
    }

    // What a level that's just become current no longer needs: the level before it, or all of
    // them, and any other objects it lists (see LevelBase's Level Transition settings).
    private void DestroyWhatLevelLeavesBehind(int index, int previousIndex)
    {
        var level = _levels[index];

        if (level.DestroyAllPreviousLevelsOnEnter)
        {
            for (int i = 0; i < index; i++)
                DestroyLevel(i);
        }
        else if (level.DestroyPreviousLevelOnEnter)
        {
            DestroyLevel(previousIndex);
        }

        foreach (var obj in level.ObjectsToDestroyOnEnter)
            DestroyOutOfSight(obj);
    }

    private void DestroyLevel(int index)
    {
        if (IsValidIndex(index) && _levels[index] != null)
            DestroyOutOfSight(_levels[index].gameObject);
    }

    private static void DestroyOutOfSight(GameObject obj)
    {
        if (obj == null)
            return;

        // The dream gate that ended the level is still drawing the player through (the next
        // level begins as they step in) - destroy it once they're out, not in front of them.
        var gate = Array.Find(UnityEngine.Object.FindObjectsByType<DreamGate>(), g => g.IsTransitioning);
        if (gate == null)
        {
            UnityEngine.Object.Destroy(obj);
            return;
        }

        void On_GatePassed()
        {
            gate.OnPassed -= On_GatePassed;
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }
        gate.OnPassed += On_GatePassed;
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
