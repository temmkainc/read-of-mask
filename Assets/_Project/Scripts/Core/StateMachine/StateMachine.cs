namespace ReadOfMask.Core.StateMachine
{
    public class StateMachine<TState> where TState : IState
    {
        protected TState CurrentState { get; private set; }

        public void ChangeState(TState state)
        {
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }
    }
}