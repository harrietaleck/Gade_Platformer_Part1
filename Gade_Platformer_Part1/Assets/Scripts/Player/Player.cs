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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float h;
        float z;

#if ENABLE_INPUT_SYSTEM
        // New Input System (ProjectSettings: activeInputHandler = 2)
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
        // Old Input Manager
        h = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
#endif

        Vector3 input = new Vector3(h, 0f, z);

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        // Rotate with mouse (or ignore if not using mouse)
        float mouseX = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            mouseX = Mouse.current.delta.ReadValue().x;
#else
        mouseX = Input.GetAxis("Mouse X");
#endif
        transform.Rotate(Vector3.up * mouseX);

        // Move relative to where the player is facing
        Vector3 move = (transform.right * h + transform.forward * z);
        if (move.sqrMagnitude > 1f)
            move.Normalize();
        move *= moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

#if ENABLE_INPUT_SYSTEM
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        bool jumpPressed = Input.GetButtonDown("Jump");
#endif

        if (jumpPressed && controller.isGrounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

}
//CharacterController movement: WASD to move on the ground, Space to jump (Unity Jump input)