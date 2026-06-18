using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string beginnerSceneName = "Beginner";
    public string advancedSceneName = "Advanced";
    public string expertSceneName   = "Expert";

    [Header("Main Menu Background")]
    [SerializeField] private Sprite menuBackground;  // Auto-loaded from Assets/UI/Textures/MainMenuBackground.png

    [Header("Guide")]
    public Sprite guideBackground;   // Assign GuideBackground.png in Inspector (or via editor setup)

    // ── SF Button Prefab (Deliverable 4 — consistent UI across all screens)
    // Drag Assets/Unity UI Samples/Prefabs/SF Button.prefab here in the Inspector.
    // When assigned, all buttons use the white/blue-border SF Button style.
    // If left empty, a plain coloured button is built at runtime as a fallback.
    [Header("SF Button (assign for consistent UI)")]
    [SerializeField] private Button sfButtonPrefab;

    // Panels — created at runtime if not pre-assigned in the Inspector
    [HideInInspector] public GameObject settingsPanel;
    [HideInInspector] public GameObject creditsPanel;
    [HideInInspector] public GameObject confirmQuitPanel;
    [HideInInspector] public GameObject guidePanel;

    Canvas rootCanvas;

    void Awake()
    {
        Time.timeScale = 1f;
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
    }

    void Start()
    {
        // ── Background ───────────────────────────────────────────────
        var bg = new GameObject("MenuBackground", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        bg.transform.SetParent(rootCanvas.transform, false);
        var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one;
        bgR.offsetMin = bgR.offsetMax = Vector2.zero;

        var bgImg = bg.GetComponent<UnityEngine.UI.Image>();

        // Auto-load MainMenuBackground.png from Assets/UI/Textures/ in the Editor if not assigned.
        // In a build the Sprite must be set in the Inspector ahead of time.
#if UNITY_EDITOR
        if (menuBackground == null)
        {
            const string BgPath = "Assets/UI/Textures/MainMenuBackground.png";
            menuBackground = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(BgPath);
        }
#endif
        if (menuBackground != null)
        {
            bgImg.sprite = menuBackground;
            bgImg.type   = UnityEngine.UI.Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.color  = Color.white;
        }
        else
        {
            bgImg.color = new Color(0.04f, 0.06f, 0.12f, 1f); // fallback dark
        }

        // ── Required level buttons (Deliverable 4) ───────────────────
        var btnBeginner = MakeBtn(bg, "Button_Beginner", "Beginner Level",
                                  0.5f, 0.65f, new Color(0.15f, 0.55f, 0.25f));
        btnBeginner.onClick.AddListener(LoadBeginner);

        var btnAdvanced = MakeBtn(bg, "Button_Advanced", "Moving Platform Level",
                                  0.5f, 0.53f, new Color(0.15f, 0.35f, 0.65f));
        btnAdvanced.onClick.AddListener(LoadAdvanced);

        var btnExpert = MakeBtn(bg, "Button_Expert", "Final Level",
                                0.5f, 0.41f, new Color(0.65f, 0.20f, 0.15f));
        btnExpert.onClick.AddListener(LoadExpert);

        var btnQuit = MakeBtn(bg, "Button_Quit", "Quit",
                              0.5f, 0.26f, new Color(0.25f, 0.25f, 0.30f));
        btnQuit.onClick.AddListener(OpenConfirmQuit);

        // ── Secondary buttons (bottom strip) ─────────────────────────
        var btnSettings = MakeBtn(bg, "Button_Settings", "Settings",
                                  0.18f, 0.08f, new Color(0.25f, 0.25f, 0.40f));
        btnSettings.onClick.AddListener(OpenSettings);

        var btnCredits = MakeBtn(bg, "Button_Credits", "Credits",
                                 0.50f, 0.08f, new Color(0.25f, 0.25f, 0.40f));
        btnCredits.onClick.AddListener(OpenCredits);

        var btnGuide = MakeBtn(bg, "Button_Guide", "Guide",
                               0.82f, 0.08f, new Color(0.25f, 0.25f, 0.40f));
        btnGuide.onClick.AddListener(OpenGuide);

        // ── Build overlay panels ──────────────────────────────────────
        if (settingsPanel    == null) settingsPanel    = BuildSettingsPanel();
        if (creditsPanel     == null) creditsPanel     = BuildCreditsPanel();
        if (confirmQuitPanel == null) confirmQuitPanel = BuildConfirmQuitPanel();
        if (guidePanel       == null) guidePanel       = BuildGuidePanel();

        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
        guidePanel.SetActive(false);
    }

    // ── Panel builders ─────────────────────────────────────────────

    GameObject BuildSettingsPanel()
    {
        var panel = MakeOverlay("SettingsPanel", new Color(0.05f, 0.05f, 0.15f, 0.95f));
        MakeTxt(panel, "SETTINGS", 0.5f, 0.88f, 42, Color.white, FontStyles.Bold);

        MakeTxtLeft(panel, "Master Volume", 0.28f, 0.73f, 20, Color.white);
        MakeTxtLeft(panel, "Music Volume",  0.28f, 0.60f, 20, Color.white);
        MakeTxtLeft(panel, "SFX Volume",    0.28f, 0.47f, 20, new Color(0.6f, 0.9f, 1f));

        MakeVisualSlider(panel, "MasterVolume", 0.65f, 0.73f, SetMasterVolume, PlayerPrefs.GetFloat("MasterVolume", 1f));
        MakeVisualSlider(panel, "MusicVolume",  0.65f, 0.60f, SetMusicVolume,  PlayerPrefs.GetFloat("MusicVolume",  1f));
        MakeVisualSlider(panel, "SFXVolume",    0.65f, 0.47f, SetSFXVolume,    PlayerPrefs.GetFloat("SFXVolume",    1f));

        // Wire close button directly — no inactive-search issue
        var closeBtn = MakeBtn(panel, "Button_CloseSettings", "Close", 0.5f, 0.12f, new Color(0.8f, 0.3f, 0.3f));
        closeBtn.onClick.AddListener(CloseSettings);
        return panel;
    }

    GameObject BuildCreditsPanel()
    {
        var panel = MakeOverlay("CreditsPanel", new Color(0.05f, 0.05f, 0.15f, 0.95f));
        MakeTxt(panel, "CREDITS",                 0.5f, 0.84f, 42, Color.white,               FontStyles.Bold);
        MakeTxt(panel, "Game Developer",          0.5f, 0.68f, 20, new Color(0.6f, 0.9f, 1f), FontStyles.Bold);
        MakeTxt(panel, "Harriet Manda",           0.5f, 0.58f, 28, Color.white,               FontStyles.Bold);
        MakeTxt(panel, "Game Development Module", 0.5f, 0.44f, 18, Color.gray,                FontStyles.Normal);
        MakeTxt(panel, "GADE  |  2026",           0.5f, 0.35f, 18, Color.gray,                FontStyles.Normal);
        MakeTxt(panel, "The Last Messanger",      0.5f, 0.24f, 16, new Color(0.8f,0.8f,0.8f), FontStyles.Italic);

        var closeBtn = MakeBtn(panel, "Button_CloseCredits", "Close", 0.5f, 0.10f, new Color(0.8f, 0.3f, 0.3f));
        closeBtn.onClick.AddListener(CloseCredits);
        return panel;
    }

    GameObject BuildConfirmQuitPanel()
    {
        var panel = MakeOverlay("ConfirmQuitPanel", new Color(0.1f, 0.04f, 0.04f, 0.97f),
                                0.25f, 0.30f, 0.75f, 0.70f);
        MakeTxt(panel, "Are you sure you want to quit?", 0.5f, 0.75f, 22, Color.white, FontStyles.Normal);

        var yesBtn = MakeBtn(panel, "Button_ConfirmYes", "Yes — Quit", 0.28f, 0.30f, new Color(0.8f, 0.3f, 0.3f));
        yesBtn.onClick.AddListener(QuitGame);

        var noBtn = MakeBtn(panel, "Button_ConfirmNo", "No — Stay",  0.72f, 0.30f, new Color(0.3f, 0.7f, 0.3f));
        noBtn.onClick.AddListener(CancelQuit);
        return panel;
    }

    GameObject BuildGuidePanel()
    {
        // Full-screen background
        var panel = new GameObject("GuidePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(rootCanvas.transform, false);
        var r = panel.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;

        var img = panel.GetComponent<Image>();
        if (guideBackground != null)
        {
            img.sprite = guideBackground;
            img.preserveAspect = false;
            img.color = Color.white;
        }
        else
            img.color = new Color(0.05f, 0.05f, 0.15f, 0.97f);

        // Semi-transparent content card overlaid on top of the background image
        var card = new GameObject("ContentCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(panel.transform, false);
        var cr = card.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.05f, 0.08f); cr.anchorMax = new Vector2(0.95f, 0.92f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        card.GetComponent<Image>().color = new Color(0.92f, 0.95f, 1.0f, 0.88f); // light icy-white, navy text readable

        // Navy-blue palette \u2014 readable on both the light GuideBackground image
        // and the semi-transparent card overlay.
        var navyTitle   = new Color(0.00f, 0.05f, 0.35f); // deep navy  (headings)
        var navySection = new Color(0.00f, 0.10f, 0.45f); // mid navy   (section labels)
        var navyBody    = new Color(0.05f, 0.15f, 0.50f); // softer navy (body text)

        // Title
        MakeTxtWide(card, "PLAYER GUIDE",  0.5f, 0.91f, 36, navyTitle,   FontStyles.Bold, 700);

        // Section: Movement
        MakeTxtWide(card, "MOVEMENT",      0.5f, 0.80f, 20, navySection, FontStyles.Bold, 600);
        MakeTxtWide(card, "Move Left       A  or  \u2190 Arrow",  0.5f, 0.72f, 17, navyBody, FontStyles.Normal, 600);
        MakeTxtWide(card, "Move Right      D  or  \u2192 Arrow",  0.5f, 0.64f, 17, navyBody, FontStyles.Normal, 600);
        MakeTxtWide(card, "Jump            Space  or  \u2191 Arrow", 0.5f, 0.56f, 17, navyBody, FontStyles.Normal, 600);
        MakeTxtWide(card, "Crouch / Fall   S  or  \u2193 Arrow",  0.5f, 0.48f, 17, navyBody, FontStyles.Normal, 600);

        // Section: Combat
        MakeTxtWide(card, "COMBAT",        0.5f, 0.37f, 20, navySection, FontStyles.Bold, 600);
        MakeTxtWide(card, "Attack         Left Mouse Button",  0.5f, 0.29f, 17, navyBody, FontStyles.Normal, 600);
        MakeTxtWide(card, "Aim / Look     Move Mouse",         0.5f, 0.21f, 17, navyBody, FontStyles.Normal, 600);

        // Section: Game
        MakeTxtWide(card, "GAME",          0.5f, 0.11f, 20, navySection, FontStyles.Bold, 600);
        MakeTxtWide(card, "Pause          Escape",             0.5f, 0.03f, 17, navyBody, FontStyles.Normal, 600);

        var closeBtn = MakeBtn(panel, "Button_CloseGuide", "Close Guide", 0.5f, 0.04f, new Color(0.7f, 0.2f, 0.2f));
        closeBtn.onClick.AddListener(CloseGuide);
        return panel;
    }

    // ── UI Helpers ─────────────────────────────────────────────────

    GameObject MakeOverlay(string name, Color bg,
                            float x0 = 0f, float y0 = 0f, float x1 = 1f, float y1 = 1f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(rootCanvas.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = bg;
        return go;
    }

    void MakeTxt(GameObject panel, string text, float anchorX, float anchorY,
                  float fs, Color color, FontStyles style)
    {
        var go = new GameObject(text.Replace(" ", "_"), typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(anchorX, anchorY);
        r.sizeDelta = new Vector2(400, 40);
        r.anchoredPosition = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.color = color;
        t.fontStyle = style; t.alignment = TextAlignmentOptions.Center;
    }

    // Left-aligned label (for settings row labels)
    void MakeTxtLeft(GameObject panel, string text, float anchorX, float anchorY,
                      float fs, Color color)
    {
        var go = new GameObject(text.Replace(" ", "_"), typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(anchorX, anchorY);
        r.sizeDelta = new Vector2(220, 36);
        r.anchoredPosition = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.color = color;
        t.fontStyle = FontStyles.Normal; t.alignment = TextAlignmentOptions.MidlineRight;
    }

    // Wider text for guide content rows
    void MakeTxtWide(GameObject parent, string text, float anchorX, float anchorY,
                      float fs, Color color, FontStyles style, float width = 700)
    {
        var go = new GameObject(text.Substring(0, Mathf.Min(12, text.Length)).Replace(" ","_"),
                                typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(anchorX, anchorY);
        r.sizeDelta = new Vector2(width, 34);
        r.anchoredPosition = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.color = color;
        t.fontStyle = style; t.alignment = TextAlignmentOptions.Center;
    }

    // ── Button factory ────────────────────────────────────────────
    // When sfButtonPrefab is assigned this instantiates the SF Button
    // prefab (white background, blue border, Animator hover/press).
    // Without it, a plain coloured rectangle is built at runtime as a
    // fallback so the menu always works even during first setup.
    Button MakeBtn(GameObject panel, string name, string label,
                    float anchorX, float anchorY, Color bg)
    {
        GameObject go;

        if (sfButtonPrefab != null)
        {
            // ── SF Button path (consistent UI style) ─────────────
            go = Instantiate(sfButtonPrefab.gameObject, panel.transform, false);
            go.name = name;

            // The SF Button prefab's label child uses a regular UI Text.
            var labelText = go.transform.Find("Label")?.GetComponent<UnityEngine.UI.Text>();
            if (labelText != null) { labelText.text = label; labelText.color = Color.white; }

            // Safety: also try TMP in case the prefab was upgraded.
            var labelTmp = go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (labelTmp != null) { labelTmp.text = label; labelTmp.color = Color.white; }
        }
        else
        {
            // ── Fallback: plain coloured button ──────────────────
            go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel.transform, false);
            go.GetComponent<Image>().color = bg;

            var tgo = new GameObject("Text", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            var txt = tgo.AddComponent<TextMeshProUGUI>();
            txt.text = label; txt.fontSize = 20; txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
        }

        // Position is the same regardless of which path was taken above.
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(anchorX, anchorY);
        r.sizeDelta = new Vector2(220, 50);
        r.anchoredPosition = Vector2.zero;

        return go.GetComponent<Button>();
    }

    void MakeVisualSlider(GameObject panel, string name, float anchorX, float anchorY,
                           UnityEngine.Events.UnityAction<float> onChange, float initialValue)
    {
        // Root slider object
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(anchorX, anchorY);
        r.sizeDelta = new Vector2(220, 22); r.anchoredPosition = Vector2.zero;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;

        // Track background
        var track = new GameObject("Background", typeof(RectTransform), typeof(Image));
        track.transform.SetParent(go.transform, false);
        var tr = track.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0.25f); tr.anchorMax = new Vector2(1f, 0.75f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        track.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.35f);

        // Fill area + fill
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var far = fillArea.GetComponent<RectTransform>();
        far.anchorMin = new Vector2(0f, 0.25f); far.anchorMax = new Vector2(1f, 0.75f);
        far.offsetMin = new Vector2(5, 0); far.offsetMax = new Vector2(-15, 0);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0.5f, 1f);
        fr.offsetMin = fr.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1f);
        slider.fillRect = fr;

        // Handle slide area + handle
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var har = handleArea.GetComponent<RectTransform>();
        har.anchorMin = Vector2.zero; har.anchorMax = Vector2.one;
        har.offsetMin = new Vector2(10, 0); har.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var hr = handle.GetComponent<RectTransform>();
        hr.sizeDelta = new Vector2(20, 20); hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
        var hImg = handle.GetComponent<Image>(); hImg.color = Color.white;
        slider.handleRect = hr;
        slider.targetGraphic = hImg;

        slider.value = initialValue;
        slider.onValueChanged.AddListener(onChange);
    }

    void WireBtn(string btnName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(btnName);
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    // ── Level Buttons ──────────────────────────────────────────────

    public void LoadBeginner() => SceneManager.LoadScene(beginnerSceneName);
    public void LoadAdvanced() => SceneManager.LoadScene(advancedSceneName);
    public void LoadExpert()   => SceneManager.LoadScene(expertSceneName);

    // ── Settings Panel ─────────────────────────────────────────────

    public void OpenSettings()  { if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }

    public void SetMasterVolume(float v) { AudioListener.volume = v; PlayerPrefs.SetFloat("MasterVolume", v); }
    public void SetMusicVolume(float v)  { PlayerPrefs.SetFloat("MusicVolume", v); }
    public void SetSFXVolume(float v)    { PlayerPrefs.SetFloat("SFXVolume", v); }

    // ── Credits Panel ──────────────────────────────────────────────

    public void OpenCredits()  { if (creditsPanel != null) creditsPanel.SetActive(true); }
    public void CloseCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }

    // ── Guide Panel ────────────────────────────────────────────────

    public void OpenGuide()  { if (guidePanel != null) guidePanel.SetActive(true); }
    public void CloseGuide() { if (guidePanel != null) guidePanel.SetActive(false); }

    // ── Quit ───────────────────────────────────────────────────────

    public void OpenConfirmQuit() { if (confirmQuitPanel != null) confirmQuitPanel.SetActive(true); }
    public void CancelQuit()      { if (confirmQuitPanel != null) confirmQuitPanel.SetActive(false); }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
