using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Voiceline", menuName = "Audio/Voiceline")]
public class VoicelineData : AudioData
{
    public List<SubtitleCue> Subtitles = new List<SubtitleCue>();
    public SubtitleCue GetCueAt(float time)
    {
        foreach (var cue in Subtitles)
        {
            if (time >= cue.startTime && time < cue.endTime)
            {
                return cue;
            }
        }
        return null;
    }
    public bool IsValid()
    {
        if (base.AudioClip == null || Subtitles == null || Subtitles.Count == 0) return false;
        foreach (var cue in Subtitles)
            if (cue.endTime <= cue.startTime) return false;
        return true;
    }
}

[Serializable]
public class SubtitleCue
{
    public float startTime;
    public float endTime;
    [TextArea(1, 4)]
    public string text;

    public float Duration => endTime - startTime;
}
