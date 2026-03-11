using UnityEngine;

[CreateAssetMenu(fileName = "GamingCartridgeSO", menuName = "Scriptable Objects/GamingCartridgeSO")]
public class GamingCartridgeData : ScriptableObject
{
    public string GameName;
    public MinigameType MinigameType;
}
