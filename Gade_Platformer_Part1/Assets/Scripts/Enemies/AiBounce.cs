using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AiBounce : MonoBehaviour
{
    //Declare variables
    AiController aiController;

    private void Start()
    {
        aiController = GetComponent<AiController>();
    }

    //Check if the player touches the collider attack the player
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (aiController != null)
            {
                aiController.BounceOffPLY(collision);
            }
        }
    }
}