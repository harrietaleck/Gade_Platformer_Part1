using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int lives = 3;
    public int score = 0;
    public int thermalStones = 10;
    public int temperature = 30;
    public int foodSupplies = 10;
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
        if (playerCheckpointDatat == null) return;
        //Increase the timer count every second
        timerCount += Time.deltaTime;
        int randomSubtract = Random.Range(1, 3); //Randomly decrease using te value assigned to this

        if (timerCount >= 5.0f)
        {
            thermalStones -= randomSubtract;
            foodSupplies -= randomSubtract;
            if (thermalStones == 0 && foodSupplies == 0)
            {
                playerCheckpointDatat.Death();
                thermalStones = 10;
                foodSupplies = 10;
            }
            UIManager.Instance?.RefreshHUD();
            timerCount = 0;

        }
            
    }
    
}