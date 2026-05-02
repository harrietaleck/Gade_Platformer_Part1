using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
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
        //Check if both the game manager and checkpoint data are available to update the HUD
        if (GameManager.Instance == null) return;

        //Update the HUD Texts
        if (thermalStonesText != null) thermalStonesText.text = "Thermal Stones: " + GameManager.Instance.thermalStones;
        if (foodSuppliesText != null) foodSuppliesText.text = "Food Supplies: " + GameManager.Instance.foodSupplies;
        if (winterClothingText != null) winterClothingText.text = "Winter Clothing: " + GameManager.Instance.winterClothing;
    }
}