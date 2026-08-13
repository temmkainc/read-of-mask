using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class NagEntry
{
    [Tooltip("Seconds after this step becomes active (or after the previous nag fired) before this line plays.")]
    public float Delay = 5f;
    public AudioClip Voiceline;
    [TextArea]
    [Tooltip("Shown via Debug.Log for now (when Voiceline is unassigned), until a real voiceline is recorded.")]
    public string DebugText;
}

/// <summary>
/// One "do this specific thing right now" unit within an ObligatorySequence.
/// While active: nags the player with escalating voicelines if they stall. Once every nag entry
/// has played (i.e. the player stalled through the whole escalation), stalling itself counts as
/// a wrong action by default - same flash + full level restart as any other wrong action.
/// A wrong action always waits for its correction line to actually be heard before restarting.
/// </summary>
public class ObligatoryStep : MonoBehaviour
{
    [Header("Nagging")]
    [Tooltip("Escalating lines that play while this step sits incomplete, in order.")]
    [SerializeField] private List<NagEntry> _nagEntries = new();
    [SerializeField] private AudioSource _narratorAudioSource;

    [Tooltip("If true, stalling through every nag entry counts as a wrong action (triggers the same flash + full restart). If false, the last nag entry just repeats indefinitely with no consequence.")]
    [SerializeField] private bool _restartAfterStalling = true;
    [Tooltip("Seconds to wait after the last nag line plays before treating the stall as a wrong action (which itself then waits Correction Delay before restarting).")]
    [SerializeField] private float _stallRestartDelay = 2f;

    [Header("Expected Action")]
    [Tooltip("ID(s) that count as the correct action for this step (e.g. 'BathroomDoor'). An ObligatoryStepGate/ObligatoryStepTriggerGate with a matching Action Id calls Complete() automatically and lets its interaction through; any other Action Id gets blocked and reports a wrong action.")]
    [SerializeField] private List<string> _expectedActionIds = new();

    [Header("Correction")]
    [SerializeField] private AudioClip _correctionVoiceline;
    [TextArea]
    [Tooltip("Shown via Debug.Log for now (when Correction Voiceline is unassigned), until a real voiceline is recorded. Also used as the 'you stalled too long' line, if Restart After Stalling is on.")]
    [SerializeField] private string _correctionDebugText;
    [Tooltip("Seconds to wait after the correction line plays before the restart actually happens, so the player has time to hear it.")]
    [SerializeField] private float _correctionDelay = 2f;

    private CancellationTokenSource _nagCts;
    private CancellationTokenSource _wrongActionCts;
    private bool _isActive;
    private bool _isCompleted;

    public event Action OnStepCompleted;
    /// <summary>Fired (after the correction delay) when a wrong action is reported for this step. The owning ObligatorySequence restarts the level.</summary>
    public event Action OnWrongAction;

    public bool IsActive => _isActive;

    /// <summary>True if this actionId is the correct one for this step right now.</summary>
    public bool IsExpectedAction(string actionId)
        => _isActive && !_isCompleted && !string.IsNullOrEmpty(actionId) && _expectedActionIds.Contains(actionId);

    /// <summary>Begins this step and starts the nag timer.</summary>
    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _isCompleted = false;
        RestartNagTimer();
    }

    /// <summary>Marks this step done. Stops nagging and fires OnStepCompleted.</summary>
    public void Complete()
    {
        if (_isCompleted)
            return;

        _isCompleted = true;
        _isActive = false;
        StopNagTimer();
        OnStepCompleted?.Invoke();
    }

    /// <summary>Resets this step back to inactive without firing any events - used when the whole sequence restarts from a wrong action.</summary>
    public void ResetState()
    {
        StopNagTimer();
        StopWrongActionDelay();
        _isActive = false;
        _isCompleted = false;
    }

    /// <summary>
    /// Call this when the player does something other than the required action for this step
    /// (e.g. tries a locked door meant for later), or when they've stalled too long. Plays this
    /// step's correction line, waits Correction Delay seconds so it can actually be heard, then
    /// reports the wrong action upward - the owning ObligatorySequence restarts the level.
    /// </summary>
    public void TriggerWrongAction()
    {
        if (!_isActive || _isCompleted)
            return;

        // Mark inactive immediately so a second wrong action during the delay window is ignored
        // rather than double-triggering a restart.
        _isActive = false;
        StopNagTimer();

        StopWrongActionDelay();
        _wrongActionCts = new CancellationTokenSource();
        TriggerWrongActionAsync(_wrongActionCts.Token).Forget();
    }

    private async UniTaskVoid TriggerWrongActionAsync(CancellationToken ct)
    {
        PlayCorrectionLine();

        await UniTask.Delay(TimeSpan.FromSeconds(_correctionDelay), cancellationToken: ct).SuppressCancellationThrow();
        if (ct.IsCancellationRequested)
            return;

        OnWrongAction?.Invoke();
    }

    private void StopWrongActionDelay()
    {
        _wrongActionCts?.Cancel();
        _wrongActionCts?.Dispose();
        _wrongActionCts = null;
    }

    private void PlayCorrectionLine()
    {
        if (_correctionVoiceline != null && _narratorAudioSource != null)
            _narratorAudioSource.PlayOneShot(_correctionVoiceline);
        else if (!string.IsNullOrEmpty(_correctionDebugText))
            Debug.Log($"[Narrator] {_correctionDebugText}");
    }

    private void RestartNagTimer()
    {
        StopNagTimer();
        _nagCts = new CancellationTokenSource();
        RunNagTimeline(_nagCts.Token).Forget();
    }

    private void StopNagTimer()
    {
        _nagCts?.Cancel();
        _nagCts?.Dispose();
        _nagCts = null;
    }

    private async UniTaskVoid RunNagTimeline(CancellationToken ct)
    {
        foreach (var nag in _nagEntries)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(nag.Delay), cancellationToken: ct).SuppressCancellationThrow();
            if (ct.IsCancellationRequested)
                return;

            PlayNag(nag);
        }

        if (_nagEntries.Count == 0)
            return;

        if (_restartAfterStalling)
        {
            // Stalled through every configured nag - give the player a moment to actually hear
            // the last line, then treat it as a wrong action (which itself waits Correction
            // Delay for the correction line before actually restarting).
            await UniTask.Delay(TimeSpan.FromSeconds(_stallRestartDelay), cancellationToken: ct).SuppressCancellationThrow();
            if (ct.IsCancellationRequested)
                return;

            TriggerWrongAction();
            return;
        }

        // Otherwise, just keep repeating the last nag indefinitely with no consequence.
        var lastNag = _nagEntries[_nagEntries.Count - 1];
        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lastNag.Delay), cancellationToken: ct).SuppressCancellationThrow();
            if (ct.IsCancellationRequested)
                return;

            PlayNag(lastNag);
        }
    }

    private void PlayNag(NagEntry nag)
    {
        if (nag.Voiceline != null && _narratorAudioSource != null)
            _narratorAudioSource.PlayOneShot(nag.Voiceline);
        else if (!string.IsNullOrEmpty(nag.DebugText))
            Debug.Log($"[Narrator] {nag.DebugText}");
    }

    private void OnDisable()
    {
        StopNagTimer();
        StopWrongActionDelay();
    }
}
