using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerGrabbing : MonoBehaviour
{
    public bool IsHolding => _heldObject != null;

    [SerializeField] private LayerMask _grabbableLayer;
    [SerializeField] private float _holdDistance = 0f;
    [SerializeField] private float _holdFollowSpeed = 10f;
    [SerializeField] private float _maxHoldAngle = 30f;

    [SerializeField] private LayerMask _collisionMask; 

    [SerializeField] private Transform _cameraFollowTarget;
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private float _pushDampingSpeed = 10f;

    [Inject] private InputManager _inputManager;

    private Vector3 _cameraFollowOriginalLocalPosition;
    private Vector3 _currentHoldPosition;
    private bool _isObjectBlocked;

    [Inject] private PlayerLookTarget _lookTarget;
    private IGrabbable _currentLookTarget;
    private IGrabbable _heldObject;

    private Player _player;
    private Camera _playerCamera;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerCamera = Camera.main;
        _cameraFollowOriginalLocalPosition = _cameraFollowTarget.localPosition;
    }

    private void OnEnable()
    {
        _inputManager.PlayerGrabAction.performed += OnGrabPerformed;
    }

    private void OnDisable()
    {
        _inputManager.PlayerGrabAction.performed -= OnGrabPerformed;
    }

    private void Update()
    {
        if (_heldObject == null)
            UpdateLookTarget();
        else
            UpdateHeldObjectPosition();
    }

    public bool TryGetHeld<T>(out T result) where T : class
    {
        if (_heldObject is T typed)
        {
            result = typed;
            return true;
        }

        result = null;
        return false;
    }
    public GameObject ReleaseHeldObject()
    {
        if (_heldObject == null)
            return null;

        _cameraFollowTarget.localPosition = _cameraFollowOriginalLocalPosition;

        var obj = (_heldObject as MonoBehaviour).gameObject;

        _heldObject.Release(Vector3.zero);

        _heldObject = null;

        _inputManager.PlayerGrabAction.performed -= OnThrowPerformed;
        _inputManager.PlayerGrabAction.performed += OnGrabPerformed;

        return obj;
    }

    private void UpdateHeldObjectPosition()
    {
        var heldTransform = (_heldObject as MonoBehaviour).transform;

        Ray centerRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 desiredPosition = centerRay.GetPoint(_holdDistance);

        Vector3 toDesired = desiredPosition - _holdPoint.position;
        Vector3 toDesiredClamped = Vector3.RotateTowards(
            _holdPoint.forward * _holdDistance,
            toDesired,
            Mathf.Deg2Rad * _maxHoldAngle,
            float.MaxValue
        );
        Vector3 clampedPosition = _holdPoint.position + toDesiredClamped.normalized * _holdDistance;

        Vector3 dir = clampedPosition - _playerCamera.transform.position;
        float dist = dir.magnitude;
        if (Physics.Raycast(_playerCamera.transform.position, dir.normalized, out RaycastHit hit, dist, _collisionMask))
        {
            clampedPosition = hit.point + hit.normal * _heldObject.WallOffset;
            _isObjectBlocked = true;
        }
        else
        {
            _isObjectBlocked = false;
        }

        _currentHoldPosition = Vector3.Lerp(_currentHoldPosition, clampedPosition, Time.deltaTime * _holdFollowSpeed);

        heldTransform.position = _currentHoldPosition;
        heldTransform.rotation = _holdPoint.rotation;

        PushCameraFollowTargetFromHeldObject(heldTransform);
    }

    private void PushCameraFollowTargetFromHeldObject(Transform heldTransform)
    {
        if (!_isObjectBlocked)
        {
            _cameraFollowTarget.localPosition = new Vector3(
                _cameraFollowOriginalLocalPosition.x,
                _cameraFollowOriginalLocalPosition.y,
                Mathf.Lerp(_cameraFollowTarget.localPosition.z, _cameraFollowOriginalLocalPosition.z, Time.deltaTime * _pushDampingSpeed)
            );
            return;
        }

        Vector3 toObject = heldTransform.position - _playerCamera.transform.position;
        float distanceToObject = toObject.magnitude;

        float pushRatio = Mathf.Clamp01((_heldObject.MinCameraDistance - distanceToObject) / _heldObject.MinCameraDistance);
        float targetZ = Mathf.Lerp(_cameraFollowOriginalLocalPosition.z, _cameraFollowOriginalLocalPosition.z - 1f, pushRatio);

        _cameraFollowTarget.localPosition = new Vector3(
            _cameraFollowOriginalLocalPosition.x,
            _cameraFollowOriginalLocalPosition.y,
            Mathf.Lerp(_cameraFollowTarget.localPosition.z, targetZ, Time.deltaTime * _pushDampingSpeed)
        );
    }

    private void UpdateLookTarget()
    {
        if (_lookTarget.TryGet<IGrabbable>(out var grabbable))
        {
            if (_currentLookTarget != grabbable)
            {
                _currentLookTarget = grabbable;
            }
            return;
        }

        _currentLookTarget = null;
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        TryGrab();
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        Throw();
    }

    private void TryGrab()
    {
        if (_currentLookTarget == null || _currentLookTarget.IsGrabbed) return;

        _heldObject = _currentLookTarget;
        _currentLookTarget = null;

        _currentHoldPosition = (_heldObject as MonoBehaviour).transform.position;

        _heldObject.Grab(_player, _holdPoint);
        _inputManager.PlayerGrabAction.performed -= OnGrabPerformed;
        SubscribeThrowNextFrame().Forget();
        Debug.Log($"[Grabbing] Grabbed: {_heldObject}");
    }

    private void Throw()
    {
        _cameraFollowTarget.localPosition = _cameraFollowOriginalLocalPosition;
        Vector3 force = _playerCamera.transform.forward * _heldObject.ThrowForce;
        _heldObject.Release(force);
        _heldObject = null;
        _inputManager.PlayerGrabAction.performed -= OnThrowPerformed;
        _inputManager.PlayerGrabAction.performed += OnGrabPerformed;
    }
    private async UniTaskVoid SubscribeThrowNextFrame()
    {
        await UniTask.NextFrame();
        _inputManager.PlayerGrabAction.performed += OnThrowPerformed;
    }
}