using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using TMPro;

public class FacelessCharacter : LookCloserInteractableBase
{   
    [Header("Animator")]
    [SerializeField] protected Animator _animator;
    [SerializeField] private bool _canInteract = true;

    private readonly int DoShowMiddleFingerHash = Animator.StringToHash("DoShowMiddleFinger");
    private readonly int DoHideMiddleFingerHash = Animator.StringToHash("DoHideMiddleFinger");

    [Inject] private InputManager _inputManager;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        _inputManager = inputManager;
        _inputManager.ShowMiddleFingerAction.performed += On_MiddleFingerPressed;
        _inputManager.ShowMiddleFingerAction.canceled += On_MiddleFingerReleased;
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
        {
            _inputManager.ShowMiddleFingerAction.performed -= On_MiddleFingerPressed;
            _inputManager.ShowMiddleFingerAction.canceled -= On_MiddleFingerReleased;
        }
    }

    public override bool CanHighlight(PlayerGrabbing grabbing) => _canInteract && base.CanHighlight(grabbing);

    public override void Interact(Player player = null)
    {
        if (player.Grabbing.IsHolding || !_canInteract) return;
        base.Interact(player);
    }

    private void On_MiddleFingerPressed(InputAction.CallbackContext ctx)
    {
        _animator.ResetTrigger(DoHideMiddleFingerHash);
        _animator.SetTrigger(DoShowMiddleFingerHash);
    }

    private void On_MiddleFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.ResetTrigger(DoShowMiddleFingerHash);
        _animator.SetTrigger(DoHideMiddleFingerHash);
    }
}