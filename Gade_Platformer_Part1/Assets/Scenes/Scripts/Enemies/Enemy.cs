using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    //Declare variables
    public float MoveSpeed { get; protected set; }
    public float AgentSize { get; protected set; }
    public float VisualScale { get; protected set; } = 0.35f;
    public Color AppearanceColor { get; protected set; }
    public bool IsPatrolling { get; protected set; }

    void Awake()
    {
        // Scene-placed enemies can carry a non-uniform scale from old platform parenting.
        if (transform.Find("WolfVisual") != null)
            transform.localScale = Vector3.one;
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        ApplyVariationStats();

        // Keep root at uniform scale — scaling the root stretches the wolf mesh.
        transform.localScale = Vector3.one;

        var wolfVisual = transform.Find("WolfVisual");
        if (wolfVisual != null)
            wolfVisual.localScale = Vector3.one * VisualScale;

        var col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.height = 2f * AgentSize;
            col.radius = 0.5f * AgentSize;
        }

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.height = 2f * AgentSize;
            agent.radius = 0.5f * AgentSize;
        }

        WolfVisualSetup.ApplyToEnemy(gameObject);

        var renderer = GetComponent<Renderer>();
        if (renderer != null && renderer is MeshRenderer)
            renderer.material.color = AppearanceColor;
    }

    protected abstract void ApplyVariationStats();
}
