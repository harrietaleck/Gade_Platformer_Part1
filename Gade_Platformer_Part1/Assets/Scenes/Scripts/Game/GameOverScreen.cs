using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("Panel")]
    public GameObject gameOverPanel;

    [Header("Text Fields")]
    public TMP_Text headlineText;       // "GAME OVER"
    public TMP_Text scoreboardText;     // full collectibles breakdown
    public TMP_Text levelNameText;      // which scene the player died in

    // Legacy fields — kept so existing scene wiring doesn't break
    [Header("Legacy (optional)")]
    public TMP_Text finalScoreText;
    public TMP_Text finalLivesText;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // ── Show the Game Over screen ─────────────────────────────────
    // Called by PlayerCheckpointDatat when lives reach zero.
    public void ShowGameOver(int finalScore, int livesLeft)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        // Level name
        string sceneName = SceneManager.GetActiveScene().name;
        if (levelNameText != null)
            levelNameText.text = "Level: " + sceneName;

        // Headline
        if (headlineText != null)
            headlineText.text = "GAME OVER";

        // Build scoreboard from GameManager counters
        var gm = GameManager.Instance;
        int stones   = gm != null ? gm.thermalStonesCollected  : 0;
        int food     = gm != null ? gm.foodSuppliesCollected    : 0;
        int clothing = gm != null ? gm.winterClothingCollected  : 0;
        int total    = gm != null ? gm.TotalCollected           : 0;
        int sc       = gm != null ? gm.score                    : finalScore;

        string board =
            $"Score:  {sc}\n" +
            $"─────────────────\n" +
            $"Thermal Stones:   {stones}\n" +
            $"Food Supplies:    {food}\n" +
            $"Winter Clothing:  {clothing}\n" +
            $"─────────────────\n" +
            $"Total Pickups:    {total}";

        // Prefer the new scoreboardText; fall back to finalScoreText
        if (scoreboardText != null)
            scoreboardText.text = board;
        else if (finalScoreText != null)
            finalScoreText.text = board;

        // Legacy lives text
        if (finalLivesText != null)
            finalLivesText.text = "Lives Left: " + livesLeft;
    }

    // ── Buttons ───────────────────────────────────────────────────

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        // Reset per-scene pickup counter so the goal works again
        if (GameManager.Instance != null)
        {
            GameManager.Instance.pickupsThisScene = 0;
            // Reset collected counters for a fresh retry
            GameManager.Instance.thermalStonesCollected  = 0;
            GameManager.Instance.foodSuppliesCollected   = 0;
            GameManager.Instance.winterClothingCollected = 0;
            GameManager.Instance.score = 0;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
