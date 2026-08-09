using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PullUpController : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> _waypoints = new();

    [Header("Timing")]
    [SerializeField] private float _basePointerSpeed = 0.6f;
    [SerializeField] private float _speedIncreasePerRep = 0.08f;
    [SerializeField] private float _maxPointerSpeed = 2.5f;

    [Header("Movement")]
    [SerializeField] private float _lerpSpeed = 6f;
    [SerializeField] private float _returnLerpSpeed = 4f;

    [Header("Window")]
    [SerializeField] private float _minWindowSize = 0.12f;
    [SerializeField] private float _maxWindowSize = 0.22f;
    [SerializeField] private float _windowShrinkPerRep = 0f;

    [SerializeField] private PullUpUI _pullUpUI;
    [SerializeField] private SFXData[] _pullupStagesSFX;
    public int RepCount { get; private set; }
    public bool IsActive { get; private set; }
    public RhythmPressChallenge Challenge { get; } = new();

    private InteractionLookPoint _lookPoint;
    private int _currentStep;
    private bool _returning;

    [Inject] private InputManager _inputManager;

    public void Activate(InteractionLookPoint lookPoint)
    {
        _lookPoint = lookPoint;
        RepCount = 0;
        IsActive = true;
        _returning = false;
        _lookPoint.SetExternalPosition(_waypoints[0].position);
        _currentStep = 0;
        StartChallenge();
    }

    public void Deactivate()
    {
        IsActive = false;
        _returning = false;
        Challenge.Deactivate();
        _lookPoint = null;
    }

    private void Update()
    {
        if (!IsActive || _lookPoint == null) return;

        Challenge.Tick(Time.deltaTime);

        Vector3 targetPosition = _waypoints[_currentStep].position;
        float speed = _returning ? _returnLerpSpeed : _lerpSpeed;
        _lookPoint.SetExternalPosition(
            Vector3.Lerp(_lookPoint.transform.position, targetPosition, speed * Time.deltaTime)
        );

        if (_returning)
        {
            if (Vector3.Distance(_lookPoint.transform.position, _waypoints[0].position) < 0.02f)
            {
                _returning = false;
                _lookPoint.SetExternalPosition(_waypoints[0].position);
                StartChallenge();
            }
            return;
        }

        if (_inputManager.LookCloserActionAction.WasPressedThisFrame())
        {
            if (Challenge.TryPress())
                OnHit();
            else
                OnMiss();
        }
    }

    private void OnHit()
    {
        _pullUpUI.ShowHitFlash();
        AudioManager.Instance.PlaySFX(_pullupStagesSFX[_currentStep].Id, volumeScale: 1f);
        int nextStep = _currentStep + 1;

        if (nextStep >= _waypoints.Count)
        {
            RepCount++;
            _pullUpUI.UpdateRepCount(RepCount);
            _returning = true;
            _currentStep = 0;
            Challenge.Deactivate();
        }
        else
        {
            _currentStep = nextStep;
            Challenge.RandomizeWindow(_minWindowSize, _maxWindowSize);
        }
    }

    private void OnMiss()
    {
        _pullUpUI.ShowMissFlash();
        RepCount = 0;
        _pullUpUI.UpdateRepCount(RepCount);
        _returning = true;
        _currentStep = 0;
        Challenge.Deactivate();
    }

    private void StartChallenge()
    {
        float speed = Mathf.Min(_basePointerSpeed + _speedIncreasePerRep * RepCount, _maxPointerSpeed);
        float minSize = Mathf.Max(_minWindowSize - _windowShrinkPerRep * RepCount, 0.05f);
        float maxSize = Mathf.Max(_maxWindowSize - _windowShrinkPerRep * RepCount, 0.07f);
        Challenge.Activate(speed, minSize, maxSize);
    }

}