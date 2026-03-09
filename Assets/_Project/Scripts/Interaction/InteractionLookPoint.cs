using UnityEngine;
using Zenject;

public class InteractionLookPoint : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 0f;
    [SerializeField] private float _maxVerticalAngle = 30f;
    [SerializeField] private float _maxHorizontalAngle = 30f;

    [Inject] private InputManager _input;

    private float _xRotation;
    private float _yRotation;
    private bool _isActive;

    private Quaternion _baseRotation;

    private void Start()
    {
        _baseRotation = transform.localRotation;
    }

    public void SetActive(bool active)
    {
        _isActive = active;

        if (!active)
        {
            _xRotation = 0f;
            _yRotation = 0f;
            transform.localRotation = _baseRotation;
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        Vector2 look = _input.GamingLookAction.ReadValue<Vector2>() * _sensitivity;

        _xRotation -= look.y;
        _xRotation = Mathf.Clamp(_xRotation, -_maxVerticalAngle, _maxVerticalAngle);

        _yRotation += look.x;
        _yRotation = Mathf.Clamp(_yRotation, -_maxHorizontalAngle, _maxHorizontalAngle);

        Quaternion offset = Quaternion.Euler(_xRotation, _yRotation, 0f);

        transform.localRotation = _baseRotation * offset;
    }
}