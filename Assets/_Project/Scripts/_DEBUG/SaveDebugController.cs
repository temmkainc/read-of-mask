using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Dev utility: press Ctrl+Shift+Delete to wipe the save file (see SaveService).
/// Add this component to any GameObject in the scene (same as LetterUnlockDebugPanel).
/// Compiled out of release builds - only active in the Editor and Development Builds.
/// </summary>
public class SaveDebugController : MonoBehaviour
{
    [Inject] private ISaveService _saveService;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool ctrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        bool shift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        if (ctrl && shift && keyboard.deleteKey.wasPressedThisFrame)
        {
            _saveService.ResetSave();
            Debug.Log("[SaveDebugController] Save file cleared (Ctrl+Shift+Delete). Restart/reload the scene to see a fresh game.");
        }
    }
#endif
}
