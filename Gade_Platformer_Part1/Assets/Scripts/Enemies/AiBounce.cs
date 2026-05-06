using Unity.VisualScripting;
using UnityEngine;

public class AiBounce : MonoBehaviour
{
    //Declare variables
    AiControllerA aiController;
    public Transform player;
    PlayerCheckpointDatat playerData;
    private int attackDamageThreshold;

    private void Start()
    {
        aiController = GetComponent<AiControllerA>();
        playerData = player.GetComponent<PlayerCheckpointDatat>();

        //Set the threshold damage for the player
        attackDamageThreshold = Random.Range(3, 6);
    }

    //Check if the player touches the collider attack the player
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (aiController != null)
            {
                aiController.BounceOffPLY(collision);
                if (aiController.hitCounter >= attackDamageThreshold)
                {
                    Debug.Log("Player hit damage max!");
                    //Call the lose life function to decrease the player's health
                    if (playerData != null)
                        playerData.lives -= 1;
                    //Reset the hit counter
                    aiController.hitCounter = 0;
                    //Reset the attack damage threshold
                    attackDamageThreshold = Random.Range(3, 6);
                }
            }
        }
    }
}