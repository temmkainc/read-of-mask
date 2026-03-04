using System;
using UnityEngine;
using Zenject;
using static InputManager;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpForce = 1.5f;

    [Header("References")]
    [SerializeField] private Transform _orientation;

    [Inject] private InputManager _input;
    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isGrounded;

    public Vector2 MoveInput { get; private set; }
    private bool _jumpPressed;
    private bool _sprintHeld;

    private bool _isCurrentMapPlayer = true;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input.OnActionMapChanged += On_InputActionMapChanged;
    }

    private void On_InputActionMapChanged(InputManager.ActionMapType type)
    {
        if(type != InputManager.ActionMapType.Player)
        {
            _isCurrentMapPlayer = false;
            return;
        }
        _isCurrentMapPlayer = true;
    }

    private void Update() => HandleMovement();

    private void HandleMovement()
    {
        if (!_isCurrentMapPlayer)
        {
            MoveInput = Vector2.zero;
            _jumpPressed = false;
            _sprintHeld = false;
            return;
        }

        MoveInput = _input.PlayerMoveAction.ReadValue<Vector2>();
        _jumpPressed = _input.PlayerJumpAction.triggered;
        _sprintHeld = _input.PlayerSprintAction.IsPressed();
        _isGrounded = _cc.isGrounded;

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float speed = _sprintHeld ? _sprintSpeed : _walkSpeed;

        Vector3 forward = _orientation.forward;
        Vector3 right = _orientation.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * MoveInput.y + right * MoveInput.x;
        if (move.magnitude > 1f)
            move.Normalize();

        _cc.Move(move * speed * Time.deltaTime);

        if (_jumpPressed && _isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
        _velocity.y += _gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}