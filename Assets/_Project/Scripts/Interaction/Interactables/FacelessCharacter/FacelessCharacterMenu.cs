using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class FacelessCharacterMenu : LookCloserInteractableBase
{
    [SerializeField] protected List<InGameMenuItemBase> _menuItems;
    [SerializeField] private float _enterDelay = 0.2f;
    [SerializeField] private float _scaleDuration = 0.6f;

    [SerializeField] private ValueInGameMenuItem _sensitivityMenuItem;

    [Inject] protected InputManager _inputManager;

    protected InputAction _directionInputAction;
    protected InputAction _actionInputAction;

    protected InGameMenu _menu;

    private void Awake()
    {
        _directionInputAction = _inputManager.LookCloserDirectionAction;
        _actionInputAction = _inputManager.LookCloserActionAction;

        _menu = new InGameMenu(
            _menuItems.ConvertAll(x => (IInGameMenuItem)x),
            _directionInputAction,
            _actionInputAction
        );
        _menu.OnItemSubmitted += OnMenuButtonSelected;

        _sensitivityMenuItem.Initialize(1f, null);
    }

    private void OnMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: ContinueGame(); break;
            case 1: break; //sensitivity;
            case 2: CloseGame(); break;
        }
    }

    public override void Interact(Player player = null)
    {
        gameObject.SetActive(true);
        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint.transform;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
        CameraSnapPoint.SetActive(false);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.LookCloser)).Execute();
    }

    private void ContinueGame()
    {
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.General)).Execute();
        ExitMenu();
    }

    private void CloseGame()
    {
        Application.Quit();
    }

    public void EnterMenu()
    {
        DOVirtual.DelayedCall(_enterDelay, () =>
        {
            PlayEnterAnimation();
            Interact();
            _menu.EnterMenu();
        });
    }
    private void ExitMenu()
    {
        transform.DOScaleY(0.05f, 0.2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _menu.ExitMenu();
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
        if(type != PlayerStateType.LookCloser)
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
