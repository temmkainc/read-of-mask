using System;

/// <summary>
/// Holds the player's current objective text (e.g. the active obligatory step: "Go to the kitchen")
/// for any UI to show. Anything can set it; ObjectiveHud displays it.
/// </summary>
public class ObjectiveTracker
{
    public string Current { get; private set; } = string.Empty;

    public event Action<string> OnObjectiveChanged;

    public void Set(string text)
    {
        text ??= string.Empty;
        if (text == Current)
            return;

        Current = text;
        OnObjectiveChanged?.Invoke(Current);
    }

    public void Clear() => Set(string.Empty);
}
