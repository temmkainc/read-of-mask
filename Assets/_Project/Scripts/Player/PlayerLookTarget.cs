using System;
using UnityEngine;

public class PlayerLookTarget
{
    [Serializable]
    public struct Config
    {
        [field: SerializeField] public float Distance { get; private set; }
        [field: SerializeField] public LayerMask Mask { get; private set; }
        [field: SerializeField] public float AuxRayViewportRadius { get; private set; }
        [field: SerializeField] public int AuxRayCount { get; private set; }
    }

    // Fired whenever the looked-at interactable target changes (including to null).
    // Carries the collider of the new target, or null if nothing is targeted.
    public event Action<Collider> OnTargetChanged;

    private const int SCORE_GRABBABLE    = 2;
    private const int SCORE_INTERACTABLE = 1;
    private const int SCORE_NONE        = 0;

    private readonly struct Candidate
    {
        public readonly object   Target;
        public readonly int      Priority;
        public readonly float    Distance;
        public readonly float    ViewportDist;
        public readonly Collider Collider;

        public Candidate(object target, int priority, float distance, float viewportDist, Collider collider)
        {
            Target       = target;
            Priority     = priority;
            Distance     = distance;
            ViewportDist = viewportDist;
            Collider     = collider;
        }
    }

    private readonly Camera              _camera;
    private readonly float               _distance;
    private readonly LayerMask           _mask;
    private readonly HighlightManager    _highlightManager;
    private readonly PlayerGrabbing      _playerGrabbing;
    private readonly IPlayerStateManager _playerStateManager;
    private readonly Vector3[]           _rayOrigins;

    // Track the previously reported collider so we only fire OnTargetChanged
    // when something actually changes (not every frame).
    private Collider _lastReportedCollider;
    // Tracked separately from the collider reference itself: if the reported collider gets
    // destroyed while still being the current target, Unity's == treats it as equal to null,
    // which would incorrectly suppress the transition to "nothing targeted" below. This flag
    // makes that transition explicit regardless of what _lastReportedCollider compares as.
    private bool _hasReportedTarget;

    public object Current { get; private set; }
    public PlayerGrabbing Grabbing => _playerGrabbing;

    public PlayerLookTarget(Config config,
                            HighlightManager    highlightManager,
                            PlayerGrabbing      playerGrabbing,
                            IPlayerStateManager playerStateManager)
    {
        _camera             = Camera.main;
        _distance           = config.Distance;
        _mask               = config.Mask;
        _highlightManager   = highlightManager;
        _playerStateManager = playerStateManager;
        _playerGrabbing     = playerGrabbing;

        int auxCount = Mathf.Max(0, config.AuxRayCount);
        _rayOrigins  = new Vector3[1 + auxCount];
        _rayOrigins[0] = new Vector3(0.5f, 0.5f, 0f);

        float r = config.AuxRayViewportRadius;
        for (int i = 0; i < auxCount; i++)
        {
            float angle = 2f * Mathf.PI * i / auxCount;
            _rayOrigins[1 + i] = new Vector3(
                0.5f + r * Mathf.Cos(angle),
                0.5f + r * Mathf.Sin(angle),
                0f);
        }
    }

    public void Tick()
    {
        Candidate? best = null;

        for (int i = 0; i < _rayOrigins.Length; i++)
        {
            var ray = _camera.ViewportPointToRay(_rayOrigins[i]);
            if (!Physics.Raycast(ray, out var hit, _distance, _mask)) continue;

            float vpDist = (i == 0) ? 0f : Vector2.Distance(
                new Vector2(_rayOrigins[i].x, _rayOrigins[i].y),
                new Vector2(0.5f, 0.5f));

            var candidate = new Candidate(
                target:       TargetFromHit(hit.collider),
                priority:     ScoreHit(hit.collider),
                distance:     hit.distance,
                viewportDist: vpDist,
                collider:     hit.collider);

            if (best == null || IsBetter(candidate, best.Value))
                best = candidate;
        }

        if (best == null)
        {
            _highlightManager.ClearHighlight();
            Current = null;
            ReportTargetCollider(null);
            return;
        }

        Current = best.Value.Target;
        ApplyHighlight(best.Value.Collider);

        // Only show hint for colliders that actually carry an interactable.
        ReportTargetCollider(best.Value.Target != null ? best.Value.Collider : null);
    }

    public bool TryGet<T>(out T result) where T : class
    {
        result = Current as T;
        return result != null;
    }

    // ── private ─────────────────────────────────────────────────────────────

    private void ReportTargetCollider(Collider col)
    {
        if (col != null)
        {
            // Only skip if it's the exact same still-alive collider we already reported.
            if (_hasReportedTarget && col == _lastReportedCollider)
                return;

            _lastReportedCollider = col;
            _hasReportedTarget = true;
            OnTargetChanged?.Invoke(col);
            return;
        }

        // Nothing currently targeted. Fire even if _lastReportedCollider now Unity-compares as
        // null (e.g. it was destroyed) - what matters is whether we'd previously reported a
        // target, not whether the stale reference happens to look null right now.
        if (!_hasReportedTarget)
            return;

        _lastReportedCollider = null;
        _hasReportedTarget = false;
        OnTargetChanged?.Invoke(null);
    }

    private static object TargetFromHit(Collider col)
    {
        if (col.TryGetComponent<IGrabbable>(out var g))     return g;
        if (col.TryGetComponent<IInteractable>(out var ia)) return ia;
        return null;
    }

    private static int ScoreHit(Collider col)
    {
        if (col.TryGetComponent<IGrabbable>(out _))    return SCORE_GRABBABLE;
        if (col.TryGetComponent<IInteractable>(out _)) return SCORE_INTERACTABLE;
        return SCORE_NONE;
    }

    private static bool IsBetter(Candidate challenger, Candidate current)
    {
        if (challenger.Priority     != current.Priority)     return challenger.Priority     > current.Priority;
        if (challenger.ViewportDist != current.ViewportDist) return challenger.ViewportDist < current.ViewportDist;
        return challenger.Distance < current.Distance;
    }

    private void ApplyHighlight(Collider col)
    {
        var highlightable = col.GetComponent<IHighlightable>();
        bool canHighlight  = highlightable != null
                          && highlightable.CanHighlight(_playerGrabbing)
                          && _playerStateManager.CurrentStateType != PlayerStateType.LookCloser;

        if (canHighlight)
            _highlightManager.SetHighlight(col.GetComponentInChildren<Outline>());
        else
            _highlightManager.ClearHighlight();
    }
}