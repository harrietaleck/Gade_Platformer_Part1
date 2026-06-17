using UnityEngine;

public class FastEnemy : Enemy
{
    protected override void ApplyVariationStats()
    {
        MoveSpeed = 8;
        AgentSize = 0.7f;
        AppearanceColor = Color.red;
        IsPatrolling = true;
    }
}
