using UnityEngine;

public class HeavyEnemy : Enemy
{
    protected override void ApplyVariationStats()
    {
        MoveSpeed = 0;
        AgentSize = 1.2f;
        VisualScale = 0.42f;
        AppearanceColor = Color.blue;
        IsPatrolling = false;
    }
}
