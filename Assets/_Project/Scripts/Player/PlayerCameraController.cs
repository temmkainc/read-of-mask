using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Transform _playerRoot;
    [SerializeField] private float _maxLookAngle = 80f;
    [Inject] private InputManager _input;
    [Inject] private GameOptions _gameOptions;
    private float _xRotation;
    private const float SENSITIVITY_SCALE_FACTOR = 0.2f;
    private void Update()
    {
        Vector2 lookInput = _input.PlayerLookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 look = lookInput * _gameOptions.Sensitivity * SENSITIVITY_SCALE_FACTOR;

        _playerRoot.Rotate(Vector3.up * look.x);

        _xRotation -= look.y;
        _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);
        _cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}