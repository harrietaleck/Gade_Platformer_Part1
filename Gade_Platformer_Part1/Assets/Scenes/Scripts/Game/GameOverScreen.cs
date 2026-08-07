using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// GameOverScreen
// ---------------
// Builds the Game Over overlay at RUNTIME using the provided artwork
// (Assets/UI/Textures/GameOverBackground.png) so it looks like part of the
// game. The "GAME OVER" title and panels are baked into that image; this
// script only lays the level name, the score breakdown, and the buttons on
// top. Runs consistently in every level (Beginner / Advanced / Expert).
public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("Panel (legacy — hidden at runtime)")]
    public GameObject gameOverPanel;

    [Header("Background art")]
    [Tooltip("GameOverBackground.png. Auto-loaded in the Editor; assign for builds.")]
    public Sprite backgroundSprite;

    [Tooltip("Shared UI button sprite used across the game (SF Button). " +
             "Auto-loaded in the Editor; assign for builds.")]
    public Sprite buttonSprite;

    [Header("Text Fields (legacy — optional)")]
    public TMP_Text headlineText;
    public TMP_Text scoreboardText;
    public TMP_Text levelNameText;
    public TMP_Text finalScoreText;
    public TMP_Text finalLivesText;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    // ── Palette (dark text reads well on the light art panel) ─────
    static readonly Color InkTitle = new Color(0.06f, 0.11f, 0.32f, 1f);   // navy
    static readonly Color InkBody  = new Color(0.10f, 0.16f, 0.36f, 1f);   // soft navy
    static readonly Color InkValue = new Color(0.12f, 0.30f, 0.62f, 1f);   // blue value

    // Runtime refs
    GameObject _overlay;
    TMP_Text   _subtitle;
    TMP_Text   _labels;
    TMP_Text   _values;

    private void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // ── Show the Game Over screen ─────────────────────────────────
    public void ShowGameOver(int finalScore, int livesLeft)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        BuildOverlay();
        _overlay.SetActive(true);
        _overlay.transform.SetAsLastSibling();

        string sceneName = SceneManager.GetActiveScene().name;
        if (_subtitle != null)
            _subtitle.text = "Level:  <b>" + sceneName + "</b>";

        var gm = GameManager.Instance;
        int stones   = gm != null ? gm.thermalStonesCollected  : 0;
        int food     = gm != null ? gm.foodSuppliesCollected    : 0;
        int clothing = gm != null ? gm.winterClothingCollected  : 0;
        int total    = gm != null ? gm.TotalCollected           : 0;
        int sc       = gm != null ? gm.score                    : finalScore;

        if (_labels != null)
            _labels.text = "Score\nThermal Stones\nFood Supplies\nWinter Clothing\n<b>Total Pickups</b>";
        if (_values != null)
            _values.text = $"{sc}\n{stones}\n{food}\n{clothing}\n<b>{total}</b>";
    }

    // ── Overlay builder (runs once, then reused) ──────────────────
    void BuildOverlay()
    {
        if (_overlay != null) return;

        Transform canvasT = gameOverPanel != null ? gameOverPanel.transform.parent : null;
        if (canvasT == null)
        {
            var c = FindObjectOfType<Canvas>();
            if (c != null) canvasT = c.transform;
        }

#if UNITY_EDITOR
        if (backgroundSprite == null)
            backgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/UI/Textures/GameOverBackground.png");

        if (buttonSprite == null)
        {
            // Same button art the Pause / Dialogue / Scoreboard screens use,
            // so the Game Over buttons match the rest of the game.
            const string sfPath = "Assets/Unity UI Samples/Textures and Sprites/SF UI/SF Button.psd";
            foreach (var rep in UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(sfPath))
            {
                if (rep is Sprite s)
                {
                    if (s.name == "SF Button - Hover") { buttonSprite = s; break; }
                    if (buttonSprite == null) buttonSprite = s; // fallback: first sprite
                }
            }
        }
#endif

        // Full-screen background art on its own high-priority canvas so it
        // always draws above the gameplay HUD.
        _overlay = NewUI("GameOverOverlay", canvasT);
        Stretch(_overlay);
        var cv = _overlay.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 1000;
        _overlay.AddComponent<GraphicRaycaster>();

        var bg = AddImage(_overlay, Color.white);
        if (backgroundSprite != null)
        {
            bg.sprite = backgroundSprite;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
        }
        else
        {
            bg.color = new Color(0.05f, 0.07f, 0.12f, 1f); // fallback
        }

        // Level name (top of the content panel)
        _subtitle = MakeText(_overlay.transform, "Subtitle", "Level:  —",
                             30, FontStyles.Bold, InkTitle,
                             new Vector2(0.5f, 0.63f), new Vector2(0, 46),
                             TextAlignmentOptions.Center);
        StretchWidth(_subtitle.rectTransform, 0.2f);

        // Score breakdown — two aligned columns, no dot leaders.
        _labels = MakeColumn("Labels", TextAlignmentOptions.TopRight, InkBody,
                             new Vector2(0.24f, 0.355f), new Vector2(0.525f, 0.575f));
        _values = MakeColumn("Values", TextAlignmentOptions.TopLeft, InkValue,
                             new Vector2(0.555f, 0.355f), new Vector2(0.76f, 0.575f));

        // Buttons (lower area of the panel)
        MakeButton(_overlay.transform, "Button_Retry",    "TRY  AGAIN", 0.285f,
                   new Color(0.16f, 0.55f, 0.30f), RetryLevel);
        MakeButton(_overlay.transform, "Button_MainMenu", "MAIN  MENU", 0.195f,
                   new Color(0.18f, 0.40f, 0.72f), ReturnToMainMenu);
        MakeButton(_overlay.transform, "Button_Quit",     "QUIT",       0.105f,
                   new Color(0.72f, 0.26f, 0.24f), QuitGame);
    }

    // ── UI helpers ────────────────────────────────────────────────
    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static Image AddImage(GameObject go, Color c)
    {
        var img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.color = c;
        return img;
    }

    static TMP_Text MakeText(Transform parent, string name, string text,
                             float size, FontStyles style, Color color,
                             Vector2 anchor, Vector2 sizeDelta, TextAlignmentOptions align)
    {
        var go = NewUI(name, parent);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = sizeDelta;
        r.anchoredPosition = Vector2.zero;

        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align; t.richText = true;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    // A stats column anchored between two screen fractions (rows line up
    // because both columns share font size / line spacing / top alignment).
    TMP_Text MakeColumn(string name, TextAlignmentOptions align, Color color,
                        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = NewUI(name, _overlay.transform);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = r.offsetMax = Vector2.zero;

        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = 26; t.color = color; t.richText = true;
        t.alignment = align; t.lineSpacing = 14f;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    static void StretchWidth(RectTransform r, float margin)
    {
        Vector2 aMin = r.anchorMin, aMax = r.anchorMax;
        aMin.x = margin; aMax.x = 1f - margin;
        r.anchorMin = aMin; r.anchorMax = aMax;
        Vector2 sd = r.sizeDelta; sd.x = 0f; r.sizeDelta = sd;
        r.anchoredPosition = new Vector2(0f, r.anchoredPosition.y);
    }

    void MakeButton(Transform parent, string name, string label, float anchorY,
                    Color bg, UnityEngine.Events.UnityAction action)
    {
        var go = NewUI(name, parent);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.34f, anchorY);
        r.anchorMax = new Vector2(0.66f, anchorY);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(0f, 58f);
        r.anchoredPosition = Vector2.zero;

        var img = AddImage(go, bg);
        if (buttonSprite != null)
        {
            img.sprite = buttonSprite;      // shared SF Button art
            img.type = Image.Type.Sliced;   // 9-slice so it scales cleanly
            img.pixelsPerUnitMultiplier = 1f;
        }
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.highlightedColor = Color.Lerp(bg, Color.white, 0.28f);
        colors.pressedColor     = Color.Lerp(bg, Color.black, 0.20f);
        colors.fadeDuration     = 0.08f;
        btn.colors = colors;
        btn.onClick.AddListener(action);

        var t = MakeText(go.transform, "Label", label,
                         26, FontStyles.Bold, Color.white,
                         new Vector2(0.5f, 0.5f), Vector2.zero,
                         TextAlignmentOptions.Center);
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
    }

    // ── Buttons ───────────────────────────────────────────────────
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.pickupsThisScene = 0;
            GameManager.Instance.thermalStonesCollected  = 0;
            GameManager.Instance.foodSuppliesCollected   = 0;
            GameManager.Instance.winterClothingCollected = 0;
            GameManager.Instance.score = 0;
            GameManager.Instance.lives = 3;
            GameManager.Instance.freezeTime = 0f;
            GameManager.Instance.thermalStones = 10;
            GameManager.Instance.foodSupplies = 10;
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
