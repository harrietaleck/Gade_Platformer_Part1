using UnityEngine;

public class PLATtrigger : MonoBehaviour
{
    public MOVEplatform platform;
    //Activate the platform when the player enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.Activate();
        }
    }
}
