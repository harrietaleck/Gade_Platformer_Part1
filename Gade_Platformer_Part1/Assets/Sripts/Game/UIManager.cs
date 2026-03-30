using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
    public TMP_Text livesText;
    public TMP_Text scoreText;
    public TMP_Text thermalStonesText;
    public TMP_Text foodSuppliesText;
    public TMP_Text winterClothingText;

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
        if (foodSuppliesText != null) foodSuppliesText.text = "Food Supplies: " + GameManager.Instance.foodSupplies;
        if (winterClothingText != null) winterClothingText.text = "Winter Clothing: " + GameManager.Instance.winterClothing;
    }
}