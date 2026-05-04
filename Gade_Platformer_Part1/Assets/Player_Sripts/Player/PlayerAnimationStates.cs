using UnityEngine;

// Shared references used by all states
public class PlayerStateContext
{
    public readonly Player Player;
    public readonly Animator Animator;
    public readonly PlayerStateMachine StateMachine;

    public IPlayerState IdleState;
    public IPlayerState RunState;
    public IPlayerState JumpState;
    public IPlayerState FallState;

    public PlayerStateContext(Player player, Animator animator, PlayerStateMachine stateMachine)
    {
        Player = player;
        Animator = animator;
        StateMachine = stateMachine;
    }
}

public abstract class PlayerBaseState : IPlayerState
{
    protected readonly PlayerStateContext Ctx;

    protected PlayerBaseState(PlayerStateContext context)
    {
        Ctx = context;
    }

    public abstract string Name { get; }
    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
}

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateContext context) : base(context) { }
    public override string Name => "Idle";

    public override void Enter()
    {
        // 0 = Idle
        Ctx.Animator.SetInteger("State", 0);
    }

    public override void Tick()
    {
        // If falling, switch to fall state
        if (!Ctx.Player.IsGrounded)
        {
            Ctx.StateMachine.ChangeState(Ctx.FallState);
            return;
        }

        // If jump pressed, switch to jump state
        if (Ctx.Player.JumpPressedThisFrame)
        {
            Ctx.StateMachine.ChangeState(Ctx.JumpState);
            return;
        }

        // If player starts moving, switch to run state
        if (Ctx.Player.MoveMagnitude > 0.1f)
        {
            Ctx.StateMachine.ChangeState(Ctx.RunState);
        }
    }

    public override void Exit() { }
}

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateContext context) : base(context) { }
    public override string Name => "Run";

    public override void Enter()
    {
        // 1 = Run
        Ctx.Animator.SetInteger("State", 1);
    }

    public override void Tick()
    {
        // Airborne -> Fall
        if (!Ctx.Player.IsGrounded)
        {
            Ctx.StateMachine.ChangeState(Ctx.FallState);
            return;
        }

        // Jump input -> Jump state
        if (Ctx.Player.JumpPressedThisFrame)
        {
            Ctx.StateMachine.ChangeState(Ctx.JumpState);
            return;
        }

        // Stop moving -> Idle
        if (Ctx.Player.MoveMagnitude <= 0.1f)
        {
            Ctx.StateMachine.ChangeState(Ctx.IdleState);
        }
    }

    public override void Exit() { }
}

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateContext context) : base(context) { }
    public override string Name => "Jump";

    public override void Enter()
    {
        // 2 = Jump
        Ctx.Animator.SetInteger("State", 2);
        Ctx.Animator.SetTrigger("JumpTrigger");
    }

    public override void Tick()
    {
        // Start falling after upward velocity ends
        if (Ctx.Player.VerticalVelocity <= 0f)
        {
            Ctx.StateMachine.ChangeState(Ctx.FallState);
        }
    }

    public override void Exit() { }
}

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateContext context) : base(context) { }
    public override string Name => "Fall";

    public override void Enter()
    {
        // 3 = Fall
        Ctx.Animator.SetInteger("State", 3);
    }

    public override void Tick()
    {
        // On landing, choose Run or Idle based on movement input
        if (Ctx.Player.IsGrounded)
        {
            if (Ctx.Player.MoveMagnitude > 0.1f)
                Ctx.StateMachine.ChangeState(Ctx.RunState);
            else
                Ctx.StateMachine.ChangeState(Ctx.IdleState);
        }
    }

    public override void Exit() { }
}
