using ReadOfMask.Core.StateMachine;
using Zenject;

public abstract class PlayerState : IState
{
    private CinemachineManager _cinemachineManager;
    private InputsManager _inputsManager;
    protected ICommandBus CommandBus { get; private set; }

    public PlayerStateData Data { get; private set; }
    protected PlayerState(PlayerStateData data) 
    {
        Data = data;
    }

    [Inject]
    public void Construct(CinemachineManager cinemachineManager, InputsManager inputManager, ICommandBus commandBus)
    {
        _cinemachineManager = cinemachineManager;
        _inputsManager = inputManager;
        CommandBus = commandBus;

        Initialize();
    }

    protected virtual void Initialize() { }

    public virtual void Enter()
    {
        _cinemachineManager.SwitchCamera(Data.Camera);
        _inputsManager.SwitchActionMap(Data.ActionMapType);
        _inputsManager.SetCursorLockState(Data.CursorLockMode);
    }

    public virtual void Exit()
    {
        
    }
}
