using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();

        if (player != null)
        {
            player.lives--;
            player.Death();
        }
    }
}
