using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Basic 3D character for platform level, player Drop on a CharacterController + tag Player.

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -30f;

    private CharacterController controller;
    private float verticalVelocity;
    private float horizontalInput;
    private float verticalInput;
    private bool jumpPressedThisFrame;

    // Values the animation state system reads
    public bool IsGrounded => controller != null && controller.isGrounded;
    public float VerticalVelocity => verticalVelocity;
    public float MoveMagnitude => new Vector2(horizontalInput, verticalInput).magnitude;
    public bool JumpPressedThisFrame => jumpPressedThisFrame;

    private void Awake()
    {
        // Cache controller once
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float h;
        float z;

#if ENABLE_INPUT_SYSTEM
        // Read WASD from the new Input System
        Vector2 moveAxis = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) moveAxis.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveAxis.x += 1f;
            if (Keyboard.current.sKey.isPressed) moveAxis.y -= 1f;
            if (Keyboard.current.wKey.isPressed) moveAxis.y += 1f;
        }
        h = moveAxis.x;
        z = moveAxis.y;
#else
        // Read movement from old Input Manager
        h = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
#endif

        Vector3 input = new Vector3(h, 0f, z);

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        // Turn player using mouse X
        float mouseX = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            mouseX = Mouse.current.delta.ReadValue().x;
#else
        mouseX = Input.GetAxis("Mouse X");
#endif
        transform.Rotate(Vector3.up * mouseX);

        // Move in local forward/right direction
        Vector3 move = (transform.right * h + transform.forward * z);
        if (move.sqrMagnitude > 1f)
            move.Normalize();
        move *= moveSpeed;

        // Keep a tiny downward force while grounded
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

#if ENABLE_INPUT_SYSTEM
        // Space key jump (new Input System)
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        // Jump button (old Input Manager)
        bool jumpPressed = Input.GetButtonDown("Jump");
#endif
        jumpPressedThisFrame = jumpPressed;
        horizontalInput = h;
        verticalInput = z;

        // Start jump
        if (jumpPressed && controller.isGrounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Apply gravity and move
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

}
//CharacterController movement: WASD to move on the ground, Space to jump (Unity Jump input)