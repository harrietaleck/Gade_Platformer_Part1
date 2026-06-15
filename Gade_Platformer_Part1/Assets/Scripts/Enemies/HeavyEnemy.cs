using UnityEngine;

public class HeavyEnemy : Enemy
{
    protected override void ApplyVariationStats()
    {
        MoveSpeed = 0;
        AgentSize = 1.5f;
        AppearanceColor = Color.blue;
        IsPatrolling = false;
    }
}
