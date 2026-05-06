using UnityEngine;

public class attacking : MonoBehaviour
{
    //Declare variables
    AiControllerA patrolScript;

    private void Start()
    {
        patrolScript = GetComponent<AiControllerA>();
    }

    //Check if the player touches the collider attack the player
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (patrolScript != null)
            {
                patrolScript.BounceOffPLY(collision);
            }
        }
    }

}
