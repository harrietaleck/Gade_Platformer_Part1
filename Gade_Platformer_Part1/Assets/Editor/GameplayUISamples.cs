using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayUISamples
{
    public const string ButtonPrefabPath = "Assets/Unity UI Samples/Prefabs/SF Button.prefab";
    public const string SliderPrefabPath = "Assets/Unity UI Samples/Prefabs/SF Slider.prefab";
    public const string TitlePrefabPath  = "Assets/Unity UI Samples/Prefabs/SF Title.prefab";
    public const string HudPanelSpritePath = "Assets/Unity UI Samples/Textures and Sprites/Rounded UI/UIPanel.png";

    static GameObject _buttonPrefab;
    static GameObject _sliderPrefab;
    static GameObject _titlePrefab;

    public static Button CreateButton(Transform parent, string name, string label)
    {
        var prefab = LoadButtonPrefab();
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;

        var rect = go.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(320f, 58f);

        var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        layout.preferredHeight = 58f;
        layout.minHeight = 48f;

        SetButtonLabel(go, label);
        return go.GetComponent<Button>();
    }

    public static Slider CreateSliderRow(Transform parent, string name, string label,
        float x0, float y0, float x1, float y1)
    {
        var row = new GameObject(name + "Row", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        AnchorRect(row, x0, y0, x1, y1);

        var lbl = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(row.transform, false);
        AnchorRect(lbl, 0f, 0f, 0.34f, 1f);
        var text = lbl.AddComponent<Text>();
        text.text = label;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var sliderPrefab = LoadSliderPrefab();
        var sliderGO = (GameObject)PrefabUtility.InstantiatePrefab(sliderPrefab, row.transform);
        sliderGO.name = name;
        AnchorRect(sliderGO, 0.36f, 0.15f, 1f, 0.85f);

        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    public static GameObject CreateTitle(Transform parent, string name, string title,
        float x0, float y0, float x1, float y1)
    {
        var prefab = LoadTitlePrefab();
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;
        AnchorRect(go, x0, y0, x1, y1);

        var label = go.transform.Find("TitleLabel");
        if (label != null)
        {
            var text = label.GetComponent<Text>();
            if (text != null) text.text = title;
        }

        return go;
    }

    public static void SetButtonLabel(GameObject buttonRoot, string label)
    {
        var labelTransform = buttonRoot.transform.Find("Label");
        if (labelTransform == null)
        {
            var text = buttonRoot.GetComponentInChildren<Text>(true);
            if (text != null) text.text = label;
            return;
        }

        var uiText = labelTransform.GetComponent<Text>();
        if (uiText != null) uiText.text = label;
    }

    public static void AnchorRect(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rect = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(x0, y0);
        rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static void RemoveOverlayTitles(Transform canvas)
    {
        foreach (var panelName in new[] { "PausePanel", "GameOverPanel", "GuidePanel", "ScoreboardPanel" })
        {
            var panel = canvas.Find(panelName);
            if (panel == null) continue;

            var title = panel.Find("Title");
            if (title != null)
                Object.DestroyImmediate(title.gameObject);

            var settings = panel.Find("SettingsPanel");
            if (settings == null) continue;
            var settingsTitle = settings.Find("Title");
            if (settingsTitle != null)
                Object.DestroyImmediate(settingsTitle.gameObject);
        }
    }

    public static void StyleGameplayHudPanels(Transform canvas)
    {
        StyleHudPanel(canvas, "Panel1", true,
            new[] { "WinterClothingText", "FoodSuppliesText", "ThermalStonesText" },
            new Vector2(300f, 132f), new Vector2(16f, -16f));

        StyleHudPanel(canvas, "Panel2", false,
            new[] { "LivesText", "Score(Text)" },
            new Vector2(220f, 96f), new Vector2(-16f, -16f));
    }

    public static void StyleDialoguePanel(Transform canvas)
    {
        var dialogue = Object.FindFirstObjectByType<DialogueController>();
        if (dialogue == null || dialogue.dialoguePanel == null) return;

        var panel = dialogue.dialoguePanel.transform;
        ApplyRoundedPanelStyle(dialogue.dialoguePanel);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.localScale = Vector3.one;
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchorMin = new Vector2(0.06f, 0.04f);
        panelRect.anchorMax = new Vector2(0.94f, 0.24f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 16, 12);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        StyleDialogueText(panel, "SpeakerNameText", 26, FontStyles.Bold, 30f);
        StyleDialogueText(panel, "DialogueText", 20, FontStyles.Normal, 72f, wrap: true);
        EnsureDialogueNextButton(dialogue, panel);
    }

    static void StyleDialogueText(Transform panel, string name, float fontSize, FontStyles style,
        float preferredHeight, bool wrap = false)
    {
        var text = FindChildFuzzy(panel, name);
        if (text == null) return;

        text.SetParent(panel, false);
        var rect = text.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, preferredHeight);

        var le = text.GetComponent<LayoutElement>() ?? text.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.minHeight = preferredHeight * 0.75f;

        var tmp = text.GetComponent<TMP_Text>();
        if (tmp == null) return;

        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        if (wrap)
        {
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    static void EnsureDialogueNextButton(DialogueController dialogue, Transform panel)
    {
        var buttonRow = panel.Find("ButtonRow");
        if (buttonRow == null)
        {
            var rowGO = new GameObject("ButtonRow", typeof(RectTransform));
            buttonRow = rowGO.transform;
            buttonRow.SetParent(panel, false);

            var rowRect = rowGO.GetComponent<RectTransform>();
            rowRect.localScale = Vector3.one;
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = Vector2.zero;
            rowRect.sizeDelta = new Vector2(0f, 52f);

            var rowLayout = rowGO.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 52f;
            rowLayout.minHeight = 48f;

            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(0, 0, 0, 0);
        }

        var oldButton = panel.Find("NextButton");
        if (oldButton == null)
            oldButton = buttonRow.Find("NextButton");
        if (oldButton == null) return;

        Button button;
        if (IsSampleButton(oldButton.gameObject))
        {
            oldButton.SetParent(buttonRow, false);
            ResetDialogueButtonRect(oldButton);
            button = oldButton.GetComponent<Button>();
        }
        else
        {
            var sibling = oldButton.GetSiblingIndex();
            Object.DestroyImmediate(oldButton.gameObject);

            var newButtonGO = (GameObject)PrefabUtility.InstantiatePrefab(LoadButtonPrefab(), buttonRow);
            newButtonGO.name = "NextButton";
            newButtonGO.transform.SetSiblingIndex(sibling);
            ResetDialogueButtonRect(newButtonGO.transform);
            SetButtonLabel(newButtonGO, "Next");
            button = newButtonGO.GetComponent<Button>();
        }

        if (button == null) return;

        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, dialogue.ShowNextLine);
        dialogue.nextButton = button;
        EditorUtility.SetDirty(dialogue);
    }

    static void ResetDialogueButtonRect(Transform button)
    {
        var rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 48f);

        var le = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 180f;
        le.preferredHeight = 48f;
        le.minHeight = 44f;
    }

    static bool IsSampleButton(GameObject buttonRoot)
    {
        return buttonRoot.transform.Find("Background") != null;
    }

    static void StyleHudPanel(Transform canvas, string panelName, bool leftSide,
        string[] textNames, Vector2 size, Vector2 cornerOffset)
    {
        var panel = canvas.Find(panelName);
        if (panel == null) return;

        ApplyRoundedPanelStyle(panel.gameObject);

        var rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.pivot = leftSide ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rect.anchorMin = rect.anchorMax = new Vector2(leftSide ? 0f : 1f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = cornerOffset;
        panel.SetAsFirstSibling();

        var layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 6;
        layout.childAlignment = leftSide ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        foreach (var textName in textNames)
        {
            var text = FindChildFuzzy(canvas, textName);
            if (text == null) continue;

            text.SetParent(panel, false);
            var textRect = text.GetComponent<RectTransform>();
            textRect.localScale = Vector3.one;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(leftSide ? 0f : 1f, 1f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0f, 28f);

            var le = text.GetComponent<LayoutElement>() ?? text.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 28f;
            le.minHeight = 24f;

            var tmp = text.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.fontSize = 22;
                tmp.color = Color.white;
                tmp.alignment = leftSide ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            }
        }
    }

    static void ApplyRoundedPanelStyle(GameObject go)
    {
        var raw = go.GetComponent<RawImage>();
        if (raw != null)
            Object.DestroyImmediate(raw);

        var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.sprite = LoadHudPanelSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.9f);
        image.raycastTarget = false;
    }

    static Transform FindChildFuzzy(Transform parent, string name)
    {
        var direct = parent.Find(name);
        if (direct != null) return direct;

        var trimmed = name.Trim();
        foreach (Transform child in parent)
        {
            if (child.name.Trim() == trimmed)
                return child;
        }

        return null;
    }

    static Sprite _hudPanelSprite;

    static Sprite LoadHudPanelSprite()
    {
        if (_hudPanelSprite != null) return _hudPanelSprite;

        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(HudPanelSpritePath))
        {
            if (asset is Sprite sprite && sprite.name == "UIPanel_0")
            {
                _hudPanelSprite = sprite;
                break;
            }
        }

        if (_hudPanelSprite == null)
            Debug.LogError("[GameplayUI] Missing HUD sprite: " + HudPanelSpritePath);
        return _hudPanelSprite;
    }

    static GameObject LoadButtonPrefab()
    {
        if (_buttonPrefab == null)
            _buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
        if (_buttonPrefab == null)
            Debug.LogError("[GameplayUI] Missing prefab: " + ButtonPrefabPath);
        return _buttonPrefab;
    }

    static GameObject LoadSliderPrefab()
    {
        if (_sliderPrefab == null)
            _sliderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SliderPrefabPath);
        if (_sliderPrefab == null)
            Debug.LogError("[GameplayUI] Missing prefab: " + SliderPrefabPath);
        return _sliderPrefab;
    }

    static GameObject LoadTitlePrefab()
    {
        if (_titlePrefab == null)
            _titlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TitlePrefabPath);
        if (_titlePrefab == null)
            Debug.LogError("[GameplayUI] Missing prefab: " + TitlePrefabPath);
        return _titlePrefab;
    }
}
