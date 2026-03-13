using UnityEngine;
using UnityEngine.UI;

public class GamingCartridgeItem : GrabbableObject
{
    [field: SerializeField] public GamingCartridgeData CartridgeData { get; private set; }
    [field: SerializeField] public Sprite GameCoverSprite { get; private set;  }
    [field: SerializeField] public Image BackCoverImage { get; private set; }
    [field: SerializeField] public Image FrontCoverImage { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        BackCoverImage.sprite = GameCoverSprite;
        FrontCoverImage.sprite = GameCoverSprite;
    }
}
