using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class GamingScreen : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private CanvasGroup _turnedOnScreenCanvasGroup;
    [SerializeField] private float _turnOnDuration = 0.1f;
    [SerializeField] private float _turnOffDuration = 0.1f;

    [Inject] InteractionCinemachineCamera _interactionCamera;
    [Inject] ICommandBus _commandBus;
    [Inject] IPlayerStateManager _playerStateManager;

    private PlayerStateType _previousPlayerStateType;

    [field: SerializeField] public InteractionLookPoint CameraSnapPoint { get; private set; }

    public bool HighlightWhenHolding => false;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding) 
            return;

        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.Gaming)).Execute();
    }

    private void On_PlayerStateChanged(PlayerStateType type)
    {
        if (type == PlayerStateType.Gaming)
        {
            CameraSnapPoint.SetActive(true);
            On_EnterInteractionState().Forget();
        }
        else if (_previousPlayerStateType == PlayerStateType.Gaming)
        {
            On_ExitInteractionState().Forget();
        }

        _previousPlayerStateType = type;
    }

    private async UniTask On_ExitInteractionState()
    {
        await _turnedOnScreenCanvasGroup
            .DOFade(1.2f, 0.05f)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();

        await _turnedOnScreenCanvasGroup
            .DOFade(0f, _turnOffDuration)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();

        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
    }

    private async UniTask On_EnterInteractionState()
    {
        _turnedOnScreenCanvasGroup.alpha = 0f;

        await _turnedOnScreenCanvasGroup
            .DOFade(1f, _turnOnDuration)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();
    }
}