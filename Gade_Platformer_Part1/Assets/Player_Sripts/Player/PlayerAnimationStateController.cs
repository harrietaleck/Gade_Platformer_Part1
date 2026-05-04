using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    private Player player;
    private PlayerStateMachine stateMachine;
    private PlayerStateContext stateContext;

    private void Awake()
    {
        // Read Player movement values
        player = GetComponent<Player>();

        // Auto-find Animator if not assigned
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Create state machine and all state objects
        stateMachine = new PlayerStateMachine();
        stateContext = new PlayerStateContext(player, animator, stateMachine);

        stateContext.IdleState = new PlayerIdleState(stateContext);
        stateContext.RunState = new PlayerRunState(stateContext);
        stateContext.JumpState = new PlayerJumpState(stateContext);
        stateContext.FallState = new PlayerFallState(stateContext);
    }

    private void Start()
    {
        // Stop script if Animator is missing
        if (animator == null)
        {
            Debug.LogWarning("PlayerAnimationStateController needs an Animator reference.");
            enabled = false;
            return;
        }

        // Start in Idle
        stateMachine.Initialize(stateContext.IdleState);
    }

    private void Update()
    {
        // Keep Animator parameters updated every frame
        animator.SetBool("IsGrounded", player.IsGrounded);
        animator.SetFloat("Speed", player.MoveMagnitude);
        animator.SetFloat("VerticalVelocity", player.VerticalVelocity);

        // Run current state logic
        stateMachine.Tick();
    }
}
