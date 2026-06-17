using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("Panel")]
    public GameObject gameOverPanel;

    [Header("Stats Display")]
    public TMP_Text finalScoreText;
    public TMP_Text finalLivesText;
    public TMP_Text levelNameText;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Called by PlayerCheckpointDatat when lives reach zero
    public void ShowGameOver(int finalScore, int finalLives)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore;

        if (finalLivesText != null)
            finalLivesText.text = "Lives Left: " + finalLives;

        if (levelNameText != null)
            levelNameText.text = "Level: " + SceneManager.GetActiveScene().name;
    }

    // ── Buttons ────────────────────────────────────────────────────

    public void RetryLevel()
    {
        Time.timeScale = 1f;
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
