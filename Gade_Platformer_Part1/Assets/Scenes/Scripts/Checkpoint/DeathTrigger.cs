using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();

        if (player != null)
        {
            // --- SFX: hit sound (Part 3 D3) ---
            // Plays when the player falls into a death zone trigger.
            SFXManager.Instance?.PlaySound("hit");
            player.LoseLife();
            player.Death();
        }
    }
}
