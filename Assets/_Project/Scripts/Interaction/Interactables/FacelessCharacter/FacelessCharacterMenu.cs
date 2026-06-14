using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public class FacelessCharacterMenu : LookCloserInteractableBase
{
    [SerializeField] protected List<InGameMenuItemBase> _menuItems;
    [SerializeField] protected List<InGameMenuItemBase> _optionsItems;
    [SerializeField] protected Transform _menuRoot;
    [SerializeField] protected Transform _optionsRoot;
    [SerializeField] private float _enterDelay = 0.2f;

    [SerializeField] private ValueInGameMenuItem _sensitivityMenuItem;
    [SerializeField] private ValueInGameMenuItem _masterVolumeMenuItem;
    [SerializeField] private ValueInGameMenuItem _musicVolumeMenuItem;
    [SerializeField] private CycleInGameMenuItem _fullScreenMenuItem;
    [SerializeField] private CycleInGameMenuItem _motionBlurMenuItem;
    [SerializeField] private CycleInGameMenuItem _resolutionScreenMenuItem;

    [Inject] protected InputManager _inputManager;
    [Inject] private PlayerFirstPersonHandsController _handsController;
    [Inject] private GameOptions _options;

    protected InputAction _directionInputAction;
    protected InputAction _actionInputAction;

    protected InGameMenu _menu;
    protected InGameMenu _optionsMenu;

    private bool _isInOptions = false;
    private const int MAX_VISIBLE_ITEMS = 5;

    private void Awake()
    {
        _directionInputAction = _inputManager.LookCloserDirectionAction;
        _actionInputAction = _inputManager.LookCloserActionAction;
        InitializeOptionsMenuItems();
        InitializeMenus();
    }

    private void OnOptionsButtonSelected(int index)
    {
        switch (index)
        {
            case 0: ToggleOptions(false); break;
            default: break;
        }
    }

    private void InitializeOptionsMenuItems()
    {
        _sensitivityMenuItem.Initialize(_options.Sensitivity,
            value => _options.Sensitivity = value);

        _masterVolumeMenuItem.Initialize(_options.MasterVolume,
            value => _options.MasterVolume = value);

        _musicVolumeMenuItem.Initialize(_options.MusicVolume,
            value => _options.MusicVolume = value);

        _motionBlurMenuItem.Initialize(
            new[] { "On", "Off" },
            _options.MotionBlur ? "On" : "Off",
            value => _options.MotionBlur = value == "On"
        );

        _fullScreenMenuItem.Initialize(
            new[] { "On", "Off" },
            _options.Fullscreen ? "On" : "Off",
            value => _options.Fullscreen = value == "On"
        );

        _resolutionScreenMenuItem.Initialize(
            new[] { "1280x720", "1920x1080" },
            _options.Resolution,
            value => _options.Resolution = value
        );
    }

    private void InitializeMenus()
    {
        _menu = new InGameMenu(
            _menuItems.ConvertAll(x => (IInGameMenuItem)x),
            _directionInputAction,
            _actionInputAction,
            MAX_VISIBLE_ITEMS
        );

        _optionsMenu = new InGameMenu(
            _optionsItems.ConvertAll(x => (IInGameMenuItem)x),
            _directionInputAction,
            _actionInputAction,
            MAX_VISIBLE_ITEMS
        );

        _menu.OnItemSubmitted += OnMenuButtonSelected;
        _menu.EnterMenu();
        _optionsMenu.OnItemSubmitted += OnOptionsButtonSelected;
    }

    private void OnMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: ContinueGame(); break;
            case 1: ToggleOptions(true); break;
            case 2: CloseGame(); break;
        }
    }

    public override void Interact(Player player = null)
    {
        gameObject.SetActive(true);
        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _handsController.ResetBlockingAnimation();
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.LookCloser)).Execute();
    }
    private void ToggleOptions(bool isEnter)
    {
        _isInOptions = isEnter;
        _menuRoot.gameObject.SetActive(!isEnter);
        _optionsRoot.gameObject.SetActive(isEnter);

        if (isEnter)
        {
            _menu.ExitMenu();
            _optionsMenu.EnterMenu();
            return;
        }

        _menu.EnterMenu();
        _optionsMenu.ExitMenu();
    }
    private void CloseGame()
    {
        Application.Quit();
    }
    private void ContinueGame()
    {
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.General)).Execute();
        ExitMenu();
    }
    public void EnterMenu()
    {
        DOVirtual.DelayedCall(_enterDelay, () =>
        {
            PlayEnterAnimation();
            Interact();
            if (_isInOptions)
            {
                _optionsMenu.EnterMenu();
            }
            else
            {
                _menu.EnterMenu();
            }
        });
    }

    private void ExitMenu()
    {
        transform.DOScaleY(0.05f, 0.2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _menu.ExitMenu(clearFocus: false);
                _optionsMenu.ExitMenu(clearFocus: false);
            });
    }
    private void PlayEnterAnimation()
    {
        transform.localScale = new Vector3(1f, 0.05f, 1f);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(0.2f);

        seq.Append(
            transform.DOScaleY(1f, 0.8f)
                .SetEase(Ease.OutCubic)
        );

        seq.Join(
            transform.DOScale(1.02f, 0.8f)
                .SetEase(Ease.OutSine)
        );

        seq.Append(
            transform.DOScale(1f, 0.4f)
                .SetEase(Ease.InOutSine)
        );
    }
    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        if (type != PlayerStateType.LookCloser)
        {
            ExitMenu();
            return;
        }

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
