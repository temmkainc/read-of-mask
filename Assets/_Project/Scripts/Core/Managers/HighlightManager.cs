public class HighlightManager
{
    private Outline _current;

    public void SetHighlight(Outline target)
    {
        if (_current == target) return;

        if (_current != null) _current.enabled = false;
        _current = target;
        if (_current != null) _current.enabled = true;
    }

    public void ClearHighlight()
    {
        if (_current != null) _current.enabled = false;
        _current = null;
    }
}