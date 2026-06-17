using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    //Declare variables
    public float MoveSpeed { get; protected set; }
    public float AgentSize { get; protected set; }
    public Color AppearanceColor { get; protected set; }
    public bool IsPatrolling { get; protected set; }
    //Create a method to initialize the enemy
    public void Initialize()
    {
        //Set the enemy's stats based on its type
        ApplyVariationStats();

        transform.localScale = Vector3.one * AgentSize;

        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = AppearanceColor;
        }
    }

    protected abstract void ApplyVariationStats();
}
