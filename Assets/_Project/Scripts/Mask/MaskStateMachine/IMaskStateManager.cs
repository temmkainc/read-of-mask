using UnityEngine.InputSystem;

public interface IMaskStateManager
{
    public event System.Action<MaskStateType> OnStateChanged;
    public MaskStateType CurrentStateType { get; }

    public void ConfirmStateTransition();

}
