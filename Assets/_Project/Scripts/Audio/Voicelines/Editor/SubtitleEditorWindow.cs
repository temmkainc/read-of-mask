// Place this file in any Editor/ folder inside your Assets directory.
// Window → Audio → Subtitle Editor

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubtitleEditorWindow : EditorWindow
{
    // ── State ────────────────────────────────────────────────────────────────
    private VoicelineData _target;
    private AudioClip _clip;

    private double _playStartDSP;    // AudioSettings.dspTime when we hit Play
    private double _pausedAt;        // clip time when paused
    private bool _isPlaying;
    private bool _isPaused;

    private float _stampStart = -1f;// pending start time (waiting for end stamp)

    private Vector2 _scrollPos;

    private Texture2D _waveformTex;
    private AudioClip _waveformFor;

    private const float TIMELINE_H = 56f;
    private const float WAVEFORM_H = 48f;
    private const float MIN_CUE_DURATION = 0.1f;

    // ── Menu item ────────────────────────────────────────────────────────────
    [MenuItem("Window/Audio/Subtitle Editor")]
    public static void Open() => GetWindow<SubtitleEditorWindow>("Subtitle Editor");

    // ── GUI ──────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawHeader();

        if (_target == null) { DrawEmpty(); return; }
        if (_clip == null) { EditorGUILayout.HelpBox("Assign an AudioClip to the VoicelineData asset.", MessageType.Warning); return; }

        EditorGUILayout.Space(4);
        DrawTransport();
        EditorGUILayout.Space(6);
        DrawTimeline();
        EditorGUILayout.Space(6);
        DrawStampControls();
        EditorGUILayout.Space(8);
        DrawCueList();
        EditorGUILayout.Space(4);
        DrawFooter();

        // Repaint every frame while playing so the playhead animates.
        if (_isPlaying) Repaint();
    }

    // ── Header ───────────────────────────────────────────────────────────────
    private void DrawHeader()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Subtitle Editor", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        EditorGUI.BeginChangeCheck();
        _target = (VoicelineData)EditorGUILayout.ObjectField(
            _target, typeof(VoicelineData), false, GUILayout.Width(220));
        if (EditorGUI.EndChangeCheck())
        {
            StopPlayback();
            _clip = _target != null ? _target.AudioClip : null;
            _waveformTex = null;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
        DrawDivider();
    }

    private void DrawEmpty()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Select a VoicelineData asset above to begin.",
            new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 12 });
        GUILayout.FlexibleSpace();
    }

    // ── Transport ────────────────────────────────────────────────────────────
    private void DrawTransport()
    {
        EditorGUILayout.BeginHorizontal();

        float currentTime = GetCurrentTime();
        string timeLabel = $"{currentTime:F2}s / {_clip.length:F2}s";
        GUILayout.Label(timeLabel, GUILayout.Width(130));
        GUILayout.FlexibleSpace();

        // Play / Pause
        string playLabel = _isPlaying ? (_isPaused ? "▶ Resume" : "⏸ Pause") : "▶ Play";
        if (GUILayout.Button(playLabel, GUILayout.Width(90)))
        {
            if (!_isPlaying) StartPlayback(0f);
            else if (_isPaused) ResumePlayback();
            else PausePlayback();
        }

        if (GUILayout.Button("⏹ Stop", GUILayout.Width(70))) StopPlayback();
        if (GUILayout.Button("↩ Restart", GUILayout.Width(80))) StartPlayback(0f);

        EditorGUILayout.EndHorizontal();

        // Scrub bar
        EditorGUI.BeginChangeCheck();
        float scrubbed = EditorGUILayout.Slider(currentTime, 0f, _clip.length);
        if (EditorGUI.EndChangeCheck()) SeekTo(scrubbed);
    }

    // ── Timeline / Waveform ──────────────────────────────────────────────────
    private void DrawTimeline()
    {
        Rect area = GUILayoutUtility.GetRect(position.width - 20, WAVEFORM_H + TIMELINE_H + 4);

        Rect waveRect = new Rect(area.x, area.y, area.width, WAVEFORM_H);
        Rect cueRect = new Rect(area.x, area.y + WAVEFORM_H + 4, area.width, TIMELINE_H);

        DrawWaveform(waveRect);
        DrawCueBar(cueRect);
        DrawPlayhead(waveRect, cueRect);
    }

    private void DrawWaveform(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

        if (_waveformTex == null || _waveformFor != _clip)
            RebuildWaveform((int)rect.width, (int)rect.height);

        if (_waveformTex != null)
            GUI.DrawTexture(rect, _waveformTex, ScaleMode.StretchToFill);
    }

    private void RebuildWaveform(int w, int h)
    {
        if (_clip == null || w <= 0 || h <= 0) return;

        // Get samples — works for compressed clips too.
        float[] samples = new float[_clip.samples * _clip.channels];
        _clip.GetData(samples, 0);

        _waveformTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color bg = new Color(0.15f, 0.15f, 0.15f, 1f);
        Color wave = new Color(0.35f, 0.75f, 0.45f, 1f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;

        int channels = _clip.channels;
        int totalSamples = samples.Length / channels;

        for (int x = 0; x < w; x++)
        {
            int startSample = Mathf.FloorToInt((float)x / w * totalSamples);
            int endSample = Mathf.FloorToInt((float)(x + 1) / w * totalSamples);
            endSample = Mathf.Min(endSample, totalSamples - 1);

            float peak = 0f;
            for (int s = startSample; s <= endSample; s++)
                peak = Mathf.Max(peak, Mathf.Abs(samples[s * channels]));

            int barH = Mathf.FloorToInt(peak * h * 0.9f);
            int mid = h / 2;
            for (int y = mid - barH / 2; y <= mid + barH / 2; y++)
            {
                if (y >= 0 && y < h)
                    pixels[y * w + x] = wave;
            }
        }

        _waveformTex.SetPixels(pixels);
        _waveformTex.Apply();
        _waveformFor = _clip;
    }

    private void DrawCueBar(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

        if (_target.Subtitles == null) return;
        float duration = _clip.length;

        for (int i = 0; i < _target.Subtitles.Count; i++)
        {
            var cue = _target.Subtitles[i];
            float x0 = rect.x + (cue.startTime / duration) * rect.width;
            float x1 = rect.x + (cue.endTime / duration) * rect.width;

            Color c = ColorForIndex(i);
            EditorGUI.DrawRect(new Rect(x0, rect.y + 2, x1 - x0, rect.height - 4), c);

            // Short text label if wide enough
            float cueW = x1 - x0;
            if (cueW > 36)
            {
                GUI.color = Color.white;
                string lbl = cue.text.Length > 12 ? cue.text.Substring(0, 12) + "…" : cue.text;
                GUI.Label(new Rect(x0 + 2, rect.y + 4, cueW - 4, rect.height - 8),
                    lbl, EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
        }

        // Show pending stamp start
        if (_stampStart >= 0f)
        {
            float px = rect.x + (_stampStart / duration) * rect.width;
            EditorGUI.DrawRect(new Rect(px - 1, rect.y, 2, rect.height), Color.yellow);
        }
    }

    private void DrawPlayhead(Rect waveRect, Rect cueRect)
    {
        float t = GetCurrentTime();
        float px = waveRect.x + (t / _clip.length) * waveRect.width;

        EditorGUI.DrawRect(new Rect(px - 1, waveRect.y, 2, waveRect.height + TIMELINE_H + 4), Color.red);
    }

    // ── Stamp controls ───────────────────────────────────────────────────────
    private void DrawStampControls()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Stamp Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        Color prev = GUI.backgroundColor;

        if (_stampStart < 0f)
        {
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
            if (GUILayout.Button("[ Mark Start  (S)", GUILayout.Height(32)))
                StampStart();
        }
        else
        {
            GUILayout.Label($"Start locked: {_stampStart:F2}s — now speak…",
                EditorStyles.boldLabel, GUILayout.Height(32));

            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.4f);
            if (GUILayout.Button("] Mark End  (E)", GUILayout.Height(32)))
                StampEnd();

            GUI.backgroundColor = new Color(0.9f, 0.85f, 0.3f);
            if (GUILayout.Button("✕ Cancel", GUILayout.Width(70), GUILayout.Height(32)))
                _stampStart = -1f;
        }

        GUI.backgroundColor = prev;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "1) Hit Play.  2) Press S when speech starts.  3) Press E when it ends — a cue is created.\n" +
            "Repeat for each line. Edit text in the cue list below.", MessageType.None);

        EditorGUILayout.EndVertical();
    }

    // ── Keyboard shortcuts ───────────────────────────────────────────────────
    private void OnInspectorUpdate() => Repaint();

    private void OnFocus() { } // needed so key events arrive
    private void Update() { }

    private void ProcessKeys()
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;
        if (e.keyCode == KeyCode.S) { StampStart(); e.Use(); }
        if (e.keyCode == KeyCode.E) { StampEnd(); e.Use(); }
        if (e.keyCode == KeyCode.Space)
        {
            if (!_isPlaying) StartPlayback(GetCurrentTime());
            else if (_isPaused) ResumePlayback();
            else PausePlayback();
            e.Use();
        }
    }

    // ── Cue list ─────────────────────────────────────────────────────────────
    private void DrawCueList()
    {
        GUILayout.Label("Subtitle Cues", EditorStyles.boldLabel);

        if (_target.Subtitles == null || _target.Subtitles.Count == 0)
        {
            EditorGUILayout.HelpBox("No cues yet. Use Stamp Controls or click '+ Add Cue'.", MessageType.Info);
        }
        else
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(280));

            int toDelete = -1;
            for (int i = 0; i < _target.Subtitles.Count; i++)
            {
                var cue = _target.Subtitles[i];
                Color cb = ColorForIndex(i);
                Rect row = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.DrawRect(new Rect(row.x, row.y, 4, row.height), cb);

                // Time row
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.Label($"#{i + 1}", GUILayout.Width(24));

                EditorGUI.BeginChangeCheck();
                float s = EditorGUILayout.FloatField("Start", cue.startTime, GUILayout.Width(130));
                float e2 = EditorGUILayout.FloatField("End", cue.endTime, GUILayout.Width(130));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Edit Cue Time");
                    cue.startTime = Mathf.Clamp(s, 0f, _clip.length);
                    cue.endTime = Mathf.Clamp(e2, cue.startTime + MIN_CUE_DURATION, _clip.length);
                    EditorUtility.SetDirty(_target);
                }

                GUILayout.Label($"{cue.Duration:F2}s", GUILayout.Width(46));
                GUILayout.FlexibleSpace();

                // Jump to this cue
                if (GUILayout.Button("▶", GUILayout.Width(24))) SeekTo(cue.startTime);

                // Delete
                Color prevBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                if (GUILayout.Button("✕", GUILayout.Width(24))) toDelete = i;
                GUI.backgroundColor = prevBg;

                EditorGUILayout.EndHorizontal();

                // Text area
                EditorGUI.BeginChangeCheck();
                string newText = EditorGUILayout.TextArea(cue.text, GUILayout.MinHeight(32));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Edit Cue Text");
                    cue.text = newText;
                    EditorUtility.SetDirty(_target);
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }

            if (toDelete >= 0)
            {
                Undo.RecordObject(_target, "Delete Cue");
                _target.Subtitles.RemoveAt(toDelete);
                EditorUtility.SetDirty(_target);
            }

            EditorGUILayout.EndScrollView();
        }

        // Add cue manually
        if (GUILayout.Button("+ Add Cue at Current Time"))
        {
            Undo.RecordObject(_target, "Add Cue");
            float t = GetCurrentTime();
            _target.Subtitles.Add(new SubtitleCue
            {
                startTime = t,
                endTime = Mathf.Min(t + 2f, _clip.length),
                text = "New subtitle text"
            });
            SortCues();
            EditorUtility.SetDirty(_target);
        }
    }

    // ── Footer / Validation ──────────────────────────────────────────────────
    private void DrawFooter()
    {
        DrawDivider();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Sort Cues by Time"))
        {
            Undo.RecordObject(_target, "Sort Cues");
            SortCues();
            EditorUtility.SetDirty(_target);
        }

        if (GUILayout.Button("Clear All Cues"))
        {
            if (EditorUtility.DisplayDialog("Clear All Cues",
                "Delete every cue from this VoicelineData? This cannot be undone via Undo.", "Clear", "Cancel"))
            {
                Undo.RecordObject(_target, "Clear Cues");
                _target.Subtitles.Clear();
                EditorUtility.SetDirty(_target);
            }
        }

        GUILayout.FlexibleSpace();

        if (_target.IsValid())
        {
            GUI.color = Color.green;
            GUILayout.Label("Valid", EditorStyles.boldLabel);
        }
        else
        {
            GUI.color = new Color(1f, 0.7f, 0.2f);
            GUILayout.Label("Invalid / Incomplete", EditorStyles.boldLabel);
        }
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    // ── Playback helpers ─────────────────────────────────────────────────────
    private void StartPlayback(float fromTime)
    {
        if (_clip == null) return;
        AudioUtilityWrapper.PlayClip(_clip, Mathf.RoundToInt(fromTime * _clip.frequency));
        _playStartDSP = AudioSettings.dspTime - fromTime;
        _isPlaying = true;
        _isPaused = false;
    }

    private void PausePlayback()
    {
        _pausedAt = GetCurrentTime();
        AudioUtilityWrapper.StopClip(_clip);
        _isPaused = true;
        _isPlaying = true;  // still conceptually playing (paused state)
    }

    private void ResumePlayback()
    {
        _isPaused = false;
        StartPlayback((float)_pausedAt);
    }

    private void StopPlayback()
    {
        if (_clip != null) AudioUtilityWrapper.StopClip(_clip);
        _isPlaying = false;
        _isPaused = false;
        _pausedAt = 0;
    }

    private void SeekTo(float time)
    {
        bool wasPlaying = _isPlaying && !_isPaused;
        StopPlayback();
        _pausedAt = time;
        _isPaused = true;
        _isPlaying = true;
        if (wasPlaying) StartPlayback(time);
    }

    private float GetCurrentTime()
    {
        if (!_isPlaying || _clip == null) return 0f;
        if (_isPaused) return (float)_pausedAt;
        float t = (float)(AudioSettings.dspTime - _playStartDSP);
        return Mathf.Clamp(t, 0f, _clip.length);
    }

    // ── Stamp helpers ────────────────────────────────────────────────────────
    private void StampStart()
    {
        _stampStart = GetCurrentTime();
    }

    private void StampEnd()
    {
        if (_stampStart < 0f) return;

        float endTime = GetCurrentTime();
        if (endTime <= _stampStart + MIN_CUE_DURATION)
            endTime = _stampStart + MIN_CUE_DURATION;

        Undo.RecordObject(_target, "Stamp Cue");
        _target.Subtitles.Add(new SubtitleCue
        {
            startTime = _stampStart,
            endTime = endTime,
            text = "Enter subtitle text here"
        });
        SortCues();
        EditorUtility.SetDirty(_target);
        _stampStart = -1f;
    }

    // ── Utilities ────────────────────────────────────────────────────────────
    private void SortCues()
    {
        _target.Subtitles.Sort((a, b) => a.startTime.CompareTo(b.startTime));
    }

    private Color ColorForIndex(int i)
    {
        Color[] palette =
        {
            new Color(0.35f, 0.60f, 0.90f, 0.75f),
            new Color(0.45f, 0.82f, 0.56f, 0.75f),
            new Color(0.90f, 0.65f, 0.30f, 0.75f),
            new Color(0.80f, 0.45f, 0.80f, 0.75f),
            new Color(0.90f, 0.42f, 0.42f, 0.75f),
            new Color(0.42f, 0.82f, 0.82f, 0.75f),
        };
        return palette[i % palette.Length];
    }

    private void DrawDivider()
    {
        Rect r = GUILayoutUtility.GetRect(position.width, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f));
    }

    private void OnDestroy() => StopPlayback();
}

/// <summary>
/// Thin wrapper around Unity's internal AudioUtil via reflection.
/// Avoids a hard dependency on the internal API so the project compiles on all Unity versions.
/// </summary>
internal static class AudioUtilityWrapper
{
    private static System.Type _audioUtil;

    private static System.Type AudioUtil
    {
        get
        {
            if (_audioUtil == null)
                _audioUtil = System.Type.GetType(
                    "UnityEditor.AudioUtil, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            return _audioUtil;
        }
    }

    public static void PlayClip(AudioClip clip, int startSample = 0)
    {
        var method = AudioUtil?.GetMethod("PlayPreviewClip",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        if (method != null)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 3)
                method.Invoke(null, new object[] { clip, startSample, false });
            else
                method.Invoke(null, new object[] { clip });
        }
    }

    public static void StopClip(AudioClip clip)
    {
        var method = AudioUtil?.GetMethod("StopAllPreviewClips",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            ?? AudioUtil?.GetMethod("StopPreviewClip",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        method?.Invoke(null, method.GetParameters().Length == 0 ? null : new object[] { clip });
    }
}
#endif