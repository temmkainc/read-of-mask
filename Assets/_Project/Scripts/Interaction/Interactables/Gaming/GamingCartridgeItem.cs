using UnityEngine;

public class GamingCartridgeItem : GrabbableObject
{
    [field: SerializeField] public GamingCartridgeData CartridgeData { get; private set; }
}
