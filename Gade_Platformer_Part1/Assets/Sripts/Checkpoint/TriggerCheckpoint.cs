using UnityEngine;

public class TriggerCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();

        if (player != null)
        {
            player.CheckpointSave();
        }
    }
}
