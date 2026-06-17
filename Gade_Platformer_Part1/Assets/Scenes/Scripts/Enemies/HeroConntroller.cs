using UnityEngine;

public class HeroController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool IsAlive { get; private set; } = true;

    public void Die()
    {
        if (!IsAlive)
            return;

        IsAlive = false;
        Debug.Log("Hero killed — game over.");
        gameObject.SetActive(false);
    }
}
