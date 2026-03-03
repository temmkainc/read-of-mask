using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInteraction : MonoBehaviour
{
    private IRaycaster _raycaster;
    private IInteractable _currentLookTarget;

    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _distance = 3f;

    [Inject] private InputManager _inputManager;

    private Player _player;
    private Camera _playerCamera;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerCamera = Camera.main;
        _raycaster = new CameraCenterRaycaster(_playerCamera, _distance, _interactableLayer);
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
        if (_raycaster.TryHit<IInteractable>(out var interactable))
        {
            if (_currentLookTarget != interactable)
            {
                ClearHighlight();
                _currentLookTarget = interactable;

                //if (_currentLookTarget.CanBeHighlighted)
                //{
                    // TODO: highlight logic here
                //}

                Debug.Log($"[Interaction] Looking at: {interactable}");
            }

            return;
        }

        ClearHighlight();
        _currentLookTarget = null;
    }

    private void ClearHighlight()
    {
        //if (_currentLookTarget != null && _currentLookTarget.CanBeHighlighted)
        //{
            // TODO: remove highlight
        //}
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