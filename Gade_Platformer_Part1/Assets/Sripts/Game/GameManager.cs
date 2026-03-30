using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int lives = 3;
    public int score = 0;
    public int thermalStones = 0;
    public int foodSupplies = 0;
    public int winterClothing = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void AddThermalStone(int amount)
    {
        thermalStones += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void AddFoodSupply(int amount)
    {
        foodSupplies += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void AddWinterClothing(int amount)
    {
        winterClothing += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void GainLife(int amount = 1)
    {
        lives += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void LoseLife(int amount = 1)
    {
        lives -= amount;
        if (lives < 0) lives = 0;
        UIManager.Instance?.RefreshHUD();
    }
}