// ============================================================
// AiAttack.cs  —  Proximity-based damage to the player
//
// Uses a distance check in Update() each frame to detect when
// the player is close enough to take damage. This approach is
// more reliable than OnCollisionEnter/OnTriggerEnter because
// the player uses a CharacterController (no Rigidbody), which
// stops at solid colliders without generating collision events.
// ============================================================

using UnityEngine;

public class AiAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    // Distance (world units) within which the enemy damages the player.
    // Tune this in the Inspector to match the visual size of the enemy.
    [SerializeField] private float hitRadius = 1.5f;

    // Seconds between consecutive hits — prevents instant repeated damage
    // and gives the player a brief window to escape after being hit.
    [SerializeField] private float attackCooldown = 1.5f;

    // Cached reference to the player transform, found by tag in Start().
    private Transform player;

    // Cached reference to PlayerCheckpointDatat — manages lives, score,
    // and respawn. Lives are stored here, not in GameManager, so this is
    // the component we call for both LoseLife() and Death().
    private PlayerCheckpointDatat checkpointData;

    // Cached reference to Player — used to trigger the hurt animation
    // (brief knockback visual + screen flash) when the player is hit.
    private Player playerScript;

    // Timestamp of the last successful hit, used to enforce the cooldown.
    private float lastAttackTime = -999f;

    private void Start()
    {
        // Find the player at startup by tag to avoid manual Inspector wiring.
        // The "Player" tag must be set on the player GameObject in the Inspector.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;

            // PlayerCheckpointDatat holds lives, score, and the respawn stack.
            // It lives on the Player GameObject itself.
            checkpointData = playerObj.GetComponent<PlayerCheckpointDatat>();
            if (checkpointData == null)
                Debug.LogWarning($"AiAttack on '{name}': PlayerCheckpointDatat not found on Player.");

            // Player script — used to trigger the hurt animation on hit.
            playerScript = playerObj.GetComponent<Player>();
            if (playerScript == null)
                Debug.LogWarning($"AiAttack on '{name}': Player component not found — hurt animation won't play.");
        }
        else
        {
            Debug.LogWarning($"AiAttack on '{name}': No GameObject tagged 'Player' found.");
        }
    }

    private void Update()
    {
        // Cannot damage if we never found the player in Start().
        if (player == null) return;

        // Calculate straight-line distance from this enemy to the player.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Damage the player if they are within range AND the cooldown has elapsed.
        if (distanceToPlayer <= hitRadius && Time.time - lastAttackTime >= attackCooldown)
        {
            // Record this hit time to start the cooldown.
            lastAttackTime = Time.time;

            if (checkpointData != null)
            {
                // Deduct one life and update the HUD.
                // LoseLife() also handles game-over (lives == 0 → load StartScreen).
                checkpointData.LoseLife();

                // Teleport the player back to the last saved checkpoint position.
                // Death() uses the custom Stack ADT (Peek) to retrieve the position
                // without removing it, so the same checkpoint works for multiple deaths.
                checkpointData.Death();
            }

            // Trigger knockback animation + screen flash on the player.
            playerScript?.TriggerHurt();

            // Play the hit sound via the HashMap SFX Manager (Part 3 D3).
            SFXManager.Instance?.PlaySound("hit");
        }
    }
}
