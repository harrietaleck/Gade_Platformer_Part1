public class PlayerStateMachine
{
    // Current active state (Idle/Run/Jump/Fall)
    public IPlayerState CurrentState { get; private set; }

    public void Initialize(IPlayerState startState)
    {
        // Set first state when game starts
        CurrentState = startState;
        CurrentState?.Enter();
    }

    public void ChangeState(IPlayerState nextState)
    {
        // Ignore invalid or same-state changes
        if (nextState == null || nextState == CurrentState)
            return;

        // Exit old state, enter new state
        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        // Update current state each frame
        CurrentState?.Tick();
    }
}
