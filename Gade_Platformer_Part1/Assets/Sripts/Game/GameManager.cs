using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int lives = 3;
    public int score = 0;
    public int thermalStones = 5;
    public int temperature = 30;
    public int foodSupplies = 0;
    public int winterClothing = 0;
    public float timerCount = 0f;
    PlayerCheckpointDatat playerCheckpointDatat;

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
    public void DecreaseThermalStone(int amount)
    {
        if (playerCheckpointDatat == null) return;
        thermalStones -= amount;
        if (thermalStones == 0)
        {
            playerCheckpointDatat.Death();
            thermalStones = 5;
        }
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
    void Update()
    {
        //Increase the timer count every second
        timerCount += Time.deltaTime;

        if (timerCount >= 20.0f)
        {
            DecreaseThermalStone(1);
            timerCount = 0;
        }
            
    }
    
}