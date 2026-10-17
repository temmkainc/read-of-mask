using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// An ordered chain of ObligatorySteps for a level (e.g. the whole "wake up -> hallway ->
/// bathroom -> kitchen -> exit" morning routine). Only one step is active at a time;
/// completing it activates the next.
///
/// A wrong action anywhere in the sequence restarts the WHOLE level in-place (via
/// LevelManager.RestartCurrentLevel -> LevelBase.RestartLevel) - like restarting the level, but
/// without an actual scene reload - then restarts the sequence from its first step. Each level
/// defines what "restarting" means for itself (e.g. Level06 re-triggers its bed interaction),
/// so this sequence doesn't need to track/restore arbitrary object state itself.
///
/// The restart is played as a cinematic rollback (see EffectsContainer): tension builds while the
/// correction line plays, then a hit and white-out, and the level resets while the screen is white.
/// The active step's objective text is published to the ObjectiveTracker for on-screen display.
/// </summary>
public class ObligatorySequence : MonoBehaviour
{
    [SerializeField] private List<ObligatoryStep> _steps = new();

    [Inject] private LevelManager _levelManager;
    [Inject] private EffectsContainer _effectsContainer;
    [InjectOptional] private ObjectiveTracker _objectives;

    private int _currentIndex = -1;

    public event Action OnSequenceComplete;

    public ObligatoryStep CurrentStep =>
        (_currentIndex >= 0 && _currentIndex < _steps.Count) ? _steps[_currentIndex] : null;

    /// <summary>Starts the sequence from its first step.</summary>
    public void StartSequence()
    {
        _currentIndex = -1;
        AdvanceToNextStep();
    }

    private void AdvanceToNextStep()
    {
        UnsubscribeFromCurrent();

        _currentIndex++;

        if (_currentIndex >= _steps.Count)
        {
            _objectives?.Clear();
            OnSequenceComplete?.Invoke();
            return;
        }

        var step = _steps[_currentIndex];
        step.OnStepCompleted += HandleStepCompleted;
        step.OnWrongActionStarted += HandleWrongActionStarted;
        step.OnWrongAction += HandleWrongAction;
        _objectives?.Set(step.ObjectiveText);
        step.Activate();
    }

    private void HandleStepCompleted()
    {
        AdvanceToNextStep();
    }

    private void HandleWrongActionStarted()
    {
        if (_effectsContainer != null && CurrentStep != null)
            _effectsContainer.BeginRollbackTension(CurrentStep.CorrectionDelay);
    }

    private void HandleWrongAction()
    {
        RestartSequence();
    }

    /// <summary>Restarts the whole level in-place and restarts this sequence from its first step.</summary>
    private void RestartSequence()
    {
        UnsubscribeFromCurrent();
        CurrentStep?.ResetState();

        if (_effectsContainer != null)
            _effectsContainer.PlayRollback(ResetLevelAndRestart);
        else
            ResetLevelAndRestart();
    }

    // Runs at full white during the rollback, so the reset itself is never seen.
    private void ResetLevelAndRestart()
    {
        if (this == null)
            return;

        ResetAllGatedDoors();
        _levelManager?.RestartCurrentLevel();

        _currentIndex = -1;
        AdvanceToNextStep();
    }

    /// <summary>Resets every ObligatoryStepGate-wrapped door in this level back to closed, so a
    /// door opened earlier in this attempt doesn't stay visually (and logically) open across a
    /// restart caused by a later wrong action.</summary>
    private void ResetAllGatedDoors()
    {
        var level = GetComponentInParent<LevelBase>();
        if (level == null)
            return;

        var gates = level.GetComponentsInChildren<ObligatoryStepGate>(true);
        foreach (var gate in gates)
        {
            if (gate != null)
                gate.ResetWrappedDoorState();
        }
    }

    private void UnsubscribeFromCurrent()
    {
        if (CurrentStep == null)
            return;

        CurrentStep.OnStepCompleted -= HandleStepCompleted;
        CurrentStep.OnWrongActionStarted -= HandleWrongActionStarted;
        CurrentStep.OnWrongAction -= HandleWrongAction;
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrent();
    }
}
