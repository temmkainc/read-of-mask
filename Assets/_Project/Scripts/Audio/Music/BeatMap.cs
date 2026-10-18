using UnityEngine;

/// <summary>
/// The beat times of a music track (seconds from the start of its clip), for things that should move
/// on the beat of a live recording whose tempo drifts a little - e.g. the faceless guitarist's strumming.
/// </summary>
[CreateAssetMenu(menuName = "Audio/Beat Map")]
public class BeatMap : ScriptableObject
{
    public float[] BeatTimes;

    /// <summary>Beats since the clip started (fractional), counting on across loops. -1 when empty.</summary>
    public float BeatAt(float time, float clipLength, int loopsPlayed)
    {
        if (BeatTimes == null || BeatTimes.Length == 0) return -1f;

        int count = BeatTimes.Length;
        int index = System.Array.BinarySearch(BeatTimes, time);
        if (index < 0) index = ~index - 1;

        float start, next;
        if (index < 0)
        {
            // before the first beat: count back from the last beat of the previous loop
            start = BeatTimes[count - 1] - clipLength;
            next = BeatTimes[0];
            index = -1;
        }
        else
        {
            start = BeatTimes[index];
            next = index + 1 < count ? BeatTimes[index + 1] : BeatTimes[0] + clipLength;
        }

        float phase = Mathf.Clamp01((time - start) / Mathf.Max(next - start, 0.0001f));
        return loopsPlayed * count + index + phase;
    }

    /// <summary>Length of the beat playing at this time.</summary>
    public float BeatLengthAt(float time, float clipLength)
    {
        if (BeatTimes == null || BeatTimes.Length < 2) return 0f;

        int count = BeatTimes.Length;
        int index = System.Array.BinarySearch(BeatTimes, time);
        if (index < 0) index = ~index - 1;
        if (index < 0) return BeatTimes[0] + clipLength - BeatTimes[count - 1];
        return index + 1 < count ? BeatTimes[index + 1] - BeatTimes[index] : BeatTimes[0] + clipLength - BeatTimes[index];
    }
}
