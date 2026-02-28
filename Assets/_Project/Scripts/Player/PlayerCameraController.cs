using UnityEngine;
using Zenject;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Transform _playerRoot;  
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private float _maxLookAngle = 80f;

    [Inject] private InputManager _input;
    private float _xRotation;


    private void Update()
    {
        Vector2 look = _input.LookInput * _sensitivity;

        _playerRoot.Rotate(Vector3.up * look.x);

        _xRotation -= look.y;
        _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);
        _cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}