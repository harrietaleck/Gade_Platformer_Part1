using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class SceneBuilder
{
    // ── Texture Importer ───────────────────────────────────────────
    [MenuItem("Tools/Build Scenes/Fix UI Texture Imports")]
    public static void FixUITextureImports()
    {
        string[] paths = {
            "Assets/UI/Textures/SplashScreen.png",
            "Assets/UI/Textures/MainMenuBackground.png",
            "Assets/UI/Textures/PauseMenuBackground.png",
            "Assets/UI/Textures/NarrativeBackground.png",
            "Assets/UI/Textures/GameOverBackground.png",
            "Assets/UI/Textures/GuideBackground.png",
            "Assets/Materials/Textures/IcePlatform.png",
            "Assets/Materials/Textures/SnowGround.png"
        };

        foreach (var path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning("Not found: " + path); continue; }

            bool isUI = path.Contains("UI/Textures");
            importer.textureType = isUI ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (isUI) importer.spriteImportMode = SpriteImportMode.Single;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("Fixed import: " + path);
        }
        AssetDatabase.Refresh();
        Debug.Log("All UI textures reimported as Sprites.");
    }

    // ── Fix MainMenu Buttons + Scoreboard Background ───────────────
    [MenuItem("Tools/Build Scenes/Fix MainMenu Buttons and Scoreboard")]
    public static void FixMainMenuButtonsAndScoreboard()
    {
        // ── Import new scoreboard texture ──
        var sbImp = AssetImporter.GetAtPath("Assets/UI/Textures/ScoreBoardBackground.png") as TextureImporter;
        if (sbImp != null)
        {
            sbImp.textureType = TextureImporterType.Sprite;
            sbImp.spriteImportMode = SpriteImportMode.Single;
            sbImp.wrapMode = TextureWrapMode.Clamp;
            AssetDatabase.ImportAsset("Assets/UI/Textures/ScoreBoardBackground.png",
                                      ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();

        // ── Fix MainMenu scene ──
        EditorSceneManager.OpenScene("Assets/Scenes/Scenes/MainMenu.unity", OpenSceneMode.Single);

        // Correct button labels & scene targets
        var buttonFixes = new System.Collections.Generic.Dictionary<string, (string label, string scene)>
        {
            { "Button_BeginnerLevel",       ("Beginner Level",  "Beginner") },
            { "Button_MovingPlatformLevel", ("Advanced Level",  "Advanced") },
            { "Button_FinalLevel",          ("Expert Level",    "Expert")   },
            { "Button_Guide",               ("Guide",           "")         },
            { "Button_Settings",            ("Settings",        "")         },
            { "Button_Credits",             ("Credits",         "")         },
            { "Button_Quit",                ("Quit",            "")         },
        };

        MainMenu menuCtrl = null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            menuCtrl = root.GetComponentInChildren<MainMenu>(true);
            if (menuCtrl != null) break;
        }

        foreach (var kv in buttonFixes)
        {
            var go = GameObject.Find(kv.Key);
            if (go == null) continue;

            // Update label text
            var txt = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = kv.Value.label;

            // Re-wire click if it's a level button
            if (menuCtrl != null && !string.IsNullOrEmpty(kv.Value.scene))
            {
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    switch (kv.Value.scene)
                    {
                        case "Beginner": UnityEventTools.AddPersistentListener(btn.onClick, menuCtrl.LoadBeginner); break;
                        case "Advanced": UnityEventTools.AddPersistentListener(btn.onClick, menuCtrl.LoadAdvanced); break;
                        case "Expert":   UnityEventTools.AddPersistentListener(btn.onClick, menuCtrl.LoadExpert);   break;
                    }
                }
            }
        }

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("MainMenu buttons fixed.");

        // ── Apply scoreboard background to all game scenes ──
        string[] scenes = {
            "Assets/Scenes/Scenes/Beginner.unity",
            "Assets/Scenes/Scenes/Advanced.unity",
            "Assets/Scenes/Scenes/Expert.unity"
        };
        var sbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Textures/ScoreBoardBackground.png");

        foreach (var sp in scenes)
        {
            EditorSceneManager.OpenScene(sp, OpenSceneMode.Single);
            var sbPanel = GameObject.Find("ScoreboardPanel");
            if (sbPanel != null)
            {
                var img = sbPanel.GetComponent<Image>();
                if (img != null && sbSprite != null)
                {
                    img.sprite = sbSprite;
                    img.color  = Color.white;
                    img.type   = Image.Type.Simple;
                }
                // Fix title text
                var titleTxt = sbPanel.transform.Find("Title")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (titleTxt != null)
                {
                    titleTxt.text  = "SCORE BOARD";
                    titleTxt.color = new Color(0.85f, 0.3f, 0.05f);
                    titleTxt.fontSize = 58;
                    titleTxt.fontStyle = TMPro.FontStyles.Bold;
                }
                Debug.Log("Scoreboard updated in: " + sp);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Reopen MainMenu
        EditorSceneManager.OpenScene("Assets/Scenes/Scenes/MainMenu.unity", OpenSceneMode.Single);
        Debug.Log("All done.");
    }

    // ── Fix MainMenu Background & EventSystem ──────────────────────
    [MenuItem("Tools/Build Scenes/Fix MainMenu Background")]
    public static void FixMainMenuBackground()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Scenes/MainMenu.unity", OpenSceneMode.Single);

        // Apply background sprite
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Textures/MainMenuBackground.png");
        var bgGO = GameObject.Find("Background");
        if (bgGO != null)
        {
            var img = bgGO.GetComponent<Image>();
            if (img != null && bgSprite != null)
            {
                img.sprite = bgSprite;
                img.color = Color.white;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
                Debug.Log("Background sprite set.");
            }
            else Debug.LogWarning("Image or sprite missing on Background.");
        }
        else Debug.LogWarning("Background GameObject not found.");

        // Add EventSystem if missing
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("EventSystem added.");
        }

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("MainMenu background fixed and saved.");
    }

    // ── Menu items ─────────────────────────────────────────────────
    [MenuItem("Tools/Build Scenes/Build SplashScreen Scene")]
    public static void BuildSplashScreen()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGO.tag = "MainCamera";

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background Image
        var bgGO = new GameObject("SplashImage");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();

        // Load splash texture
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Textures/SplashScreen.png");
        if (sprite != null)
        {
            bgImg.sprite = sprite;
            bgImg.preserveAspect = false;
        }
        else
        {
            Debug.LogWarning("SplashScreen.png not found. Import it as a Sprite first.");
        }

        // SplashController
        var ctrlGO = new GameObject("SplashController");
        var splashScript = ctrlGO.AddComponent<SplashScreen>();
        splashScript.splashImage = bgImg;
        splashScript.fadeInDuration  = 1.2f;
        splashScript.holdDuration    = 2.0f;
        splashScript.fadeOutDuration = 0.8f;
        splashScript.nextSceneName = "MainMenu";

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Scenes/SplashScreen.unity");
        Debug.Log("SplashScreen scene created at Assets/Scenes/Scenes/SplashScreen.unity");

        AddSceneToBuildSettingsAtIndex0("Assets/Scenes/Scenes/SplashScreen.unity");
    }

    [MenuItem("Tools/Build Scenes/Build MainMenu Scene")]
    public static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        camGO.tag = "MainCamera";

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Textures/MainMenuBackground.png");
        if (bgSprite != null) bgImg.sprite = bgSprite;

        // Title
        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(canvasGO.transform, false);
        var titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(600, 80);
        titleRect.anchoredPosition = Vector2.zero;
        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "MAIN MENU";
        titleTxt.fontSize = 60;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.1f, 0.2f, 0.6f);
        titleTxt.fontStyle = FontStyles.Bold;

        // Button panel
        var panelGO = new GameObject("ButtonPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.35f, 0.15f);
        panelRect.anchorMax = new Vector2(0.65f, 0.82f);
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        var layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        // MainMenu controller
        var ctrlGO = new GameObject("MainMenuController");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        ctrlGO.AddComponent<MainMenu>();

        string[] buttonLabels = { "Beginner Level", "Moving Platform Level", "Final Level", "Guide", "Settings", "Credits", "Quit" };
        foreach (var label in buttonLabels)
            CreateButton(panelGO.transform, label, 280, 55);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Scenes/MainMenu.unity");
        Debug.Log("MainMenu scene created at Assets/Scenes/Scenes/MainMenu.unity");
        AddSceneToBuildSettings("Assets/Scenes/Scenes/MainMenu.unity");
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static GameObject CreateButton(Transform parent, string label, float width, float height)
    {
        var btnGO = new GameObject("Button_" + label.Replace(" ", ""));
        btnGO.transform.SetParent(parent, false);
        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.85f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.8f, 0.9f, 1f);
        colors.pressedColor = new Color(0.6f, 0.7f, 0.9f);
        btn.colors = colors;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 22;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = new Color(0.05f, 0.1f, 0.3f);
        txt.fontStyle = FontStyles.Bold;

        return btnGO;
    }

    private static void AddSceneToBuildSettingsAtIndex0(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
        for (int i = 0; i < scenes.Length; i++)
            newScenes[i + 1] = scenes[i];
        EditorBuildSettings.scenes = newScenes;
        Debug.Log("Added " + scenePath + " to Build Settings at index 0.");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Added " + scenePath + " to Build Settings.");
        }
    }
}
