using UnityEngine;

// Collectable thermal stone, requires trigger collider. Awards score + thermal count.
public class ThermalStonePickup : MonoBehaviour
{
    [Header("Rewards")]
    public int scoreValue = 10;
    public int thermalStoneValue = 1;

    [Header("Sound")]
    public AudioClip collectSFX;   // Assign: Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.AddThermalStone(thermalStoneValue);
        }

        // Play clip at world position — works even after this object is destroyed
        if (collectSFX != null)
            AudioSource.PlayClipAtPoint(collectSFX, transform.position);

        Destroy(gameObject);
    }
}
//On trigger with something that has Player, calls GameManager.AddScore + AddThermalStone, then destroys the pickup.