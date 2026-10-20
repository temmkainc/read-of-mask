using UnityEngine;

/// <summary>
/// One of the faceless, all-white kids in drawn masks that fill the Level 09 school. Each one does a single
/// idle thing on the spot - stands, fidgets, looks around, shivers, coughs, reaches out, sits at a
/// table - slowly, a little out of step with the world. Wherever the player is, every one of them
/// turns its head to watch them; when the player comes close, the standing ones turn to face them too.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SchoolFigure : MonoBehaviour
{
    // each one is a state of the same name in SchoolFigure.controller (SchoolFigureTools)
    public enum Activity { Stand, Fidget, LookAround, Search, Think, Shiver, RubHands, Huddle, Reach, Cough, Greet, Sit, SitPlay }

    [SerializeField] private Activity _activity = Activity.Stand;
    [Tooltip("Animation speed. Below 1 makes them move a little too slowly.")]
    [SerializeField][Range(0.2f, 1.5f)] private float _animationSpeed = 0.8f;

    [Header("Noticing the player")]
    [Tooltip("Within this distance, standing figures turn to face the player.")]
    [SerializeField] private float _noticeDistance = 5f;
    [Tooltip("Turning speed, degrees per second.")]
    [SerializeField] private float _turnSpeed = 35f;
    [SerializeField] private bool _turnToPlayer = true;

    [Header("Watching the player")]
    [Tooltip("How far the head turns to follow the player, at any distance. Needs IK Pass on the controller's layer.")]
    [SerializeField][Range(0f, 1f)] private float _lookWeight = 1f;
    [Tooltip("How much the spine turns along with the head.")]
    [SerializeField][Range(0f, 1f)] private float _lookBodyWeight = 0.3f;
    [Tooltip("0 lets the head turn all the way round, 1 keeps it still.")]
    [SerializeField][Range(0f, 1f)] private float _lookClamp = 0.35f;
    [Tooltip("How quickly the gaze catches up with the player.")]
    [SerializeField] private float _lookSpeed = 1.5f;

    // the player's eyes are this high above their root when there's no camera to follow
    private const float PlayerEyeHeight = 1.34f;

    private Animator _animator;
    private AnimatorCullingMode _cullingMode;
    private bool _firstFrame = true;
    private Transform _player;
    private Transform _eyes;
    private Vector3 _lookTarget;

    public Activity CurrentActivity => _activity;
    public bool IsSitting => _activity == Activity.Sit || _activity == Activity.SitPlay;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;

        // Off screen they don't animate (the prefab culls the Animator's updates), so each one is
        // animated fully on its first frame to be in its own pose - otherwise one that comes into view
        // would show the prefab's standing pose for a frame. LateUpdate puts the culling back.
        _cullingMode = _animator.cullingMode;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private void Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        _player = player != null ? player.transform : null;
        _eyes = Camera.main != null ? Camera.main.transform : null;
        _lookTarget = EyesPosition();

        // start somewhere random in the loop, so no two of them move in step
        _animator.Play(Animator.StringToHash(_activity.ToString()), 0, Random.value);
        _animator.speed = _animationSpeed;
    }

    private void Update()
    {
        if (!IsSitting)
            TurnTowardsPlayer();
        _lookTarget = Vector3.Lerp(_lookTarget, EyesPosition(), 1f - Mathf.Exp(-_lookSpeed * Time.deltaTime));
    }

    private void LateUpdate()
    {
        // the Animator has run once by now (it updates between Update and LateUpdate)
        if (!_firstFrame)
            return;
        _firstFrame = false;
        _animator.cullingMode = _cullingMode;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_player == null)
            return;
        _animator.SetLookAtWeight(_lookWeight, _lookBodyWeight, 1f, 0f, _lookClamp);
        _animator.SetLookAtPosition(_lookTarget);
    }

    private Vector3 EyesPosition()
    {
        if (_eyes != null)
            return _eyes.position;
        return _player != null ? _player.position + Vector3.up * PlayerEyeHeight : transform.position + transform.forward;
    }

    private void TurnTowardsPlayer()
    {
        if (!_turnToPlayer || _player == null)
            return;
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > _noticeDistance * _noticeDistance || toPlayer.sqrMagnitude < 0.01f)
            return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toPlayer), _turnSpeed * Time.deltaTime);
    }
}
