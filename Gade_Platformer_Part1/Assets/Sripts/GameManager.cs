using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Player resources
    public int thermalStones = 0;
    public int food = 0;
    public int water = 0;
    public int clothes = 0;
    public int backpack = 0;

    void Awake()
    {
        // Makes sure only one GameManager exists
        if (instance == null)
        {
            instance = this;
        }
    }

    // Add resources
    public void AddResource(string itemType)
    {
        if (itemType == "Stone")
        {
            thermalStones++;
        }

        if (itemType == "Food")
        {
            food++;
        }

        if (itemType == "Water")
        {
            water++;
        }

        if (itemType == "Clothes")
        {
            clothes++;
        }

        if (itemType == "Backpack")
        {
            backpack++;
        }

        Debug.Log("Collected: " + itemType);
    }
}

// Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
  