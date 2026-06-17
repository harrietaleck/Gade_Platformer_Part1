using UnityEngine;

public class AiChase : MonoBehaviour
{
    //Declare variables
    AiControllerA patrolScript;

    private void Start()
    {
        patrolScript = GetComponent<AiControllerA>();
    }

    //Check if the player touches the collider chase the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Chase the player
            if (patrolScript != null)
            {
                //code fix : remove slashes when ai controllera is fixed
                //patrolScript.playerInChase = true;
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
                //code fix : remove slashes when ai controllera is fixed
                //patrolScript.playerInChase = false;
                Debug.Log("Player exited chase range");
            }
        }
    }
}

