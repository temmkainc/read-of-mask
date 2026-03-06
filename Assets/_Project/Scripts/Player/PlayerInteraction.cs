using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInteraction : MonoBehaviour
{
    [Inject] private PlayerLookTarget _lookTarget;

    private IInteractable _currentLookTarget;

    [SerializeField] private LayerMask _interactableLayer;

    [Inject] private InputManager _inputManager;

    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        _inputManager.InteractAction.performed += On_InteractPerformed;
    }

    private void OnDisable()
    {
        _inputManager.InteractAction.performed -= On_InteractPerformed;
    }

    private void Update()
    {
        UpdateLookTarget();
    }

    private void UpdateLookTarget()
    {
        if (_lookTarget.TryGet<IInteractable>(out var interactable))
        {
            if (_currentLookTarget != interactable)
            {
                _currentLookTarget = interactable;
            }
            return;
        }

        _currentLookTarget = null;
    }

    private void On_InteractPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[Interaction] Attempting to interact...");
        TryInteract();
    }

    private void TryInteract()
    {

        if (_currentLookTarget != null)
        {
            _currentLookTarget.Interact(_player);
            Debug.Log($"[Interaction] Interacted with: {_currentLookTarget}");
        }
    }
}