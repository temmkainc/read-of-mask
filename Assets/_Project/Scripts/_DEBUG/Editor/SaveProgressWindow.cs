// Tools → Read of Mask → Save Progress
// Dev utility: pick which scene and level the save file starts at, without playing up to it.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveProgressWindow : EditorWindow
{
    private const string SaveFileName = "save.json";
    private const string LevelsPropertyPath = "_coreModuleConfigData.<Levels>k__BackingField";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private SaveData _data;
    private string _loadError;
    private int _sceneChoice;
    private int _manualLevel;
    private Vector2 _scroll;

    // ── Menu item ────────────────────────────────────────────────────────────
    [MenuItem("Tools/Read of Mask/Save Progress")]
    public static void Open() => GetWindow<SaveProgressWindow>("Save Progress");

    private void OnEnable()
    {
        Reload();
        EditorApplication.playModeStateChanged += On_PlayModeStateChanged;
    }

    private void OnDisable() => EditorApplication.playModeStateChanged -= On_PlayModeStateChanged;

    private void OnFocus() => Reload();

    // the game writes the save while playing, so show what it left behind
    private void On_PlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            Reload();
            Repaint();
        }
    }

    // ── GUI ──────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawCurrentSave();
        EditorGUILayout.Space(8);

        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Leave Play mode to change the save - the running game keeps its own copy and would write over it.", MessageType.Info);
            return;
        }

        DrawLevelPicker();
        EditorGUILayout.Space(8);
        DrawFooter();
    }

    private void DrawCurrentSave()
    {
        EditorGUILayout.LabelField("Current Save", EditorStyles.boldLabel);

        if (_loadError != null)
        {
            EditorGUILayout.HelpBox(_loadError, MessageType.Error);
            return;
        }

        if (!File.Exists(SavePath))
        {
            EditorGUILayout.HelpBox("No save file yet - the game starts from the first level.", MessageType.None);
            return;
        }

        string scene = string.IsNullOrEmpty(_data.CurrentSceneName) ? "(first scene that loads)" : _data.CurrentSceneName;
        string level = LevelLabel(_data.CurrentSceneName, _data.CurrentLevelIndex);
        EditorGUILayout.LabelField("Starts at", $"{scene}  ›  {level}");
        EditorGUILayout.LabelField("Completed levels", _data.CompletedLevelKeys.Count == 0 ? "none" : string.Join(", ", _data.CompletedLevelKeys));
    }

    private void DrawLevelPicker()
    {
        EditorGUILayout.LabelField("Start The Game At", EditorStyles.boldLabel);

        string[] scenes = BuildSceneNames();
        if (scenes.Length == 0)
        {
            EditorGUILayout.HelpBox("No scenes in Build Settings.", MessageType.Warning);
            return;
        }

        _sceneChoice = Mathf.Clamp(_sceneChoice, 0, scenes.Length - 1);
        _sceneChoice = EditorGUILayout.Popup("Scene", _sceneChoice, scenes);
        string sceneName = scenes[_sceneChoice];

        List<string> levels = LevelNames(sceneName);
        if (levels == null)
        {
            // the level list lives in the scene, so names are only known while it's open
            EditorGUILayout.HelpBox($"Open {sceneName} to see its levels by name.", MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                _manualLevel = Mathf.Max(0, EditorGUILayout.IntField("Level index", _manualLevel));
                if (GUILayout.Button("Start here", GUILayout.Width(90)))
                    SetProgress(sceneName, _manualLevel);
            }
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (int i = 0; i < levels.Count; i++)
        {
            bool isCurrent = _data != null && _data.CurrentSceneName == sceneName && _data.CurrentLevelIndex == i;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{i}   {levels[i]}", isCurrent ? EditorStyles.boldLabel : EditorStyles.label);
                using (new EditorGUI.DisabledScope(isCurrent))
                {
                    if (GUILayout.Button(isCurrent ? "Current" : "Start here", GUILayout.Width(90)))
                        SetProgress(sceneName, i);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawFooter()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Show Save File"))
                EditorUtility.RevealInFinder(File.Exists(SavePath) ? SavePath : Application.persistentDataPath);

            using (new EditorGUI.DisabledScope(!File.Exists(SavePath)))
            {
                if (GUILayout.Button("Delete Save") &&
                    EditorUtility.DisplayDialog("Delete save?", "The game will start from the very beginning, with nothing unlocked.", "Delete", "Cancel"))
                {
                    File.Delete(SavePath);
                    Reload();
                }
            }
        }
    }

    // ── Save file ────────────────────────────────────────────────────────────

    /// <summary>
    /// Points the save at a level and makes the completed levels match: everything before it
    /// done, it and everything after it not. Unlocked letters and actions are left as they are.
    /// </summary>
    public void SetProgress(string sceneName, int levelIndex)
    {
        if (_data == null)
            return;

        string[] scenes = BuildSceneNames();
        int sceneOrder = System.Array.IndexOf(scenes, sceneName);

        var completed = new List<string>();
        foreach (string key in _data.CompletedLevelKeys)
        {
            if (!TryParseKey(key, out string keyScene, out int keyLevel))
                continue;

            int keyOrder = System.Array.IndexOf(scenes, keyScene);
            if (keyScene == sceneName ? keyLevel < levelIndex : keyOrder < sceneOrder)
                completed.Add(key);
        }

        for (int i = 0; i < levelIndex; i++)
        {
            string key = $"{sceneName}:{i}";
            if (!completed.Contains(key))
                completed.Add(key);
        }

        _data.CurrentSceneName = sceneName;
        _data.CurrentLevelIndex = levelIndex;
        _data.CompletedLevelKeys = completed;
        Write();
        Debug.Log($"[SaveProgress] The game now starts at {sceneName} › {LevelLabel(sceneName, levelIndex)}.");
    }

    private void Reload()
    {
        _loadError = null;
        try
        {
            _data = File.Exists(SavePath) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) ?? new SaveData() : new SaveData();
        }
        catch (System.Exception e)
        {
            _data = null;
            _loadError = $"Couldn't read the save file: {e.Message}";
            return;
        }

        // start the picker on the scene the save is in, or else the scene that's open
        string[] scenes = BuildSceneNames();
        string preferred = !string.IsNullOrEmpty(_data.CurrentSceneName) ? _data.CurrentSceneName : SceneManager.GetActiveScene().name;
        int index = System.Array.IndexOf(scenes, preferred);
        if (index >= 0)
            _sceneChoice = index;
    }

    private void Write()
    {
        string dir = Path.GetDirectoryName(SavePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(SavePath, JsonUtility.ToJson(_data, true));
    }

    private static bool TryParseKey(string key, out string scene, out int level)
    {
        scene = null;
        level = -1;
        int colon = key.LastIndexOf(':');
        if (colon <= 0)
            return false;
        scene = key.Substring(0, colon);
        return int.TryParse(key.Substring(colon + 1), out level);
    }

    // ── Scenes and levels ────────────────────────────────────────────────────

    private static string[] BuildSceneNames() =>
        EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => Path.GetFileNameWithoutExtension(s.path)).ToArray();

    /// <summary>The level names from the scene's installer, or null if that scene isn't open.</summary>
    public static List<string> LevelNames(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var installer in root.GetComponentsInChildren<AppInstaller>(true))
            {
                var levels = new SerializedObject(installer).FindProperty(LevelsPropertyPath);
                if (levels == null || !levels.isArray)
                    continue;

                var names = new List<string>();
                for (int i = 0; i < levels.arraySize; i++)
                {
                    var level = levels.GetArrayElementAtIndex(i).objectReferenceValue;
                    names.Add(level != null ? $"{level.name}  ({level.GetType().Name})" : "(missing)");
                }
                return names;
            }
        }
        return null;
    }

    private static string LevelLabel(string sceneName, int index)
    {
        var names = string.IsNullOrEmpty(sceneName) ? null : LevelNames(sceneName);
        return names != null && index >= 0 && index < names.Count ? $"{index}  {names[index]}" : $"level {index}";
    }
}
#endif
