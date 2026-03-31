using UnityEngine;

public class DeathObstacle : MonoBehaviour
{
    GameManager gameManager;
    PlayerCheckpointDatat playerCheckpointDatat;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
           
        }
    }
}
