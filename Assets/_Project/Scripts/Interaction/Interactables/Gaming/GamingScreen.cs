using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;   
using Zenject;

public class GamingScreen : LookCloserInteractableBase
{
    [SerializeField] private CanvasGroup _turnedOnScreenCanvasGroup;
    [SerializeField] private float _turnOnDuration = 0.1f;
    [SerializeField] private float _turnOffDuration = 0.1f;
    [SerializeField] private GamingCartridgeSlot _cartridgeSlot;
    [SerializeField] private Image _minigameLoadingBarImage;

    [Inject] private MinigameManager _minigameManager;
    private GamingScreenVisuals _loader;

    private CancellationTokenSource _loadingCts;

    private void Awake()
    {
        _loader = new GamingScreenVisuals(_minigameLoadingBarImage);
    }

    private void Start()
    {
        _cartridgeSlot.OnCartridgeInserted += On_CartridgeInserted;
        _cartridgeSlot.OnCartridgeEjected += On_CartridgeEjected;
    }

    private void On_CartridgeEjected()
    {
        On_CartridgeEjectedAsync().Forget();
    }

    private void On_CartridgeInserted(MinigameType minigameType)
    {
        On_CartridgeInsertedAsync().Forget();
        _minigameManager.EnterMinigame(minigameType);
    }

    public override void Interact(Player player)
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

        }
        else if (_previousPlayerStateType == PlayerStateType.Gaming)
        {
            _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
        }

        _previousPlayerStateType = type;
    }


    private async UniTask On_CartridgeInsertedAsync()
    {
        _loadingCts?.Cancel();
        _loadingCts = new CancellationTokenSource();

        _turnedOnScreenCanvasGroup.alpha = 0f;

        try
        {
            await _loader.SimulateLoadingAsync(_loadingCts.Token);
            await TurnScreenOn();
        }
        catch (OperationCanceledException)
        {
            // Do nothing — screen stays off
        }
    }
    private async UniTask On_CartridgeEjectedAsync()
    {
        _loadingCts?.Cancel();
        _loader.Reset();
        await TurnScreenOff();
    }

    private async UniTask TurnScreenOff()
    {
        await _turnedOnScreenCanvasGroup
            .DOFade(0f, _turnOffDuration)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();
    }

    private async UniTask TurnScreenOn()
    {
        await _turnedOnScreenCanvasGroup
            .DOFade(1f, _turnOnDuration)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();
    }
}