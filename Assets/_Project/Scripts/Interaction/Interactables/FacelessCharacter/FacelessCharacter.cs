using System;
using UnityEngine;
using Zenject;

public class FacelessCharacter : LookCloserInteractableBase
{
    [SerializeField] private Animator _animator;
    private InputManager _inputManager;

    private readonly int DoShowMiddleFingerHash = Animator.StringToHash("DoShowMiddleFinger");
    private readonly int DoHideMiddleFingerHash = Animator.StringToHash("DoHideMiddleFinger");

    [Inject]
    public void Construct(InputManager inputManager)
    {
        _inputManager = inputManager;
        _inputManager.ShowMiddleFingerAction.performed += On_MiddleFingerPressed;
        _inputManager.ShowMiddleFingerAction.canceled += On_MiddleFingerReleased;
    }

    private void On_MiddleFingerReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {    
        _animator.ResetTrigger(DoShowMiddleFingerHash);
        _animator.SetTrigger(DoHideMiddleFingerHash);
    }

    private void On_MiddleFingerPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _animator.ResetTrigger(DoHideMiddleFingerHash);
        _animator.SetTrigger(DoShowMiddleFingerHash);
    }
}
