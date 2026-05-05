using UnityEngine;

public class AiChase : MonoBehaviour
{
    //Declare variables
    Patrol patrolScript;

    private void Start()
    {
        patrolScript = GetComponent<Patrol>();
    }

    //Check if the player touches the collider chase the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Chase the player
            if (patrolScript != null)
            {
                patrolScript.playerInChase = true;
                Debug.Log("Player entered chase range");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (patrolScript != null)
            {
                patrolScript.playerInChase = false;
                Debug.Log("Player exited chase range");
            }
        }
    }
}
