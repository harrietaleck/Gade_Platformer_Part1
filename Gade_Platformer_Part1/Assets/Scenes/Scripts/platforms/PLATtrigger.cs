using UnityEngine;

public class PLATtrigger : MonoBehaviour
{
    public MOVEplatform platform;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (platform == null)
            platform = FindObjectOfType<MOVEplatform>();

        platform?.Activate();
    }
}
