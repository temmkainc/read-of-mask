using UnityEngine;
using Zenject;

public class InteractionLookPoint : MonoBehaviour
{
    [SerializeField] private float _maxVerticalAngle = 30f;
    [SerializeField] private float _maxHorizontalAngle = 30f;

    [Inject] private InputManager _input;
    [Inject] private GameOptions _gameOptions;

    private float _xRotation;
    private float _yRotation;
    private bool _isActive;

    private const float SENSITIVITY_SCALE_FACTOR = 0.05f;

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

        var inputAction = _input.CurrentMap switch
        {
            InputManager.ActionMapType.Gaming => _input.GamingLookAction,
            InputManager.ActionMapType.LookCloser => _input.LookCloserLookAction,
            _ => null
        };

        if(inputAction == null) 
            return;

        Vector2 look = inputAction.ReadValue<Vector2>() * _gameOptions.Sensitivity * SENSITIVITY_SCALE_FACTOR;

        _xRotation -= look.y;
        _xRotation = Mathf.Clamp(_xRotation, -_maxVerticalAngle, _maxVerticalAngle);

        _yRotation += look.x;
        _yRotation = Mathf.Clamp(_yRotation, -_maxHorizontalAngle, _maxHorizontalAngle);

        Quaternion offset = Quaternion.Euler(_xRotation, _yRotation, 0f);

        transform.localRotation = _baseRotation * offset;
    }
}