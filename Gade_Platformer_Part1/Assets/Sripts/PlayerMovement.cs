using UnityEngine;




public class PlayerMovement : MonoBehaviour
{
    // Speed of the player
    public float speed = 5f;

    // Jump strength
    public float jumpForce = 5f;

    // Gravity pulling the player down
    public float gravity = -9.8f;

    // Reference to the CharacterController
    private CharacterController controller;

    // Player vertical velocity (used for jumping and gravity)
    private Vector3 velocity;

    // Check if the player is touching the ground
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    private bool isGrounded;

    // Reference to the Animator
    public Animator animator;

    void Start()
    {
        // Get the CharacterController component
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        // Reset downward velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get keyboard input (WASD)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Movement direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Move the player
        controller.Move(move * speed * Time.deltaTime);

        // Play running animation if moving
        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude);
        }

        // Jump when space is pressed
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpForce;

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move player vertically
        controller.Move(velocity * Time.deltaTime);
    }
}
// Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
  