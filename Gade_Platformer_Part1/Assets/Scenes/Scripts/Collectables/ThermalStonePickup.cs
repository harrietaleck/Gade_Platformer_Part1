using UnityEngine;

// Collectable thermal stone,requires trigger collider. Awards score + thermal count.
public class ThermalStonePickup : MonoBehaviour
{
    [Header("Rewards")]
    public int scoreValue = 10;
    public int thermalStoneValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.AddThermalStone(thermalStoneValue);
        }

        Destroy(gameObject);
    }
}
//On trigger with something that has Player, calls GameManager.AddScore + AddThermalStone, then destroys the pickup.