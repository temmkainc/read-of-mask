using Zenject;
using TMPro;
using UnityEngine;

public class OverlayHintsSceneContainer : MonoBehaviour
{
    // ── overlay hints (screen-space, unchanged) ──────────────────────────────
    [field: SerializeField] public TMP_Text ToToggleMaskTMP { get; private set; }
    [field: SerializeField] public TMP_Text ToOpenDiaryTMP  { get; private set; }
    [field: SerializeField] public TMP_Text ToThrowTMP      { get; private set; }
    [field: SerializeField] public TMP_Text ToExitTMP       { get; private set; }

    // ── world-space interact label ───────────────────────────────────────────
    [field: SerializeField] public WorldHintLabel InteractWorldHint { get; private set; }

    // ── deps ─────────────────────────────────────────────────────────────────
    [Inject] private LockableActionsManager _lockableActionsManager;
    [Inject] private PlayerLookTarget       _playerLookTarget;

    // ── state ─────────────────────────────────────────────────────────────────
    private IHighlightable   _currentHighlightable;
    private IDynamicHintLabel _currentDynamic;
    private Collider          _currentCollider;

    // ── lifecycle ────────────────────────────────────────────────────────────
    private void OnEnable()  => _playerLookTarget.OnTargetChanged += HandleTargetChanged;
    private void OnDisable()
    {
        _playerLookTarget.OnTargetChanged -= HandleTargetChanged;
        UnsubscribeFromCurrent();
    }

    // ── target tracking ──────────────────────────────────────────────────────
    private void HandleTargetChanged(Collider targetCollider)
    {
        UnsubscribeFromCurrent();

        if (targetCollider == null)
        {
            InteractWorldHint.Hide();
            return;
        }

        var highlightable = targetCollider.GetComponent<IHighlightable>();
        if (highlightable == null)
        {
            InteractWorldHint.Hide();
            return;
        }

        _currentHighlightable = highlightable;
        _currentCollider      = targetCollider;
        _currentDynamic       = highlightable as IDynamicHintLabel;
        if (_currentDynamic != null)
            _currentDynamic.OnHintChanged += RefreshCurrentHint;

        RefreshCurrentHint();
    }

    // Called both on target acquired and whenever the object fires OnHintChanged.
    private void RefreshCurrentHint()
    {
        if (_currentHighlightable == null) return;

        if (!_currentHighlightable.CanHighlight(_playerLookTarget.Grabbing))
        {
            InteractWorldHint.Hide();
            return;
        }

        InteractWorldHint.Show(
            _currentCollider.transform,
            _currentHighlightable.HintLabel,
            _currentHighlightable.HintXOffset,
            _currentHighlightable.HintYOffset,
            _currentHighlightable.HintZOffset);
    }

    private void UnsubscribeFromCurrent()
    {
        if (_currentHighlightable == null) return;
        if (_currentDynamic != null)
        {
            _currentDynamic.OnHintChanged -= RefreshCurrentHint;
            _currentDynamic = null;
        }
        _currentHighlightable = null;
        _currentCollider      = null;
    }

    // ── state callbacks ───────────────────────────────────────────────────────
    public void OnLookCloserStateEntered()
    {
        if (ToOpenDiaryTMP == null || ToExitTMP == null) return;

        ToOpenDiaryTMP.gameObject.SetActive(false);
        ToExitTMP.gameObject.SetActive(true);
        InteractWorldHint.Hide();
    }

    public void OnLookCloserStateExited()
    {
        if (ToOpenDiaryTMP == null || ToExitTMP == null) return;

        if (!_lockableActionsManager.IsActionLocked(LockableActionsManager.LockableActionType.OpenDiary))
            ToOpenDiaryTMP.gameObject.SetActive(true);

        ToExitTMP.gameObject.SetActive(false);
    }
}
