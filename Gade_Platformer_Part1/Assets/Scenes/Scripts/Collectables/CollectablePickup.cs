using UnityEngine;

public enum CollectableType
{
    ThermalStone,
    FoodSupply,
    WinterClothing
}

// Reusable pickup script for different collectable types.
public class CollectablePickup : MonoBehaviour
{
    [Header("Type")]
    public CollectableType collectableType = CollectableType.ThermalStone;

    [Header("Rewards")]
    public int scoreValue = 10;
    public int itemAmount = 1;
    public int livesToRestore = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);

            switch (collectableType)
            {
                case CollectableType.ThermalStone:
                    GameManager.Instance.AddThermalStone(itemAmount);
                    break;

                case CollectableType.FoodSupply:
                    GameManager.Instance.AddFoodSupply(itemAmount);
                    if (livesToRestore > 0)
                        GameManager.Instance.GainLife(livesToRestore);
                    break;

                case CollectableType.WinterClothing:
                    GameManager.Instance.AddWinterClothing(itemAmount);
                    break;
            }
        }

        // --- SFX: collect sound (Part 3 D3) ---
        // PlayOneShot lives on the GameManager AudioSource, so the clip
        // continues to play even after this GameObject is destroyed.
        SFXManager.Instance?.PlaySound("collect");

        Destroy(gameObject);
    }
}
