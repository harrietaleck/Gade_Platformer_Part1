using UnityEngine;

public class TriggerCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();

        if (player != null)
        {
            // --- SFX: checkpoint reached sound (Part 3 D3) ---
            // Plays once each time the player touches a checkpoint trigger.
            SFXManager.Instance?.PlaySound("checkpoint");

            player.CheckpointSave();
        }
    }
}
