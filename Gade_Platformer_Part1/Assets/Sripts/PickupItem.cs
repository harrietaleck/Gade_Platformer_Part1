using UnityEngine;

public class PickupItem : MonoBehaviour
{   
    public string itemType; // Type of the item (e.g., "FireStone", "Food", etc.)

    void OnTriggerEnter(Collider other)
    {
        // Check if the player touched the object
        if (other.CompareTag("Player"))
        {
            // Add the item to the player's inventory
            GameManager.instance.AddResource(itemType);
            Debug.Log("Item picked up!");

            // Remove the object from the scene
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
