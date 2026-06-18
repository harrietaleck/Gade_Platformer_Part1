using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("Sound")]
    public AudioClip collectSFX;   // Auto-assigned in Editor from Casual Game Sounds U6/collectSound.wav

#if UNITY_EDITOR
    // Called when the component is first added or Reset is chosen in the Inspector.
    // Automatically fills collectSFX so no manual wiring is needed.
    private void Reset()
    {
        if (collectSFX == null)
            collectSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav");
    }
#endif

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

        // Play clip at world position — works even after this object is destroyed
        if (collectSFX != null)
            AudioSource.PlayClipAtPoint(collectSFX, transform.position);

        Destroy(gameObject);
    }
}
