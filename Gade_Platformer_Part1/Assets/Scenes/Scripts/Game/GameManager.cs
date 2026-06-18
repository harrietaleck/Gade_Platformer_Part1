using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ── Survival stats (deplete over time) ───────────────────────
    [Header("Survival Stats")]
    public int lives        = 3;
    public int score        = 0;
    public int thermalStones  = 10;
    public int temperature    = 30;
    public int foodSupplies   = 10;
    public int winterClothing = 0;
    public float timerCount   = 0f;

    // ── Collectibles actually picked up (scorecard / progression) ─
    [Header("Pickup Counters (read-only in play)")]
    public int thermalStonesCollected  = 0;
    public int foodSuppliesCollected   = 0;
    public int winterClothingCollected = 0;

    // Total pickups this scene — resets when a new scene loads
    [HideInInspector] public int pickupsThisScene = 0;

    // ── Level goals ───────────────────────────────────────────────
    [Header("Level Progression Goals")]
    public int beginnerGoal  = 7;   // pickups needed in Beginner  → load Advanced
    public int advancedGoal  = 3;   // pickups needed in Advanced  → load Expert

    private bool _progressionLocked = false; // prevents double-load

    PlayerCheckpointDatat playerCheckpointDatat;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Reset per-scene pickup counter every time a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pickupsThisScene   = 0;
        _progressionLocked = false;
    }

    // ── Score / Collectible Add methods ──────────────────────────
    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void AddThermalStone(int amount)
    {
        thermalStones          += amount;
        thermalStonesCollected += amount;
        pickupsThisScene       += amount;
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void AddFoodSupply(int amount)
    {
        foodSupplies           += amount;
        foodSuppliesCollected  += amount;
        pickupsThisScene       += amount;
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void AddWinterClothing(int amount)
    {
        winterClothing             += amount;
        winterClothingCollected    += amount;
        pickupsThisScene           += amount;
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void GainLife(int amount = 1)
    {
        lives += amount;
        UIManager.Instance?.RefreshHUD();
    }

    // ── Level progression ────────────────────────────────────────
    // Called every time a pickup is collected.
    // Beginner → 7 pickups → Advanced
    // Advanced → 3 pickups → Expert
    private void CheckLevelProgression()
    {
        if (_progressionLocked) return;

        string scene = SceneManager.GetActiveScene().name;

        bool shouldProgress =
            (scene == "Beginner" && pickupsThisScene >= beginnerGoal) ||
            (scene == "Advanced" && pickupsThisScene >= advancedGoal);

        if (!shouldProgress) return;

        _progressionLocked = true;   // block further calls while loading

        string nextScene = scene == "Beginner" ? "Advanced" : "Expert";
        Debug.Log($"[GameManager] {scene}: {pickupsThisScene} pickups reached — loading {nextScene}");
        SceneManager.LoadScene(nextScene);
    }

    // ── Survival timer ───────────────────────────────────────────
    void Update()
    {
        if (playerCheckpointDatat == null) return;

        timerCount += Time.deltaTime;
        int randomSubtract = Random.Range(1, 3);

        if (timerCount >= 5.0f)
        {
            thermalStones -= randomSubtract;
            foodSupplies  -= randomSubtract;

            if (thermalStones <= 0 && foodSupplies <= 0)
            {
                playerCheckpointDatat.Death();
                thermalStones = 10;
                foodSupplies  = 10;
            }

            UIManager.Instance?.RefreshHUD();
            timerCount = 0;
        }
    }

    // ── Total collected helper (used by scoreboard) ───────────────
    public int TotalCollected =>
        thermalStonesCollected + foodSuppliesCollected + winterClothingCollected;
}
