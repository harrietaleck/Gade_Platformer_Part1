using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
    public TMP_Text livesText;
    public TMP_Text scoreText;
    public TMP_Text thermalStonesText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (GameManager.Instance == null) return;

        if (livesText != null) livesText.text = "Lives: " + GameManager.Instance.lives;
        if (scoreText != null) scoreText.text = "Score: " + GameManager.Instance.score;
        if (thermalStonesText != null) thermalStonesText.text = "Thermal Stones: " + GameManager.Instance.thermalStones;
    }
}