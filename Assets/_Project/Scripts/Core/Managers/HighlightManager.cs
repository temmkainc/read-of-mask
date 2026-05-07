using Zenject;

public class HighlightManager
{
    private Outline _current;
    [Inject] private OverlayHintsSceneContainer _overlayHintsSceneContainer;

    public void SetHighlight(Outline target)
    {
        if (_current == target) return;

        if (_current != null) _current.enabled = false;
        _current = target;
        if (_current != null) _current.enabled = true;
        _overlayHintsSceneContainer.ToInteractTMP.gameObject.SetActive(_current != null);
    }

    public void ClearHighlight()
    {
        if (_current != null) _current.enabled = false;
        _current = null;
        _overlayHintsSceneContainer.ToInteractTMP.gameObject.SetActive(false);
    }
}