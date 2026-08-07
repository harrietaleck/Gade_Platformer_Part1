using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class GameSetupBuilder
{
    static readonly string[] GameScenes = {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity"
    };

    // ══════════════════════════════════════════════════════════════
    // RUN EVERYTHING
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Run Full Setup")]
    public static void RunFullSetup()
    {
        FixTextureImports();
        CreatePlatformMaterials();
        SetupAllGameScenes();
        WireMainMenuButtons();
        AssignCollectSFX();
        Debug.Log("=== Full Game Setup Complete ===");
    }

    // ══════════════════════════════════════════════════════════════
    // STYLISH FONT – bake Jupiter as TMP SDF under Resources
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Create Jupiter TMP Font")]
    public static void CreateJupiterTmpFontAsset()
    {
        const string sourceFont = "Assets/Resources/Fonts/Jupiter.ttf";
        const string outputAsset = "Assets/Resources/Fonts/JupiterSDF.asset";

        var font = AssetDatabase.LoadAssetAtPath<Font>(sourceFont);
        if (font == null)
        {
            Debug.LogError("[Setup] Jupiter font missing at " + sourceFont);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputAsset) != null)
            AssetDatabase.DeleteAsset(outputAsset);

        var sdf = TMP_FontAsset.CreateFontAsset(font);
        if (sdf == null)
        {
            Debug.LogError("[Setup] CreateFontAsset failed for Jupiter");
            return;
        }

        sdf.name = "JupiterSDF";
        AssetDatabase.CreateAsset(sdf, outputAsset);

        if (sdf.material != null)
            AssetDatabase.AddObjectToAsset(sdf.material, sdf);
        if (sdf.atlasTextures != null)
        {
            foreach (var tex in sdf.atlasTextures)
            {
                if (tex != null)
                    AssetDatabase.AddObjectToAsset(tex, sdf);
            }
        }

        var liber = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (liber != null)
        {
            if (sdf.fallbackFontAssetTable == null)
                sdf.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!sdf.fallbackFontAssetTable.Contains(liber))
                sdf.fallbackFontAssetTable.Add(liber);
        }

        EditorUtility.SetDirty(sdf);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Setup] Jupiter TMP font ready at " + outputAsset);
    }

    // ══════════════════════════════════════════════════════════════
    // COLLECT SFX – assign collectSound.wav to all pickup prefabs
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Step 5 - Assign Collect SFX to Prefabs")]
    public static void AssignCollectSFX()
    {
        const string clipPath = "Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav";
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
        if (clip == null)
        {
            Debug.LogError("[Setup] collectSound.wav not found at: " + clipPath);
            return;
        }

        string[] prefabPaths = {
            "Assets/Prefabs/Collectibles/ThermalStone.prefab",
            "Assets/Prefabs/Collectibles/FoodSupply.prefab",
            "Assets/Prefabs/Collectibles/WinterClothing.prefab",
        };

        foreach (var path in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogWarning("[Setup] Prefab not found: " + path); continue; }

            bool changed = false;

            var thermal = prefab.GetComponent<ThermalStonePickup>();
            if (thermal != null && thermal.collectSFX == null)
            {
                thermal.collectSFX = clip;
                changed = true;
            }

            var generic = prefab.GetComponent<CollectablePickup>();
            if (generic != null && generic.collectSFX == null)
            {
                generic.collectSFX = clip;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                Debug.Log("[Setup] Assigned collectSound to: " + path);
            }
            else
            {
                Debug.Log("[Setup] Already assigned or no pickup script on: " + path);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Setup] Collect SFX assignment complete.");
    }

    // ══════════════════════════════════════════════════════════════
    // STEP 1 – FIX TEXTURE IMPORTS
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Step 1 - Fix Texture Imports")]
    public static void FixTextureImports()
    {
        string[] sprites = {
            "Assets/UI/Textures/SplashScreen.png",
            "Assets/UI/Textures/MainMenuBackground.png",
            "Assets/UI/Textures/PauseMenuBackground.png",
            "Assets/UI/Textures/NarrativeBackground.png",
            "Assets/UI/Textures/GameOverBackground.png",
            "Assets/UI/Textures/GuideBackground.png"
        };
        string[] defaults = {
            "Assets/Materials/Textures/IcePlatform.png",
            "Assets/Materials/Textures/SnowGround.png"
        };
        foreach (var p in sprites)  ImportAs(p, TextureImporterType.Sprite, TextureWrapMode.Clamp);
        foreach (var p in defaults) ImportAs(p, TextureImporterType.Default, TextureWrapMode.Repeat);
        AssetDatabase.Refresh();
        Debug.Log("[Setup] Textures reimported.");
    }

    static void ImportAs(string path, TextureImporterType type, TextureWrapMode wrap)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("Not found: " + path); return; }
        imp.textureType = type;
        if (type == TextureImporterType.Sprite) imp.spriteImportMode = SpriteImportMode.Single;
        imp.wrapMode = wrap;
        imp.maxTextureSize = 2048;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    // ══════════════════════════════════════════════════════════════
    // STEP 2 – PLATFORM MATERIALS
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Step 2 - Create & Apply Platform Materials")]
    public static void CreatePlatformMaterials()
    {
        var iceTex  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Textures/IcePlatform.png");
        var snowTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Textures/SnowGround.png");

        EnsureMaterial("Assets/Materials/IcePlatformMat.mat",  iceTex,  new Color(0.7f, 0.85f, 1f));
        EnsureMaterial("Assets/Materials/SnowGroundMat.mat", snowTex, Color.white);
        AssetDatabase.SaveAssets();

        foreach (var sp in GameScenes) ApplyMaterialsToScene(sp);
        Debug.Log("[Setup] Platform materials created and applied.");
    }

    static void EnsureMaterial(string path, Texture2D tex, Color fallback)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (tex != null) mat.SetTexture("_BaseMap", tex);
        else             mat.color = fallback;
        AssetDatabase.CreateAsset(mat, path);
    }

    static void ApplyMaterialsToScene(string scenePath)
    {
        var iceMat  = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/IcePlatformMat.mat");
        var snowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SnowGroundMat.mat");
        if (iceMat == null || snowMat == null) return;

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        foreach (var mp in Object.FindObjectsByType<MOVEplatform>(FindObjectsSortMode.None))
            foreach (var r in mp.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = iceMat;

        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.GetComponent<MOVEplatform>() != null) continue;
            string n = r.gameObject.name.ToLower();
            if (n.Contains("platform") || n.Contains("ground") || n.Contains("floor") ||
                n.Contains("terrain")  || r.gameObject.CompareTag("Ground"))
                r.sharedMaterial = snowMat;
        }

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Setup] Materials applied: " + scenePath);
    }

    // ══════════════════════════════════════════════════════════════
    // STEP 3 – SETUP ALL GAME SCENE UI
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Step 3 - Setup All Game Scene UI")]
    public static void SetupAllGameScenes()
    {
        foreach (var sp in GameScenes) SetupGameScene(sp);
    }

    public static void SetupGameScene(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Find existing canvas
        Canvas canvas = null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null) break;
        }
        if (canvas == null)
        {
            var cvGO = new GameObject("Canvas");
            canvas = cvGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var s = cvGO.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            cvGO.AddComponent<GraphicRaycaster>();
        }

        var cgo = canvas.gameObject;

        if (!HasChild(cgo, "PausePanel"))       BuildPausePanel(cgo);
        if (!HasChild(cgo, "GameOverPanel"))     BuildGameOverPanel(cgo);
        if (!HasChild(cgo, "GuidePanel"))        BuildGuidePanel(cgo);
        if (!HasChild(cgo, "ScoreboardPanel"))   BuildScoreboardPanel(cgo);
        if (!HasChild(cgo, "PauseButton"))       BuildPauseHUDButton(cgo);

        GameplayUISamples.RemoveOverlayTitles(cgo.transform);
        GameplayUISamples.StyleGameplayHudPanels(cgo.transform);
        GameplayUISamples.StyleDialoguePanel(cgo.transform);

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Setup] Scene UI done: " + scenePath);
    }

    // ──────────────────────────────────────────────────────────────
    // PAUSE PANEL
    // ──────────────────────────────────────────────────────────────
    static void BuildPausePanel(GameObject cgo)
    {
        var ctrl = GetOrAddController<PauseScreen>(cgo, "PauseController");

        var panel = MakeFullPanel(cgo.transform, "PausePanel", "Assets/UI/Textures/PauseMenuBackground.png");
        panel.SetActive(false);

        var box = MakeVBox(panel.transform, "Buttons", 0.3f, 0.1f, 0.7f, 0.84f, 12);

        var resumeBtn  = GameplayUISamples.CreateButton(box.transform, "Button_Resume",   "Resume");
        var restartBtn = GameplayUISamples.CreateButton(box.transform, "Button_Restart",  "Restart Level");
        var guideBtn   = GameplayUISamples.CreateButton(box.transform, "Button_Guide",    "Guide");
        var menuBtn    = GameplayUISamples.CreateButton(box.transform, "Button_MainMenu", "Main Menu");
        var quitBtn    = GameplayUISamples.CreateButton(box.transform, "Button_Quit",     "Quit");

        UnityEventTools.AddPersistentListener(resumeBtn.onClick,  ctrl.Resume);
        UnityEventTools.AddPersistentListener(restartBtn.onClick, ctrl.RestartLevel);
        UnityEventTools.AddPersistentListener(menuBtn.onClick,    ctrl.ReturnToMenu);
        UnityEventTools.AddPersistentListener(quitBtn.onClick,    ctrl.OpenConfirmQuit);

        // Confirm-quit overlay
        var confirmPanel = MakeAnchoredPanel(panel.transform, "ConfirmQuitPanel",
                           new Color(0.1f, 0.1f, 0.15f, 0.95f), 0.2f, 0.3f, 0.8f, 0.7f);
        confirmPanel.SetActive(false);

        MakeLabel(confirmPanel.transform, "ConfirmText", "Are you sure you want to quit?",
                  26, Color.white, 0.05f, 0.6f, 0.95f, 0.9f);

        var yesBtn = GameplayUISamples.CreateButton(confirmPanel.transform, "Button_ConfirmYes", "Yes");
        var noBtn  = GameplayUISamples.CreateButton(confirmPanel.transform, "Button_ConfirmNo",  "No");
        GameplayUISamples.AnchorRect(yesBtn.gameObject, 0.1f, 0.15f, 0.45f, 0.5f);
        GameplayUISamples.AnchorRect(noBtn.gameObject,  0.55f, 0.15f, 0.9f, 0.5f);

        UnityEventTools.AddPersistentListener(yesBtn.onClick, ctrl.QuitGame);
        UnityEventTools.AddPersistentListener(noBtn.onClick,  ctrl.CancelQuit);

        // Settings sub-panel (volume)
        var settingsPanel = MakeAnchoredPanel(panel.transform, "SettingsPanel",
                            new Color(0.05f, 0.1f, 0.2f, 0.95f), 0.15f, 0.15f, 0.85f, 0.8f);
        settingsPanel.SetActive(false);
        var masterSlider = GameplayUISamples.CreateSliderRow(settingsPanel.transform, "MasterVolumeSlider", "Master Volume",
                           0.1f, 0.62f, 0.9f, 0.74f);
        var musicSlider  = GameplayUISamples.CreateSliderRow(settingsPanel.transform, "MusicVolumeSlider",  "Music Volume",
                           0.1f, 0.46f, 0.9f, 0.58f);
        var sfxSlider    = GameplayUISamples.CreateSliderRow(settingsPanel.transform, "SFXVolumeSlider",    "SFX Volume",
                           0.1f, 0.30f, 0.9f, 0.42f);
        var closeSetBtn  = GameplayUISamples.CreateButton(settingsPanel.transform, "Button_CloseSettings", "Close");
        GameplayUISamples.AnchorRect(closeSetBtn.gameObject, 0.35f, 0.08f, 0.65f, 0.22f);
        UnityEventTools.AddPersistentListener(closeSetBtn.onClick, ctrl.CloseSettings);
        UnityEventTools.AddPersistentListener(settingsBtn(panel).onClick, ctrl.OpenSettings);
        UnityEventTools.AddPersistentListener(masterSlider.onValueChanged, ctrl.SetMasterVolume);
        UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, ctrl.SetMusicVolume);
        UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, ctrl.SetSFXVolume);

        ctrl.pausePanel      = panel;
        ctrl.confirmQuitPanel = confirmPanel;
        ctrl.settingsPanel   = settingsPanel;
        ctrl.masterVolumeSlider = masterSlider;
        ctrl.musicVolumeSlider  = musicSlider;
        ctrl.sfxVolumeSlider    = sfxSlider;

        // Wire guide btn later (GuidePanel not yet created)
    }

    static Button settingsBtn(GameObject panel)
    {
        var t = panel.transform.Find("Buttons/Button_Quit");
        // Actually we want the Settings button
        t = panel.transform.Find("Buttons");
        if (t == null) return null;
        foreach (Transform child in t)
            if (child.name == "Button_Settings") return child.GetComponent<Button>();
        // Not found yet – the Quit btn is there; let's add Settings btn right before Quit
        var quitTransform = t.Find("Button_Quit");
        var settingsButton = GameplayUISamples.CreateButton(t, "Button_Settings", "Settings");
        if (quitTransform != null) settingsButton.transform.SetSiblingIndex(quitTransform.GetSiblingIndex());
        return settingsButton;
    }

    // ──────────────────────────────────────────────────────────────
    // GAME OVER PANEL
    // ──────────────────────────────────────────────────────────────
    static void BuildGameOverPanel(GameObject cgo)
    {
        var ctrl = GetOrAddController<GameOverScreen>(cgo, "GameOverController");

        var panel = MakeFullPanel(cgo.transform, "GameOverPanel", "Assets/UI/Textures/GameOverBackground.png");
        panel.SetActive(false);

        // Stats box
        var statsBox = MakeAnchoredPanel(panel.transform, "StatsBox",
                       new Color(1f, 1f, 1f, 0.2f), 0.2f, 0.52f, 0.8f, 0.78f);
        var scoreText = MakeText(statsBox.transform, "FinalScoreText", "Score: 0", 28, Color.white);
        var levelText = MakeText(statsBox.transform, "LevelNameText",  "Level: —", 22, new Color(0.8f, 0.9f, 1f));
        AnchorRect(scoreText.gameObject, 0.05f, 0.5f, 0.95f, 0.95f);
        AnchorRect(levelText.gameObject, 0.05f, 0.05f, 0.95f, 0.45f);

        var box = MakeVBox(panel.transform, "Buttons", 0.25f, 0.08f, 0.75f, 0.48f, 10);

        var retryBtn = GameplayUISamples.CreateButton(box.transform, "Button_Retry",    "Try Again");
        var menuBtn  = GameplayUISamples.CreateButton(box.transform, "Button_MainMenu", "Main Menu");
        var quitBtn  = GameplayUISamples.CreateButton(box.transform, "Button_Quit",     "Quit");

        UnityEventTools.AddPersistentListener(retryBtn.onClick, ctrl.RetryLevel);
        UnityEventTools.AddPersistentListener(menuBtn.onClick,  ctrl.ReturnToMainMenu);
        UnityEventTools.AddPersistentListener(quitBtn.onClick,  ctrl.QuitGame);

        ctrl.gameOverPanel    = panel;
        ctrl.finalScoreText   = scoreText;
        ctrl.levelNameText    = levelText;
    }

    // ──────────────────────────────────────────────────────────────
    // GUIDE PANEL
    // ──────────────────────────────────────────────────────────────
    static void BuildGuidePanel(GameObject cgo)
    {
        var ctrl = GetOrAddController<GuideScreen>(cgo, "GuideController");

        var panel = MakeFullPanel(cgo.transform, "GuidePanel", "Assets/UI/Textures/GuideBackground.png");
        panel.SetActive(false);

        // Content is built at runtime by GuideScreen (laid out below the baked "GUIDE" title).
        // Do not add ControlsText here — it overlapped the artwork and wrapped key letters.

        var closeBtn = GameplayUISamples.CreateButton(panel.transform, "Button_CloseGuide", "Close");
        GameplayUISamples.AnchorRect(closeBtn.gameObject, 0.45f, 0.04f, 0.65f, 0.12f);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, ctrl.Close);

        ctrl.guidePanel = panel;

        // Wire Pause Panel's Guide button to this controller
        var pausePanel = cgo.transform.Find("PausePanel");
        if (pausePanel != null)
        {
            var guideBtn = pausePanel.Find("Buttons/Button_Guide");
            if (guideBtn != null)
                UnityEventTools.AddPersistentListener(guideBtn.GetComponent<Button>().onClick, ctrl.Open);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // SCOREBOARD PANEL
    // ──────────────────────────────────────────────────────────────
    static void BuildScoreboardPanel(GameObject cgo)
    {
        var panel = MakeFullPanel(cgo.transform, "ScoreboardPanel", null);
        panel.GetComponent<Image>().color = new Color(0.04f, 0.1f, 0.22f, 0.96f);
        panel.SetActive(false);

        // Stat rows
        var rows = new (string label, string id)[] {
            ("Score",          "ScoreValue"),
            ("Thermal Stones", "ThermalValue"),
            ("Food Supplies",  "FoodValue"),
            ("Winter Clothing","ClothingValue"),
            ("Lives",          "LivesValue"),
        };

        float rowH = 0.09f, startY = 0.72f;
        for (int i = 0; i < rows.Length; i++)
        {
            float y1 = startY - (i * (rowH + 0.01f));
            float y0 = y1 - rowH;
            var rowBg = MakeAnchoredPanel(panel.transform, rows[i].id + "Row",
                         new Color(1f, 1f, 1f, 0.06f), 0.1f, y0, 0.9f, y1);
            var lbl = MakeText(rowBg.transform, "Label", rows[i].label + ":", 22, new Color(0.75f, 0.9f, 1f));
            var val = MakeText(rowBg.transform, rows[i].id, "0", 22, Color.white);
            AnchorRect(lbl.gameObject, 0.02f, 0f, 0.5f, 1f); lbl.alignment = TextAlignmentOptions.MidlineLeft;
            AnchorRect(val.gameObject, 0.5f, 0f, 0.98f, 1f); val.alignment = TextAlignmentOptions.MidlineRight;
        }

        var box = MakeVBox(panel.transform, "Buttons", 0.3f, 0.04f, 0.7f, 0.24f, 8);

        // Re-use GameOverScreen controller for scene management
        var gameOverCtrl = cgo.GetComponentInChildren<GameOverScreen>(true);
        var guideCtrlSB  = cgo.GetComponentInChildren<GuideScreen>(true);
        var playAgainBtn = GameplayUISamples.CreateButton(box.transform, "Button_PlayAgain", "Play Again");
        var menuBtn      = GameplayUISamples.CreateButton(box.transform, "Button_MainMenu",  "Main Menu");

        if (gameOverCtrl != null)
        {
            UnityEventTools.AddPersistentListener(playAgainBtn.onClick, gameOverCtrl.RetryLevel);
            UnityEventTools.AddPersistentListener(menuBtn.onClick,      gameOverCtrl.ReturnToMainMenu);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // PAUSE HUD BUTTON (top-right corner)
    // ──────────────────────────────────────────────────────────────
    static void BuildPauseHUDButton(GameObject cgo)
    {
        var ctrl = cgo.GetComponentInChildren<PauseScreen>(true);
        if (ctrl == null) return;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(GameplayUISamples.ButtonPrefabPath), cgo.transform);
        go.name = "PauseButton";
        GameplayUISamples.AnchorRect(go, 0.93f, 0.93f, 0.995f, 0.995f);
        GameplayUISamples.SetButtonLabel(go, "||");
        var btn = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, ctrl.Pause);
    }

    // ══════════════════════════════════════════════════════════════
    // STEP 4 – WIRE MAIN MENU BUTTONS
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/GameSetup/Fix Build Settings")]
    public static void FixBuildSettings()
    {
        string[] required = {
            "Assets/Scenes/Scenes/SplashScreen.unity",
            "Assets/Scenes/Scenes/MainMenu.unity",
            "Assets/Scenes/Scenes/Beginner.unity",
            "Assets/Scenes/Scenes/Advanced.unity",
            "Assets/Scenes/Scenes/Expert.unity"
        };

        var list = new List<EditorBuildSettingsScene>();
        foreach (var path in required)
        {
            if (System.IO.File.Exists(path))
                list.Add(new EditorBuildSettingsScene(path, true));
            else
                Debug.LogWarning("Scene not found, skipping: " + path);
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("[Setup] Build Settings fixed. Scenes: " + list.Count);
    }

    [MenuItem("Tools/GameSetup/Step 3b - Clean Stale MainMenu Panels")]
    public static void CleanStaleMainMenuPanels()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogWarning("No Canvas found."); return; }
        foreach (var n in new[] { "SettingsPanel", "CreditsPanel", "ConfirmQuitPanel", "GuidePanel" })
        {
            var t = canvas.transform.Find(n);
            if (t != null) { Object.DestroyImmediate(t.gameObject); Debug.Log("Removed: " + n); }
        }
        // Clear any serialized panel references on MainMenu
        var mm = Object.FindFirstObjectByType<MainMenu>();
        if (mm != null) { mm.settingsPanel = null; mm.creditsPanel = null; mm.confirmQuitPanel = null; }
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Setup] Stale panels cleaned from MainMenu.");
    }

    [MenuItem("Tools/GameSetup/Step 4 - Wire MainMenu Buttons")]
    public static void WireMainMenuButtons()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Scenes/MainMenu.unity", OpenSceneMode.Single);

        MainMenu ctrl = null;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            ctrl = go.GetComponentInChildren<MainMenu>(true);
            if (ctrl != null) break;
        }
        if (ctrl == null) { Debug.LogWarning("MainMenu script not found."); return; }

        // Wire persistent listeners on the main nav buttons only.
        // Overlay panels (Settings/Credits/Quit/Guide) are built at runtime by MainMenu.Start().
        foreach (var btn in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            btn.onClick.RemoveAllListeners();
            switch (btn.gameObject.name)
            {
                case "Button_BeginnerLevel":
                case "Button_Beginner":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.LoadBeginner); break;
                case "Button_MovingPlatformLevel":
                case "Button_Advanced":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.LoadAdvanced); break;
                case "Button_FinalLevel":
                case "Button_Expert":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.LoadExpert); break;
                case "Button_Settings":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.OpenSettings); break;
                case "Button_Credits":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.OpenCredits); break;
                case "Button_Quit":
                    UnityEventTools.AddPersistentListener(btn.onClick, ctrl.OpenConfirmQuit); break;
            }
        }

        // Assign guide background sprite so runtime BuildGuidePanel() uses it
        var guideSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Textures/GuideBackground.png");
        if (guideSprite != null)
        {
            ctrl.guideBackground = guideSprite;
            Debug.Log("[Setup] Guide background sprite assigned.");
        }
        else
            Debug.LogWarning("[Setup] GuideBackground.png not found at Assets/UI/Textures/GuideBackground.png");

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Setup] MainMenu buttons wired. Overlay panels build at runtime.");
    }

    // ══════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════

    static T GetOrAddController<T>(GameObject cgo, string name) where T : Component
    {
        var existing = cgo.GetComponentInChildren<T>(true);
        if (existing != null) return existing;
        var go = new GameObject(name);
        go.transform.SetParent(cgo.transform, false);
        return go.AddComponent<T>();
    }

    static bool HasChild(GameObject go, string childName) =>
        go.transform.Find(childName) != null;

    static GameObject MakeFullPanel(Transform parent, string name, string spritePath)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        AnchorRect(go, 0, 0, 1, 1);
        var img = go.GetComponent<Image>();
        if (!string.IsNullOrEmpty(spritePath))
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp != null) { img.sprite = sp; img.color = Color.white; }
            else img.color = new Color(0f, 0f, 0f, 0.9f);
        }
        else img.color = new Color(0f, 0f, 0f, 0.9f);
        return go;
    }

    static GameObject MakeAnchoredPanel(Transform parent, string name, Color color,
                                        float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        AnchorRect(go, x0, y0, x1, y1);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static void MakeLabel(Transform parent, string name, string text, float fs, Color color,
                           float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        AnchorRect(go, x0, y0, x1, y1);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name, string text, float fs, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static Button MakeButton(Transform parent, string name, string label, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 54);
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>();
        var c = btn.colors;
        c.highlightedColor = new Color(0.85f, 0.95f, 1f);
        c.pressedColor = new Color(0.6f, 0.75f, 0.9f);
        btn.colors = c;

        var tgo = new GameObject("Text"); tgo.transform.SetParent(go.transform, false);
        var tr = tgo.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        var t = tgo.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = 22; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.05f, 0.1f, 0.3f);
        return btn;
    }

    static GameObject MakeVBox(Transform parent, string name,
                                float x0, float y0, float x1, float y1, float spacing)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        AnchorRect(go, x0, y0, x1, y1);
        var vl = go.GetComponent<VerticalLayoutGroup>();
        vl.spacing = spacing;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childControlHeight = false;
        vl.childControlWidth = true;
        vl.childForceExpandHeight = false;
        vl.childForceExpandWidth = true;
        return go;
    }

    static Slider MakeSlider(Transform parent, string name, string label,
                              float x0, float y0, float x1, float y1)
    {
        var row = new GameObject(name + "Row");
        row.transform.SetParent(parent, false);
        AnchorRect(row, x0, y0, x1, y1);

        var lbl = new GameObject("Label");
        lbl.transform.SetParent(row.transform, false);
        AnchorRect(lbl, 0, 0, 0.35f, 1);
        var t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = 18; t.color = Color.white;
        t.alignment = TextAlignmentOptions.MidlineLeft;

        var sGO = new GameObject(name);
        sGO.transform.SetParent(row.transform, false);
        AnchorRect(sGO, 0.37f, 0.2f, 1f, 0.8f);
        var slider = sGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;

        // Background
        var bg = new GameObject("Background"); bg.transform.SetParent(sGO.transform, false);
        AnchorRect(bg, 0, 0.25f, 1, 0.75f);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Fill area
        var fillArea = new GameObject("Fill Area"); fillArea.transform.SetParent(sGO.transform, false);
        AnchorRect(fillArea, 0, 0.25f, 1, 0.75f);
        var fill = new GameObject("Fill"); fill.transform.SetParent(fillArea.transform, false);
        AnchorRect(fill, 0, 0, 1, 1);
        var fillImg = fill.AddComponent<Image>(); fillImg.color = new Color(0.4f, 0.7f, 1f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        // Handle
        var handleArea = new GameObject("Handle Slide Area"); handleArea.transform.SetParent(sGO.transform, false);
        AnchorRect(handleArea, 0, 0, 1, 1);
        var handle = new GameObject("Handle"); handle.transform.SetParent(handleArea.transform, false);
        var hRect = handle.AddComponent<RectTransform>(); hRect.sizeDelta = new Vector2(20, 20);
        var hImg = handle.AddComponent<Image>(); hImg.color = Color.white;
        slider.handleRect = hRect;
        slider.targetGraphic = hImg;

        return slider;
    }

    static void AnchorRect(GameObject go, float x0, float y0, float x1, float y1)
    {
        var r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(x0, y0);
        r.anchorMax = new Vector2(x1, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
