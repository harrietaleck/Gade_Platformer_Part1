using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    Player player;
    private void OnTriggerEnter(Collider other)
    {
        /* PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();

         if (player != null)
         {
             player.PlayerDied();
         }*/
        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();
        GameManager gameManager = GameManager.Instance;
        if (player != null)
        {
            player.LoseLife();
            player.Death();
        }
    }
}
