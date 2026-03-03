using UnityEngine;
using Zenject;

public class GamingScreen : MonoBehaviour, IInteractable
{
    [Inject] InteractionCinemachineCamera _interactionCamera;
    [Inject] ICommandBus _commandBus;

    [field: SerializeField] public Transform CameraSnapPoint {  get; private set; }

    public void Interact(Player player)
    {
        _interactionCamera.CinemachineCamera.Follow = CameraSnapPoint;

        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.Interaction)).Execute();
    }
}
