using UnityEngine;
using UnityEngine.AI;

public class AiAttack : MonoBehaviour
{
    //Declare variables
    Patrol patrolScript;

    private void Start()
    {
        patrolScript = GetComponent<Patrol>();
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
