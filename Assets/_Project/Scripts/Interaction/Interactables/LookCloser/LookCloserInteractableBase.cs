using UnityEngine;
using Zenject;

public class LookCloserInteractableBase : MonoBehaviour, IInteractable, IHighlightable
{
    [field: SerializeField] public InteractionLookPoint CameraSnapPoint { get; private set; }

    [Inject] protected ICommandBus _commandBus;
    [Inject] protected IPlayerStateManager _playerStateManager;
    [Inject] protected InteractionCinemachineCamera _interactionCamera;

    protected PlayerStateType _previousPlayerStateType;

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding && _playerStateManager.CurrentStateType != PlayerStateType.LookCloser;

    public virtual void Interact(Player player)
    {
        if (player.Grabbing.IsHolding)
            return;

        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.LookCloser)).Execute();
    }

    private void On_PlayerStateChanged(PlayerStateType type)
    {
        if (type == PlayerStateType.LookCloser)
        {
            CameraSnapPoint.SetActive(true);
        }
        else if (_previousPlayerStateType == PlayerStateType.LookCloser)
        {
            _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
        }

        _previousPlayerStateType = type;
    }
}
