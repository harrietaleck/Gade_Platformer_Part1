using UnityEngine;

public class NormEnemy : Enemy
{
    protected override void ApplyVariationStats()
    {
        MoveSpeed = 2;
        AgentSize = 1f;
        VisualScale = 0.35f;
        AppearanceColor = Color.yellow;
        IsPatrolling = true;
    }
}
