using System;

public interface IHighlightable
{
    bool CanHighlight(PlayerGrabbing grabbing);
    public string HintLabel => "Take";
    public float HintXOffset => 0f;
    public float HintYOffset => 0.3f;
    public float HintZOffset => 0f;
}