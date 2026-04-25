using UnityEngine;
using UnityEngine.UI;

public class HorizontalBarInteractable : LookCloserInteractableBase
{
    [SerializeField] private PullUpController _pullUpController;
    [SerializeField] private PullUpUI _pullUpUI;
    [SerializeField] private HorizontalBarSway _barSway;
    [SerializeField] private GameObject _firstPersonHands;
    public override void Interact(Player player = null)
    {
        if (player.Grabbing.IsHolding) return;

        base.Interact(player);
        _pullUpController.Activate(CameraSnapPoint);
    }
    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        var previousType = _previousPlayerStateType; 

        base.On_PlayerStateChanged(type);

        if (type == PlayerStateType.LookCloser)
        {
            _firstPersonHands.gameObject.SetActive(true);
            _barSway.SetActive(true);
            _pullUpUI.Show();
        }
        else if (previousType == PlayerStateType.LookCloser)
        {
            _firstPersonHands.gameObject.SetActive(false);
            _barSway.SetActive(false);
            _pullUpController.Deactivate();
            CameraSnapPoint.ClearExternalPosition();
            _pullUpUI.Hide();
        }
    }
}
