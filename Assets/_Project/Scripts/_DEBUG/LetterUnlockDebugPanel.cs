using UnityEngine;
using Zenject;

public class LetterUnlockDebugPanel : MonoBehaviour
{
    [Inject] private UnlockedLettersManager _lettersManager;

    private string _letterToUnlock = "A";

    private void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(200));
        GUILayout.Label("Letter Unlock Debug Panel");

        // Input for a letter
        GUILayout.BeginHorizontal();
        GUILayout.Label("Letter:", GUILayout.Width(40));
        _letterToUnlock = GUILayout.TextField(_letterToUnlock, 1, GUILayout.Width(30));
        GUILayout.EndHorizontal();

        // Unlock the typed letter
        if (GUILayout.Button("Unlock Letter"))
        {
            if (!string.IsNullOrEmpty(_letterToUnlock))
            {
                char c = char.ToUpper(_letterToUnlock[0]);
                _lettersManager.UnlockLetter(c);
            }
        }

        // Reset everything (lock all letters)
        if (GUILayout.Button("Reset / Lock All"))
        {
            _lettersManager.ClearUnlockedLetters();
        }

        GUILayout.EndVertical();
    }
}