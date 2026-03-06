using System;
using UnityEngine;

public class PlayerLookTarget
{
    [Serializable]
    public struct Config
    {
        [field: SerializeField] public float Distance { get; private set; }
        [field: SerializeField] public LayerMask Mask { get; private set; }
    }

    private readonly Camera _camera;
    private readonly float _distance;
    private readonly LayerMask _mask;
    private readonly HighlightManager _highlightManager;
    private readonly PlayerGrabbing _playerGrabbing;
    public object Current { get; private set; }

    public PlayerLookTarget(Config config, HighlightManager highlightManager, PlayerGrabbing playerGrabbing)
    {
        _camera = Camera.main;
        _distance = config.Distance;
        _mask = config.Mask;
        _highlightManager = highlightManager;

        _playerGrabbing = playerGrabbing;
    }

    public void Tick()
    {
        var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (!Physics.Raycast(ray, out var hit, _distance, _mask))
        {
            _highlightManager.ClearHighlight();
            Current = null;
            return;
        }

        if (hit.collider.TryGetComponent<IGrabbable>(out var grabbable))
            Current = grabbable;
        else if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            Current = interactable;
        else
            Current = null;

        var highlightable = hit.collider.GetComponent<IHighlightable>();
        bool canHighlight = highlightable != null && (!_playerGrabbing.IsHolding || highlightable.HighlightWhenHolding);

        if (canHighlight)
        {
            _highlightManager.SetHighlight(hit.collider.GetComponentInChildren<Outline>());
        }
        else
        {
            _highlightManager.ClearHighlight();
        }
    }

    public bool TryGet<T>(out T result) where T : class
    {
        result = Current as T;
        return result != null;
    }
}