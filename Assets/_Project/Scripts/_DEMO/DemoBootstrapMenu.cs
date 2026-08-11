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
    [Header("Main Menu")]
    [SerializeField] protected Transform _menuRoot;
    [SerializeField] protected List<InGameMenuItemBase> _menuItems;
    [Tooltip("The Play button item - its label is set to Continue/Play automatically based on whether a save exists.")]
    [SerializeField] protected ButtonInGameMenuItem _playButtonItem;
    [Tooltip("The New Game button item - hidden entirely when there's no save yet, since it has nothing to offer over Play in that case.")]
    [SerializeField] protected ButtonInGameMenuItem _newGameButtonItem;

    [Header("New Game Confirmation")]
    [Tooltip("Root panel for the 'Are you sure?' confirmation shown before erasing the save. Leave empty to skip confirmation and start immediately.")]
    [SerializeField] protected Transform _confirmRoot;
    [Tooltip("Two items expected: index 0 = Yes/confirm, index 1 = No/cancel.")]
    [SerializeField] protected List<InGameMenuItemBase> _confirmItems;

    [Inject] protected InputManager _inputManager;
    [Inject] private ISaveService _saveService;

    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration = 0.6f;
    [Tooltip("The scene a brand new game starts in - drag the actual scene asset here, not just its name, so renaming the scene later doesn't break this.")]
    [SerializeField] private SceneField _sceneName;
    [SerializeField] private AudioSource _musicAudioSource;

    protected InputAction _directionInputAction;
    protected InputAction _actionInputAction;

    protected InGameMenu _menu;
    protected InGameMenu _confirmMenu;
    private List<InGameMenuItemBase> _activeMenuItems;

    private const int MAX_VISIBLE_ITEMS = 4;

    private void Awake()
    {
        _directionInputAction = _inputManager.LookCloserDirectionAction;
        _actionInputAction = _inputManager.LookCloserActionAction;
        _directionInputAction.Enable();
        _actionInputAction.Enable();
        InitializeMenus();
        UpdatePlayButtonLabel();
        AudioManager.Instance.PlayMusicForSource(MusicTracks.PongMusic, _musicAudioSource);
    }

    private void UpdatePlayButtonLabel()
    {
        if (_playButtonItem == null) return;
        _playButtonItem.SetLabel(_saveService.HasSaveFile ? "Continue" : "Play");
    }

    private void OnDestroy()
    {
        _menu?.Dispose();
        _confirmMenu?.Dispose();
    }

    private void InitializeMenus()
    {
        bool hasSave = _saveService.HasSaveFile;

        _activeMenuItems = new List<InGameMenuItemBase>();
        foreach (var item in _menuItems)
        {
            bool isNewGameButton = _newGameButtonItem != null && item == (InGameMenuItemBase)_newGameButtonItem;
            if (isNewGameButton && !hasSave)
            {
                // Nothing to start over from yet on a fresh boot - hide it entirely rather than
                // showing a redundant button that does the same thing as Play.
                item.gameObject.SetActive(false);
                continue;
            }

            _activeMenuItems.Add(item);
        }

        _menu = new InGameMenu(
            _activeMenuItems.ConvertAll(x => (IInGameMenuItem)x),
            _directionInputAction,
            _actionInputAction,
            MAX_VISIBLE_ITEMS
        );
        _menu.OnItemSubmitted += OnMenuButtonSelected;

        if (_confirmItems != null && _confirmItems.Count > 0)
        {
            _confirmMenu = new InGameMenu(
                _confirmItems.ConvertAll(x => (IInGameMenuItem)x),
                _directionInputAction,
                _actionInputAction,
                MAX_VISIBLE_ITEMS
            );
            _confirmMenu.OnItemSubmitted += OnConfirmButtonSelected;
        }

        if (_confirmRoot != null)
            _confirmRoot.gameObject.SetActive(false);
    }

    private void OnMenuButtonSelected(int index)
    {
        if (index < 0 || index >= _activeMenuItems.Count)
            return;

        var selected = _activeMenuItems[index];

        if (selected == (InGameMenuItemBase)_playButtonItem)
            PlayGame();
        else if (_newGameButtonItem != null && selected == (InGameMenuItemBase)_newGameButtonItem)
            EnterNewGameConfirm();
        else
            CloseGame();
    }

    private void OnConfirmButtonSelected(int index)
    {
        switch (index)
        {
            case 0: ConfirmNewGame(); break;
            case 1: CancelNewGameConfirm(); break;
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

    // ── New Game confirmation flow ───────────────────────────────────────────
    private void EnterNewGameConfirm()
    {
        if (_confirmRoot == null || _confirmMenu == null)
        {
            // No confirmation UI wired up in the Inspector yet - fall back to starting immediately.
            StartNewGame();
            return;
        }

        _menu.ExitMenu();
        if (_menuRoot != null) _menuRoot.gameObject.SetActive(false);
        _confirmRoot.gameObject.SetActive(true);
        _confirmMenu.EnterMenu();
    }

    private void CancelNewGameConfirm()
    {
        _confirmMenu.ExitMenu();
        if (_confirmRoot != null) _confirmRoot.gameObject.SetActive(false);
        if (_menuRoot != null) _menuRoot.gameObject.SetActive(true);
        _menu.EnterMenu();
    }

    private void ConfirmNewGame()
    {
        _confirmMenu.Dispose();
        StartNewGame();
    }

    private void StartNewGame()
    {
        _saveService.ResetSave();
        UpdatePlayButtonLabel();
        _menu.Dispose();
        _confirmMenu?.Dispose();
        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
        LoadGameScene().Forget();
    }

    // ── Play (resumes existing save automatically, or starts fresh if none exists) ──
    private void PlayGame()
    {
        _menu.Dispose();
        _confirmMenu?.Dispose();
        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
        LoadGameScene().Forget();
    }

    private void CloseGame()
    {
        Application.Quit();
    }

    private async UniTaskVoid LoadGameScene()
    {
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

        // Load whichever scene the save points to (a later chapter's scene), falling back to
        // this menu's configured first/default scene for a brand new game or an old save
        // saved before multi-scene support existed.
        string targetScene = string.IsNullOrEmpty(_saveService.Data.CurrentSceneName)
            ? _sceneName
            : _saveService.Data.CurrentSceneName;

        SceneManager.LoadScene(targetScene);
    }
}
