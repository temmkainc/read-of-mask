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
    [SerializeField] private AudioSource _audioSource;

    [Inject] private MinigameManager _minigameManager;
    private GamingScreenVisuals _loader;

    private CancellationTokenSource _loadingCts;

    private const float SFX_VOLUME_SCALE = 0.4f;
    private const float LOADING_DURATION = 0.5f;
    private void Awake()
    {
        _loader = new GamingScreenVisuals(_minigameLoadingBarImage, LOADING_DURATION);
    }

    private void Start()
    {
        _cartridgeSlot.OnCartridgeInserted += On_CartridgeInserted;
        _cartridgeSlot.OnCartridgeEjected += On_CartridgeEjected;
    }

    private void On_CartridgeEjected()
    {
        On_CartridgeEjectedAsync().Forget();
        AudioManager.Instance.CurrentMinigamesSource = null;
    }

    private void On_CartridgeInserted(MinigameType minigameType)
    {
        On_CartridgeInsertedAsync(minigameType).Forget();
        AudioManager.Instance.CurrentMinigamesSource = _audioSource;
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

    protected override void On_PlayerStateChanged(PlayerStateType type)
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


    private async UniTask On_CartridgeInsertedAsync(MinigameType minigameType)
    {
        _loadingCts?.Cancel();
        _loadingCts = new CancellationTokenSource();

        _turnedOnScreenCanvasGroup.alpha = 0f;

        try
        {
            AudioManager.Instance.PlaySFX(SfxClips.CartridgeInsert, volumeScale: SFX_VOLUME_SCALE, source: _audioSource);
            AudioManager.Instance.PlaySFX(SfxClips.ConsoleLoading, volumeScale: SFX_VOLUME_SCALE, source: _audioSource);
            await _loader.SimulateLoadingAsync(_loadingCts.Token);
            await TurnScreenOn();

            var musicId = GetMusicIdForMinigame(minigameType);
            if (string.IsNullOrEmpty(musicId))
                return;

            AudioManager.Instance.PlayMusicForSource(musicId, _audioSource);
            _minigameManager.EnterMinigame(minigameType);
        }
        catch (OperationCanceledException)
        {

        }

    }
    private async UniTask On_CartridgeEjectedAsync()
    {
        _loadingCts?.Cancel();
        _loader.Reset();
        await TurnScreenOff();
        _minigameManager.ExitCurrentMinigame();
        await AudioManager.Instance.PlaySFXAsync(SfxClips.CartridgeEject, volumeScale: SFX_VOLUME_SCALE, source: _audioSource);
        AudioManager.Instance.StopMusicForSource(_audioSource);
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

    private string GetMusicIdForMinigame(MinigameType type)
    {
        return type switch
        {
            MinigameType.Pong => MusicTracks.PongMusic,
            //MinigameType.Platformer => MusicTracks.PlatformerMusic,
            //MinigameType.Puzzle => MusicTracks.PuzzleGameMusic,
            _ => null
        };
    }
}