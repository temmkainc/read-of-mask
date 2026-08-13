using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class LevelBase : MonoBehaviour
{
    public event Action OnAllConditionsMet;
    public event Action OnLevelComplete;

    [Header("On Level Complete")]
    [Tooltip("Objects that get turned ON and stay active once this level is completed (e.g. permanent scene changes).")]
    [SerializeField] private GameObject[] _objectsToKeepOnFinish;

    [FormerlySerializedAs("_objectsToDeactivateOnFinish")]
    [Tooltip("Objects that should end up destroyed once this level is completed (e.g. puzzle pieces, temporary items). " +
        "NOT destroyed here on live completion - that already happens via their own destroy trigger during gameplay at the right moment. " +
        "Only force-destroyed when restoring an already-completed level after a game reboot, since that trigger won't get replayed.")]
    [SerializeField] private GameObject[] _objectsToDestroyOnFinish;

    [SerializeField] private LevelDoor _levelDoor;
    [SerializeField] private LevelDoor _levelDoorToClose;

    [Header("Level Transition")]
    [Tooltip("If true, the PREVIOUS level's whole GameObject is destroyed immediately as soon as this level becomes current " +
        "(both during normal play and when restoring this level from a save) - instead of being left in the scene. " +
        "Use this for heavy levels you never need to show/revisit again.")]
    [SerializeField] private bool _destroyPreviousLevelOnEnter;

    [Header("Player Spawn")]
    [Tooltip("Where the player is placed when the game LOADS into this level - a fresh start or resuming a save. Not used for normal level-to-level progression in the same session.")]
    [SerializeField] private Transform _spawnPoint;

    [Inject] private PlayerController _playerController;

    [Header("Obligatory Sequence")]
    [Tooltip("Optional. If this level has a forced action-by-action routine (e.g. the wake-up morning sequence), assign its ObligatorySequence here. It starts automatically when this level begins.")]
    [SerializeField] private ObligatorySequence _obligatorySequence;

    private List<ILevelCompleteCondition> _conditions = new();
    private int _conditionsMet;

    public bool DestroyPreviousLevelOnEnter => _destroyPreviousLevelOnEnter;

    /// <summary>This level's obligatory sequence, if it has one. Null for most levels.</summary>
    public ObligatorySequence ObligatorySequence => _obligatorySequence;

    /// <summary>
    /// Override and return true if this level already plays its own screen transition on Begin()
    /// (e.g. Level00's eye-open intro), so LevelManager doesn't also play the generic load fade on top of it.
    /// </summary>
    public virtual bool HasCustomLoadTransition => false;


    private void Awake()
    {
        GetComponents(_conditions);
    }

    public virtual void Begin()
    {
        gameObject.SetActive(true);
        SubscribeToConditions();

        if (_obligatorySequence != null)
            _obligatorySequence.StartSequence();
    }

    /// <summary>
    /// Restarts this level in-place, without a scene reload - used e.g. when an ObligatorySequence
    /// needs to reset the whole routine after a wrong action. Base behavior just teleports the
    /// player back to the spawn point; override to also replay any level-specific intro/setup
    /// (e.g. Level06 re-triggering its bed interaction).
    /// </summary>
    public virtual void RestartLevel()
    {
        SpawnPlayerAtSpawnPoint();
    }

    /// <summary>
    /// Teleports the player to this level's spawn point. Call only when the game is loading
    /// (fresh start or resuming a save) - never on normal level-to-level progression.
    /// </summary>
    public void SpawnPlayerAtSpawnPoint()
    {
        if (_spawnPoint == null || _playerController == null)
            return;

        _playerController.Teleport(_spawnPoint.position, _spawnPoint.rotation);

        // The teleport itself is a big instant jump - without this, any DeactivateTriggerLine
        // in this level could mistake that jump for the player having walked across it.
        // This only resyncs tracking, it does NOT disable the line - it still fires normally
        // if the player genuinely walks across it later this session.
        ResyncTriggerLines();
    }

    private void ResyncTriggerLines()
    {
        var triggerLines = GetComponentsInChildren<DeactivateTriggerLine>(true);
        foreach (var line in triggerLines)
        {
            if (line != null)
                line.ResyncPlayerPosition();
        }
    }

    /// <summary>
    /// Restores this level to its "already completed" visual state without replaying it -
    /// used when resuming a save at a later level, so earlier levels look finished:
    /// kept objects on, and objects that would've been destroyed by gameplay triggers
    /// are force-destroyed here instead, since those triggers won't fire again.
    /// Any in-level trigger lines (e.g. DeactivateTriggerLine) are disabled too, since this
    /// level isn't actually being played - it's just being shown in its finished state.
    /// </summary>
    public void RestoreCompleted()
    {
        gameObject.SetActive(true);
        ApplyCompletionState(destroyMarkedObjects: true);
        DisableTriggerLines();
    }

    protected virtual async UniTask BeforeCompleteAsync()
    {
        await UniTask.Yield();
        UnsubscribeFromConditions();

        // Don't force-destroy here - during live gameplay those objects are destroyed by
        // their own dedicated trigger at the right moment, not immediately on completion.
        ApplyCompletionState(destroyMarkedObjects: false);
    }

    public void DeactivateLevel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Disables every DeactivateTriggerLine (and similar in-level destroy triggers) under this
    /// level, so they can never fire. Only used for previously-completed levels being restored -
    /// NOT for the level you're actively resuming into, since its trigger lines still need to
    /// work normally for the rest of this session (see SpawnPlayerAtSpawnPoint/ResyncTriggerLines).
    /// </summary>
    public void DisableTriggerLines()
    {
        var triggerLines = GetComponentsInChildren<DeactivateTriggerLine>(true);
        foreach (var line in triggerLines)
        {
            if (line != null)
                line.enabled = false;
        }
    }

    private void ApplyCompletionState(bool destroyMarkedObjects)
    {
        foreach (var obj in _objectsToKeepOnFinish)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (!destroyMarkedObjects)
            return;

        foreach (var obj in _objectsToDestroyOnFinish)
        {
            if (obj != null)
                Destroy(obj);
        }
    }

    private void SubscribeToConditions()
    {
        _conditionsMet = 0;
        foreach (var condition in _conditions)
        {
            condition.OnConditionMet += On_ConditionMet;
        }
    }

    private void UnsubscribeFromConditions()
    {
        foreach (var condition in _conditions)
        {
            condition.OnConditionMet -= On_ConditionMet;
        }
    }

    private void On_ConditionMet()
    {
        _conditionsMet++;
        if (_conditionsMet < _conditions.Count)
            return;
        OnAllConditionsMet?.Invoke();
        RunCompleteSequence().Forget();
    }
    
    private async UniTask RunCompleteSequence()
    {
        await BeforeCompleteAsync();
        Complete();
    }

    protected virtual void Complete()
    {
        OnLevelComplete?.Invoke();

        if (_levelDoor == null)
            return;

        _levelDoor.Open();
    }
}
