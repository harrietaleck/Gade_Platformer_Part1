using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// UIManager — gameplay HUD
// Shows Health, Score (via Panel2), collectables (Panel1),
// and a Freeze timer (counts up; at 60s GameManager ends the run).
// ============================================================
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
    public TMP_Text thermalStonesText;
    public TMP_Text foodSuppliesText;
    public TMP_Text winterClothingText;
    public TMP_Text healthText;
    public TMP_Text scoreText;
    public TMP_Text freezeTimerText;
    public Image freezeFillImage;

    GameObject _freezePanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AutoWireHud();
        EnsureFreezePanel();
        RefreshHUD();
    }

    void AutoWireHud()
    {
        if (thermalStonesText == null)
            thermalStonesText = FindHudText("ThermalStonesText");
        if (foodSuppliesText == null)
            foodSuppliesText = FindHudText("FoodSuppliesText");
        if (winterClothingText == null)
            winterClothingText = FindHudText("WinterClothingText");
        if (healthText == null)
            healthText = FindHudText("LivesText");
        if (scoreText == null)
            scoreText = FindHudText("Score");
    }

    static TMP_Text FindHudText(string nameContains)
    {
        var texts = Object.FindObjectsOfType<TMP_Text>(true);
        foreach (var t in texts)
        {
            if (t == null) continue;
            // Only bind HUD texts under Canvas panels, not dialogue / menus.
            string path = t.transform.name;
            if (path.IndexOf(nameContains, System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            Transform p = t.transform.parent;
            if (p != null && (p.name == "Panel1" || p.name == "Panel2" ||
                              p.name.IndexOf("HUD", System.StringComparison.OrdinalIgnoreCase) >= 0))
                return t;
            if (path.IndexOf(nameContains, System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                t.GetComponentInParent<Canvas>() != null)
                return t;
        }
        return null;
    }

    void EnsureFreezePanel()
    {
        if (freezeTimerText != null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        _freezePanel = new GameObject("FreezePanel", typeof(RectTransform));
        _freezePanel.transform.SetParent(canvas.transform, false);

        var panelR = _freezePanel.GetComponent<RectTransform>();
        panelR.anchorMin = new Vector2(0.5f, 1f);
        panelR.anchorMax = new Vector2(0.5f, 1f);
        panelR.pivot = new Vector2(0.5f, 1f);
        panelR.anchoredPosition = new Vector2(0f, -12f);
        panelR.sizeDelta = new Vector2(280f, 64f);

        var bg = _freezePanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.16f, 0.32f, 0.78f);

        // Fill bar background
        var barBgGo = new GameObject("FreezeBarBg", typeof(RectTransform));
        barBgGo.transform.SetParent(_freezePanel.transform, false);
        var barBgR = barBgGo.GetComponent<RectTransform>();
        barBgR.anchorMin = new Vector2(0.06f, 0.12f);
        barBgR.anchorMax = new Vector2(0.94f, 0.42f);
        barBgR.offsetMin = barBgR.offsetMax = Vector2.zero;
        var barBg = barBgGo.AddComponent<Image>();
        barBg.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

        var fillGo = new GameObject("FreezeFill", typeof(RectTransform));
        fillGo.transform.SetParent(barBgGo.transform, false);
        var fillR = fillGo.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = Vector2.one;
        fillR.offsetMin = fillR.offsetMax = Vector2.zero;
        freezeFillImage = fillGo.AddComponent<Image>();
        freezeFillImage.color = new Color(0.45f, 0.78f, 1f, 0.95f);
        freezeFillImage.type = Image.Type.Filled;
        freezeFillImage.fillMethod = Image.FillMethod.Horizontal;
        freezeFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        freezeFillImage.fillAmount = 0f;

        // Timer label
        var labelGo = new GameObject("FreezeTimerText", typeof(RectTransform));
        labelGo.transform.SetParent(_freezePanel.transform, false);
        var labelR = labelGo.GetComponent<RectTransform>();
        labelR.anchorMin = new Vector2(0.06f, 0.45f);
        labelR.anchorMax = new Vector2(0.94f, 0.95f);
        labelR.offsetMin = labelR.offsetMax = Vector2.zero;

        freezeTimerText = labelGo.AddComponent<TextMeshProUGUI>();
        freezeTimerText.fontSize = 20;
        freezeTimerText.fontStyle = FontStyles.Bold;
        freezeTimerText.alignment = TextAlignmentOptions.Center;
        freezeTimerText.color = Color.white;
        freezeTimerText.text = "Freeze  0:00 / 1:00";
    }

    public void RefreshHUD()
    {
        var gm = GameManager.Instance;

        if (gm != null)
        {
            if (thermalStonesText != null)
                thermalStonesText.text = "Thermal Stones: " + gm.thermalStones;
            if (foodSuppliesText != null)
                foodSuppliesText.text = "Food Supplies: " + gm.foodSupplies;
            if (winterClothingText != null)
                winterClothingText.text = "Winter Clothing: " + gm.winterClothing;
        }

        int health = 3;
        int score = 0;
        var playerData = Object.FindObjectOfType<PlayerCheckpointDatat>();
        if (playerData != null)
        {
            health = playerData.lives;
            score = playerData.score;
            if (gm != null) score = Mathf.Max(score, gm.score);
        }
        else if (gm != null)
        {
            health = gm.lives;
            score = gm.score;
        }

        if (healthText != null)
            healthText.text = "Health: " + Mathf.Max(0, health);
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        // Also keep PlayerCheckpointDatat legacy fields in sync if present
        if (playerData != null)
        {
            if (playerData.livesText != null && playerData.livesText != healthText)
                playerData.livesText.text = "Health: " + Mathf.Max(0, playerData.lives);
            if (playerData.scoreText != null && playerData.scoreText != scoreText)
                playerData.scoreText.text = "Score: " + (gm != null ? gm.score : playerData.score);
        }

        RefreshFreezeUI();
    }

    public void RefreshFreezeUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        float t = Mathf.Clamp(gm.freezeTime, 0f, gm.freezeLimitSeconds);
        float limit = Mathf.Max(1f, gm.freezeLimitSeconds);
        float pct = t / limit;

        int secs = Mathf.FloorToInt(t);
        int limSecs = Mathf.FloorToInt(limit);
        string cur = $"{secs / 60}:{secs % 60:00}";
        string max = $"{limSecs / 60}:{limSecs % 60:00}";

        if (freezeTimerText != null)
            freezeTimerText.text = $"Freeze  {cur} / {max}";

        if (freezeFillImage != null)
        {
            freezeFillImage.fillAmount = pct;
            // Shift from icy blue → danger red as freeze rises
            freezeFillImage.color = Color.Lerp(
                new Color(0.45f, 0.78f, 1f, 0.95f),
                new Color(0.95f, 0.25f, 0.25f, 0.95f),
                pct
            );
        }
    }
}
