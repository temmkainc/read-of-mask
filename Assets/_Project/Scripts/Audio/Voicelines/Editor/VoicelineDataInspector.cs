// Place this file in any Editor/ folder inside your Assets directory.
// Provides a richer Inspector view for VoicelineData assets.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoicelineData))]
public class VoicelineDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        VoicelineData data = (VoicelineData)target;

        // Default fields
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Quick Info", EditorStyles.boldLabel);

        // Clip info
        if (data.AudioClip != null)
        {
            EditorGUILayout.LabelField("Clip length", $"{data.AudioClip.length:F2}s");
            EditorGUILayout.LabelField("Sample rate", $"{data.AudioClip.frequency} Hz");
            EditorGUILayout.LabelField("Channels", $"{data.AudioClip.channels}");
        }

        // Cue summary
        if (data.Subtitles != null && data.Subtitles.Count > 0)
        {
            EditorGUILayout.LabelField("Cue count", data.Subtitles.Count.ToString());

            // Coverage bar
            if (data.AudioClip != null)
            {
                float covered = 0f;
                foreach (var c in data.Subtitles)
                    covered += c.endTime - c.startTime;
                float pct = Mathf.Clamp01(covered / data.AudioClip.length);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coverage", GUILayout.Width(70));
                Rect barArea = EditorGUILayout.GetControlRect(GUILayout.Height(14));
                EditorGUI.DrawRect(barArea, new Color(0.25f, 0.25f, 0.25f));
                EditorGUI.DrawRect(new Rect(barArea.x, barArea.y, barArea.width * pct, barArea.height),
                    pct > 0.8f ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.9f, 0.65f, 0.2f));
                EditorGUI.LabelField(barArea, $" {pct * 100:F0}%", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        // Validation
        EditorGUILayout.Space(4);
        if (data.IsValid())
        {
            EditorGUILayout.HelpBox("Voiceline data is valid.", MessageType.Info);
        }
        else
        {
            string reason = data.AudioClip == null ? "No AudioClip assigned." :
                            (data.Subtitles == null || data.Subtitles.Count == 0) ? "No subtitle cues." :
                            "One or more cues have endTime <= startTime.";
            EditorGUILayout.HelpBox("Incomplete: " + reason, MessageType.Warning);
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Open Subtitle Editor"))
        {
            SubtitleEditorWindow win = EditorWindow.GetWindow<SubtitleEditorWindow>("Subtitle Editor");
            // Use reflection to set _target since it's private
            var field = typeof(SubtitleEditorWindow).GetField("_target",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var clipField = typeof(SubtitleEditorWindow).GetField("_clip",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(win, data);
            clipField?.SetValue(win, data.AudioClip);
            win.Repaint();
        }
    }
}
#endif