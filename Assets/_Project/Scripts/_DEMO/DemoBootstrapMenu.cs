using DG.Tweening;
using System;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public class DemoBootstrapMenu : LookCloserInteractableBase
{
    [SerializeField] protected List<InGameMenuItemBase> _menuItems;
    [Inject] protected InputManager _inputManager;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration = 0.6f;
    [SerializeField] private string _sceneName;
    [SerializeField] private AudioSource _musicAudioSource;

    protected InputAction _directionInputAction;
    protected InputAction _actionInputAction;

    protected InGameMenu _menu;

    private const int MAX_VISIBLE_ITEMS = 4;

    private void Awake()
    {
        _directionInputAction = _inputManager.LookCloserDirectionAction;
        _actionInputAction = _inputManager.LookCloserActionAction;
        _directionInputAction.Enable();
        _actionInputAction.Enable();
        InitializeMenus();
        AudioManager.Instance.PlayMusicForSource(MusicTracks.PongMusic, _musicAudioSource);
    }

    private void InitializeMenus()
    {
        _menu = new InGameMenu(
            _menuItems.ConvertAll(x => (IInGameMenuItem)x),
            _directionInputAction,
            _actionInputAction,
            MAX_VISIBLE_ITEMS
        );

        _menu.OnItemSubmitted += OnMenuButtonSelected;
    }

    private void OnMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: StartGame(); break;
            case 1: CloseGame(); break;
        }
    }

    public override void Interact(Player player = null)
    {
        gameObject.SetActive(true);
        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.LookCloser)).Execute();
        _menu.EnterMenu();
    }

    private void CloseGame()
    {
        Application.Quit();
    }
    private async void StartGame()
    {
        _menu.Dispose();

        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;

        _fadeImage.gameObject.SetActive(true);

        _fadeImage.color = new Color(
            _fadeImage.color.r,
            _fadeImage.color.g,
            _fadeImage.color.b,
            0f
        );

        float t = 0f;

        var fadeTween = DOTween.To(() => t, x => t = x, 1f, _fadeDuration)
            .SetEase(Ease.InOutQuad);

        _fadeImage.DOFade(1f, _fadeDuration)
            .SetEase(Ease.InOutQuad);

        _musicAudioSource.DOFade(0f, _fadeDuration)
            .SetEase(Ease.InOutQuad);

        await fadeTween.AsyncWaitForCompletion();

        SceneManager.LoadScene(_sceneName);
    }
}
