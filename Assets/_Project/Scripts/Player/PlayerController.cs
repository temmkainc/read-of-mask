using UnityEngine;

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

    private InputsManager _input;
    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = GetComponent<InputsManager>();
    }

    private void Update() => HandleMovement();

    private void HandleMovement()
    {
        _isGrounded = _cc.isGrounded;

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float speed = _input.SprintHeld ? _sprintSpeed : _walkSpeed;

        Vector3 forward = _orientation.forward;
        Vector3 right = _orientation.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * _input.MoveInput.y + right * _input.MoveInput.x;
        if (move.magnitude > 1f)
            move.Normalize();

        _cc.Move(move * speed * Time.deltaTime);

        if (_input.JumpPressed && _isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
        _velocity.y += _gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}