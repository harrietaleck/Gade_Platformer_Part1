using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// GuideScreen — controls help overlay.
// GuideBackground art already includes the "GUIDE" title; content
// is laid out in the white panel BELOW that title with a playful
// Jupiter display font + outlined game-style type.
// ============================================================
public class GuideScreen : MonoBehaviour
{
    [Header("Panel")]
    public GameObject guidePanel;

    [Header("Optional legacy text (hidden — replaced at runtime)")]
    public TMP_Text controlsText;

    GameObject _contentRoot;
    bool _built;

    static TMP_FontAsset _funFont;

    static readonly string[] ControlLines =
    {
        "<color=#1578FF>W / UP</color>      <color=#FF7A3D>—</color>  MOVE FORWARD",
        "<color=#1578FF>S / DOWN</color>    <color=#FF7A3D>—</color>  MOVE BACKWARD",
        "<color=#1578FF>A / LEFT</color>    <color=#FF7A3D>—</color>  STRAFE LEFT",
        "<color=#1578FF>D / RIGHT</color>   <color=#FF7A3D>—</color>  STRAFE RIGHT",
        "<color=#FF5A2A>SPACE</color>       <color=#FF7A3D>—</color>  JUMP",
        "<color=#FF5A2A>SHIFT</color>       <color=#FF7A3D>—</color>  SPRINT / RUN",
        "<color=#8B6CFF>ESC</color>         <color=#FF7A3D>—</color>  PAUSE MENU",
    };

    void Awake()
    {
        if (guidePanel == null)
        {
            var t = transform.Find("GuidePanel");
            if (t == null) t = GameObject.Find("GuidePanel")?.transform;
            if (t != null) guidePanel = t.gameObject;
        }
    }

    public void Open()
    {
        if (guidePanel == null) return;
        EnsureFunFont();

        // If content was built before the fun font loaded, rebuild it
        if (_built && _funFont != null && _contentRoot != null && ContentNeedsFontRefresh())
            DestroyContent();

        BuildContentIfNeeded();
        guidePanel.SetActive(true);
        guidePanel.transform.SetAsLastSibling();
    }

    void DestroyContent()
    {
        if (_contentRoot != null)
            Object.Destroy(_contentRoot);
        _contentRoot = null;
        _built = false;
    }

    bool ContentNeedsFontRefresh()
    {
        var first = _contentRoot.GetComponentInChildren<TMP_Text>(true);
        return first != null && first.font != _funFont;
    }

    public void Close()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    void BuildContentIfNeeded()
    {
        if (_built || guidePanel == null) return;
        _built = true;

        HideLegacyText();
        EnsureFunFont();

        var ice = new Color(0.10f, 0.42f, 0.95f, 1f);
        var ink = new Color(0.12f, 0.16f, 0.38f, 1f);

        _contentRoot = new GameObject("GuideContent", typeof(RectTransform));
        _contentRoot.transform.SetParent(guidePanel.transform, false);
        var root = _contentRoot.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.36f, 0.18f);
        root.anchorMax = new Vector2(0.80f, 0.64f);
        root.offsetMin = root.offsetMax = Vector2.zero;

        var cardImg = _contentRoot.AddComponent<Image>();
        cardImg.color = new Color(1f, 1f, 1f, 0.90f);
        cardImg.raycastTarget = false;

        var vlg = _contentRoot.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(34, 26, 20, 14);
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        AddHeader(_contentRoot.transform, "CONTROLS!", ice, 34, true);
        foreach (var line in ControlLines)
            AddLine(_contentRoot.transform, line, ink, 23);

        AddSpacer(_contentRoot.transform, 8);
        AddHeader(_contentRoot.transform, "SURVIVE!", ice, 32, true);
        AddBody(_contentRoot.transform,
            "GRAB THERMAL STONES, FOOD & WINTER CLOTHING!\n" +
            "HIT CHECKPOINTS TO LOCK IN YOUR PROGRESS.\n" +
            "KEEP FREEZE UNDER 1:00 — ZERO HEALTH = GAME OVER!",
            ink);

        PlaceCloseButton();
    }

    void HideLegacyText()
    {
        if (controlsText == null)
        {
            var legacy = guidePanel.transform.Find("ControlsText");
            if (legacy != null)
                controlsText = legacy.GetComponent<TMP_Text>();
        }
        if (controlsText != null)
            controlsText.gameObject.SetActive(false);

        foreach (Transform child in guidePanel.transform)
        {
            if (child.name == "GuideContent" || child.name.StartsWith("Button_"))
                continue;
            if (child.GetComponent<TMP_Text>() != null)
                child.gameObject.SetActive(false);
        }
    }

    void PlaceCloseButton()
    {
        var close = guidePanel.transform.Find("Button_CloseGuide");
        if (close == null) return;

        close.SetAsLastSibling();
        var crt = close.GetComponent<RectTransform>();
        if (crt == null) return;

        crt.anchorMin = crt.anchorMax = new Vector2(0.58f, 0.10f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(220, 54);
        crt.anchoredPosition = Vector2.zero;

        // Match Close label to Jupiter when TMP is present
        var closeTmp = close.GetComponentInChildren<TextMeshProUGUI>(true);
        if (closeTmp != null && _funFont != null)
        {
            closeTmp.font = _funFont;
            closeTmp.fontSize = 26;
            closeTmp.characterSpacing = 4f;
            closeTmp.outlineWidth = 0.12f;
            closeTmp.outlineColor = new Color(0.05f, 0.1f, 0.3f, 0.8f);
        }
    }

    static void AddHeader(Transform parent, string text, Color color, float size, bool outlined)
    {
        // Don't use FontStyles.Bold — Jupiter has no bold weight and TMP falls
        // back to LiberationSans, which kills the gamey look.
        var tmp = MakeTmp(parent, text, size, FontStyles.Normal, color, TextAlignmentOptions.Left);
        tmp.characterSpacing = 8f;
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            new Color(0.25f, 0.75f, 1f),
            new Color(0.55f, 0.45f, 1f),
            new Color(0.08f, 0.40f, 0.95f),
            new Color(0.35f, 0.25f, 0.95f));
        if (outlined)
            ApplyHeaderStyle(tmp);
        var le = tmp.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = size + 14;
        le.minHeight = size + 12;
    }

    static void AddLine(Transform parent, string text, Color color, float size)
    {
        var tmp = MakeTmp(parent, text, size, FontStyles.Normal, color, TextAlignmentOptions.Left);
        tmp.richText = true;
        tmp.characterSpacing = 1.5f;
        ApplyBodyStyle(tmp);
        var le = tmp.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = size + 10;
        le.minHeight = size + 8;
    }

    static void AddBody(Transform parent, string text, Color color)
    {
        var tmp = MakeTmp(parent, text, 20, FontStyles.Normal, color, TextAlignmentOptions.TopLeft);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.lineSpacing = 10f;
        tmp.characterSpacing = 1f;
        ApplyBodyStyle(tmp);
        var le = tmp.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 96;
        le.minHeight = 84;
        le.flexibleWidth = 1;
    }

    static void AddSpacer(Transform parent, float height)
    {
        var go = new GameObject("Spacer", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
    }

    static TextMeshProUGUI MakeTmp(Transform parent, string text, float size,
                                    FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.richText = true;
        if (_funFont != null)
            tmp.font = _funFont;
        return tmp;
    }

    static void EnsureFunFont()
    {
        if (_funFont != null) return;

        var baked = Resources.Load<TMP_FontAsset>("Fonts/JupiterSDF");
        if (baked != null)
        {
            _funFont = baked;
            WireFallback(_funFont);
            WarmGlyphs(_funFont);
            return;
        }

        var font = Resources.Load<Font>("Fonts/Jupiter");
#if UNITY_EDITOR
        if (font == null)
            font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Jupiter.ttf");
        if (font == null)
            font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Unity UI Samples/Fonts/Jupiter/Jupiter.ttf");
#endif
        if (font == null)
        {
            Debug.LogWarning("GuideScreen: Jupiter font not found — using default TMP font.");
            return;
        }

        _funFont = TMP_FontAsset.CreateFontAsset(font);
        if (_funFont == null) return;

        _funFont.name = "Jupiter SDF (Runtime)";
        WireFallback(_funFont);
        WarmGlyphs(_funFont);
    }

    static void WarmGlyphs(TMP_FontAsset font)
    {
        // Keep body text on Jupiter instead of falling back to LiberationSans
        const string charset =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            " !?/:—-=.&<>+*'\"%";
        font.TryAddCharacters(charset);
    }

    static void WireFallback(TMP_FontAsset font)
    {
        var def = TMP_Settings.defaultFontAsset;
        if (def == null || def == font) return;
        if (font.fallbackFontAssetTable == null)
            font.fallbackFontAssetTable = new List<TMP_FontAsset>();
        if (!font.fallbackFontAssetTable.Contains(def))
            font.fallbackFontAssetTable.Add(def);
    }

    static void ApplyHeaderStyle(TMP_Text tmp)
    {
        // Chunky white outline + soft dark drop for arcade/HUD titles
        var mat = new Material(tmp.font.material);
        tmp.fontMaterial = mat;

        tmp.outlineWidth = 0.28f;
        tmp.outlineColor = new Color(1f, 1f, 1f, 1f);

        mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.25f);
        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.4f);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.7f);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.85f);
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0.05f, 0.12f, 0.35f, 0.65f));
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.18f);
    }

    static void ApplyBodyStyle(TMP_Text tmp)
    {
        var mat = new Material(tmp.font.material);
        tmp.fontMaterial = mat;

        tmp.outlineWidth = 0.14f;
        tmp.outlineColor = new Color(1f, 1f, 1f, 0.85f);
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.10f);
    }
}
