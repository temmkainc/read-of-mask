using UnityEngine;

/// <summary>
/// Optional room reverb ("echo of a closed space") for one AudioSource. Tick it on for sources that
/// sit in small or enclosed spaces, pick the kind of room, and set how much of it you hear.
/// Uses Unity's reverb presets as the room character; Amount scales how loud the reverb is.
/// When off, the filter is disabled and costs nothing.
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(AudioReverbFilter))]
public class AudioRoomReverb : MonoBehaviour
{
    [SerializeField] private bool _reverbOn;
    [Tooltip("The kind of space the sound is in.")]
    [SerializeField] private AudioReverbPreset _room = AudioReverbPreset.Room;
    [Tooltip("How much reverb you hear: 0 = none, 0.5 = the preset as designed, 1 = very wet.")]
    [SerializeField][Range(0f, 1f)] private float _amount = 0.5f;

    private AudioReverbFilter _filter;

    public bool ReverbOn
    {
        get => _reverbOn;
        set { _reverbOn = value; Apply(); }
    }

    private void OnEnable() => Apply();

    private void OnValidate()
    {
        // keep the inspector live, also while playing
        if (_room == AudioReverbPreset.User || _room == AudioReverbPreset.Off) _room = AudioReverbPreset.Room;
        Apply();
    }

    private void Apply()
    {
        if (_filter == null && !TryGetComponent(out _filter)) return;

        _filter.enabled = _reverbOn;
        if (!_reverbOn) return;

        // load the room's character, then switch to User so the level can be adjusted on top of it
        _filter.reverbPreset = _room;
        float presetLevel = _filter.room;
        _filter.reverbPreset = AudioReverbPreset.User;
        _filter.room = _amount <= 0.5f
            ? Mathf.Lerp(-4000f, presetLevel, _amount / 0.5f)                       // fade the reverb in
            : Mathf.Lerp(presetLevel, Mathf.Min(0f, presetLevel + 1000f), (_amount - 0.5f) / 0.5f);
    }
}
