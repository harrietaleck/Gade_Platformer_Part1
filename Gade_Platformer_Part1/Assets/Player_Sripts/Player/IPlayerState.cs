// Every state must implement these 4 functions
public interface IPlayerState
{
    string Name { get; }
    void Enter();
    void Tick();
    void Exit();
}
