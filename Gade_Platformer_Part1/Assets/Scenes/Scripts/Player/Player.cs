using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// First-person platformer player.
/// Drives a CharacterController from WASD + Space + LeftShift,
/// and feeds the "State" int parameter on the Animator:
///   0 = Idle, 1 = Walk, 2 = Run, 3 = Fall, 4 = Jump.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    // --- State IDs that match the Animator Controller's "State" parameter ---
    public const int StateIdle = 0;
    public const int StateWalk = 1;
    public const int StateRun  = 2;
    public const int StateFall = 3;
    public const int StateJump = 4;

    [Header("Movement")]
    [Tooltip("Walking speed (no Shift held).")]
    public float walkSpeed = 10f;
    [Tooltip("Running speed (Shift held).")]
    public float runSpeed = 20f;
    [Tooltip("Back-compat: legacy single speed used by mud/freeze triggers. " +
             "Setting this overrides both walk and run speeds.")]
    public float moveSpeed
    {
        get => Mathf.Max(walkSpeed, runSpeed);
        set { walkSpeed = value; runSpeed = value; }
    }
    [Tooltip("Jump height in metres.")]
    public float jumpHeight = 1.2f;
    [Tooltip("Gravity applied while airborne (negative = down).")]
    public float gravity = -30f;

    [Header("Look")]
    [Tooltip("Mouse-look horizontal sensitivity.")]
    public float mouseSensitivity = 2f;

    [Header("Animation")]
    [Tooltip("Animator on the visible character. Auto-resolved from children if left empty.")]
    public Animator animator;
    [Tooltip("Animator int parameter that drives the locomotion state.")]
    public string stateParameter = "State";
    [Tooltip("Movement input magnitude required before we leave Idle.")]
    public float moveDeadzone = 0.05f;
    [Tooltip("Vertical velocity above this threshold counts as 'going up' (Jump). " +
             "Below it (or <=0) counts as 'going down' (Fall).")]
    public float jumpVelocityThreshold = 0.1f;
    [Tooltip("Disable animation driving (useful when isolating movement bugs).")]
    public bool disableAnimationForConflictTest = false;

    // --- Runtime state ---
    private CharacterController controller;
    private float verticalVelocity;
    private Vector2 moveAxis;
    private bool jumpPressedThisFrame;
    private bool runHeld;
    // True only during the upward arc of a Space-initiated jump.
    // Cleared on landing or once we cross the apex. Walking off a ledge
    // does NOT set this, so the player goes straight to Fall.
    private bool jumpAscending;

    // --- Hurt state ---
    // isHurt: true for hurtDuration seconds after TriggerHurt() is called.
    // While hurt, UpdateAnimationState() overrides normal locomotion and
    // plays the Fall clip as a "knockback" visual proxy.
    private bool  isHurt       = false;
    private float hurtEndTime  = -1f;
    private const float hurtDuration = 0.5f;

    // --- SFX state (Part 3 D3) ---
    // wasGrounded: remembers last frame's grounded state so we can detect
    // the exact frame the player lands (transition from air -> ground).
    private bool wasGrounded;
    // Footstep throttle: prevents the walk/run clip firing every frame.
    // The sound fires at most once every WalkSoundInterval seconds.
    private float lastWalkSoundTime = -999f;
    private const float WalkSoundInterval = 0.4f;

    // --- Public read-only accessors used by other scripts (checkpoints etc.) ---
    public bool IsGrounded => controller != null && controller.isGrounded;
    public float VerticalVelocity => verticalVelocity;
    public float MoveMagnitude => moveAxis.magnitude;
    public bool JumpPressedThisFrame => jumpPressedThisFrame;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        ResolveAnimator();
    }

    private void Update()
    {
        ReadInput();
        RotateFromMouse();

        // Snapshot grounded state BEFORE moving so we can detect the landing frame.
        // On the frame the player touches ground, wasGrounded=false but isGrounded=true.
        wasGrounded = controller.isGrounded;

        Vector3 horizontal = BuildHorizontalVelocity();
        verticalVelocity = ComputeVerticalVelocity();

        Vector3 velocity = horizontal;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // --- SFX: landing sound (Part 3 D3) ---
        // Fires once on the exact frame the player transitions from air to ground.
        if (!wasGrounded && controller.isGrounded)
            SFXManager.Instance?.PlaySound("land");

        // --- SFX: footstep sounds (Part 3 D3) ---
        // Only fires when grounded and moving, throttled to WalkSoundInterval
        // so the clip doesn't restart every frame (which would sound like silence).
        if (controller.isGrounded && moveAxis.magnitude > moveDeadzone &&
            Time.time - lastWalkSoundTime >= WalkSoundInterval)
        {
            // Play run loop when Shift is held, walk loop otherwise.
            SFXManager.Instance?.PlaySound(runHeld ? "run" : "walk");
            lastWalkSoundTime = Time.time;
        }

        // Clear the hurt flag once the brief hurt window has elapsed.
        if (isHurt && Time.time >= hurtEndTime)
            isHurt = false;

        UpdateAnimationState();
    }

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>
    /// Called by AiAttack when the player takes a hit.
    /// Plays the Fall animation briefly as a knockback visual, and triggers
    /// the screen-flash effect on PlayerHurtEffect if it is attached.
    /// </summary>
    public void TriggerHurt()
    {
        isHurt      = true;
        hurtEndTime = Time.time + hurtDuration;

        // Screen flash — PlayerHurtEffect is an optional component on this GameObject.
        GetComponent<PlayerHurtEffect>()?.TriggerFlash();
    }

    // --- Input ----------------------------------------------------------------

    private void ReadInput()
    {
        moveAxis = ReadMoveAxis();
        jumpPressedThisFrame = ReadJumpPressed();
        runHeld = ReadRunHeld();
    }

    private static Vector2 ReadMoveAxis()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 axis = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed) axis.x -= 1f;
            if (kb.dKey.isPressed) axis.x += 1f;
            if (kb.sKey.isPressed) axis.y -= 1f;
            if (kb.wKey.isPressed) axis.y += 1f;
        }
        return axis;
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    private static bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private static bool ReadRunHeld()
    {
        // Project has Active Input Handling = Both, so check whichever path reports true.
        // We accept Left or Right Shift to avoid keyboard-layout surprises.
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed))
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return true;
#endif
        return false;
    }

    public float ReadMoveInputMagnitudeNow() => ReadMoveAxis().magnitude;

    // --- Movement -------------------------------------------------------------

    private void RotateFromMouse()
    {
        float mouseX;
#if ENABLE_INPUT_SYSTEM
        mouseX = Mouse.current != null ? Mouse.current.delta.ReadValue().x : 0f;
#else
        mouseX = Input.GetAxis("Mouse X");
#endif
        transform.Rotate(Vector3.up, mouseX * mouseSensitivity);
    }

    private Vector3 BuildHorizontalVelocity()
    {
        Vector3 dir = transform.right * moveAxis.x + transform.forward * moveAxis.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float speed = runHeld ? runSpeed : walkSpeed;
        return dir * speed;
    }

    private float ComputeVerticalVelocity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            jumpAscending = false; // landed -> jump arc is over
        }

        if (jumpPressedThisFrame && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpAscending = true; // deliberate jump just started

            // --- SFX: jump sound (Part 3 D3) ---
            // Plays once per jump on the frame Space is pressed.
            // Uses the null-conditional (?.) so nothing breaks if SFXManager
            // hasn't loaded yet (e.g. very first frame of the game).
            SFXManager.Instance?.PlaySound("jump");
        }

        // Once we cross the apex, we're falling, not jumping anymore.
        if (jumpAscending && verticalVelocity <= jumpVelocityThreshold)
            jumpAscending = false;

        return verticalVelocity + gravity * Time.deltaTime;
    }

    // --- Animation ------------------------------------------------------------

    private void ResolveAnimator()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            return;
        }

        // Search children first (typical setup: character model is a child of the player root).
        foreach (var a in GetComponentsInChildren<Animator>(includeInactive: true))
        {
            if (a.transform == transform) continue; // prefer child; check root below if nothing found
            animator = a;
            animator.applyRootMotion = false;
            return;
        }

        // Fall back: animator is on the root GameObject itself.
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            return;
        }

        Debug.LogWarning($"[Player] No Animator found on '{name}' or its children. " +
                         "Drag one into the Animator field in the Inspector.");
    }

    private void UpdateAnimationState()
    {
        if (animator == null || disableAnimationForConflictTest) return;

        int state;

        // While hurt: play the Fall clip as a brief knockback visual.
        // This overrides the normal locomotion state for hurtDuration seconds.
        if (isHurt)
        {
            state = StateFall;
        }
        else if (!controller.isGrounded)
        {
            // Jump only fires while we're in the upward arc of a Space-initiated jump.
            // Walking off a ledge or rebounding on geometry goes straight to Fall and
            // stays there until we touch ground, so the Jump clip can't restart in air.
            state = jumpAscending ? StateJump : StateFall;
        }
        else if (moveAxis.magnitude > moveDeadzone)
        {
            state = runHeld ? StateRun : StateWalk;
        }
        else
        {
            state = StateIdle;
        }

        animator.SetInteger(stateParameter, state);
    }
}
