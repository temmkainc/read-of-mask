using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A scene reference that survives renaming the scene file - drag the actual .unity asset in
/// here instead of typing its name. Unity tracks the asset reference by GUID, so if you rename
/// the scene later, this field keeps pointing at it correctly with no manual fixing needed.
/// Use implicitly as a string (e.g. SceneManager.LoadScene(mySceneField)) to get its build name.
/// </summary>
[System.Serializable]
public class SceneField
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField] private string sceneName;

    public string SceneName => sceneName;

    public static implicit operator string(SceneField sceneField) => sceneField != null ? sceneField.SceneName : string.Empty;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
        var sceneNameProperty = property.FindPropertyRelative("sceneName");

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUI.BeginChangeCheck();
        var newSceneAsset = EditorGUI.ObjectField(position, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            sceneAssetProperty.objectReferenceValue = newSceneAsset;
            sceneNameProperty.stringValue = newSceneAsset != null ? newSceneAsset.name : string.Empty;
        }

        EditorGUI.EndProperty();
    }
}
#endif
