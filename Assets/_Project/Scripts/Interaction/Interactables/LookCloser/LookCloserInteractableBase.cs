using System;
using UnityEngine;
using Zenject;

public class LookCloserInteractableBase : MonoBehaviour, IInteractable, IHighlightable
{
    [field: SerializeField] public InteractionLookPoint CameraSnapPoint { get; private set; }

    [Header("Hint")]
    [SerializeField] private string _hintLabel   = "Look closer";
    [SerializeField] private float  _hintXOffset = 0f;
    [SerializeField] private float  _hintYOffset = 0.5f;
    [SerializeField] private float  _hintZOffset = 0f;

    [Inject] protected ICommandBus _commandBus;
    [Inject] protected IPlayerStateManager _playerStateManager;
    [Inject] protected InteractionCinemachineCamera _interactionCamera;

    protected PlayerStateType _previousPlayerStateType;

    /// <summary>Fired specifically when the player exits LookCloser as a result of THIS interactable's own session (not any other LookCloser-based object, e.g. a chair or bench). Use this instead of the generic IPlayerStateManager.OnStateChanged when you need to know "did the player finish looking at this specific thing".</summary>
    public event Action OnExitedLookCloser;

    public bool IsEnabled { get; set; } = true;

    public string HintLabel   => _hintLabel;
    public float  HintXOffset => _hintXOffset;
    public float  HintYOffset => _hintYOffset;
    public float  HintZOffset => _hintZOffset;

    public virtual bool CanHighlight(PlayerGrabbing grabbing)
        => IsEnabled && !grabbing.IsHolding && _playerStateManager.CurrentStateType != PlayerStateType.LookCloser;

    public virtual void Interact(Player player = null)
    {
        if (!IsEnabled)
            return;

        // Guards against calling this before the Player (or its Grabbing component) has finished
        // initializing - e.g. a level's intro sequence calling Interact() on the very first frame.
        if (player == null || player.Grabbing == null)
        {
            Debug.LogWarning($"[{nameof(LookCloserInteractableBase)}] Interact() called on '{name}' with a null player or player not yet ready - ignoring.");
            return;
        }

        if (player.Grabbing.IsHolding)
            return;

        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.LookCloser)).Execute();
    }

    protected virtual void On_PlayerStateChanged(PlayerStateType type)
    {
        var previous = _previousPlayerStateType;
        _previousPlayerStateType = type;

        if (type == PlayerStateType.LookCloser)
        {
            CameraSnapPoint.SetActive(true);
        }
        else if (previous == PlayerStateType.LookCloser)
        {
            _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
            OnExitedLookCloser?.Invoke();
        }
    }
}
