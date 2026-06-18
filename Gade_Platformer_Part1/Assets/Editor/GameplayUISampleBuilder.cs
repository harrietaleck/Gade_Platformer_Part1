using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameplayUISampleBuilder
{
    static readonly string[] GameScenes =
    {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity",
    };

    static readonly string[] LegacyCanvasChildren =
    {
        "PauseInfor", "MenuInfor", "Pause",
        "PausePanel", "GameOverPanel", "GuidePanel", "ScoreboardPanel", "PauseButton",
    };

    [MenuItem("Tools/GameSetup/Step 11 - Apply Unity UI Samples To Gameplay")]
    public static void ApplyUnityUISamplesToGameplay()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(GameplayUISamples.ButtonPrefabPath) == null)
        {
            Debug.LogError("[GameplayUI] Unity UI Samples prefabs not found under Assets/Unity UI Samples/Prefabs/");
            return;
        }

        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;
            ApplyToScene(scenePath);
        }

        Debug.Log("[GameplayUI] Unity UI Samples applied to all gameplay scenes.");
    }

    [MenuItem("Tools/GameSetup/Step 12 - Style Gameplay HUD Panels")]
    public static void StyleGameplayHudOnly()
    {
        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) continue;

            GameplayUISamples.RemoveOverlayTitles(canvas.transform);
            GameplayUISamples.StyleGameplayHudPanels(canvas.transform);
            GameplayUISamples.StyleDialoguePanel(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GameplayUI] HUD and dialogue styled: " + scene.name);
        }
    }

    static void ApplyToScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        CleanLegacyGameplayUI();
        GameSetupBuilder.SetupGameScene(scenePath);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameplayUI] Scene updated: " + scene.name);
    }

    static void CleanLegacyGameplayUI()
    {
        var rootPause = GameObject.Find("PauseInfor");
        if (rootPause != null && rootPause.transform.parent == null)
            Object.DestroyImmediate(rootPause);

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        foreach (var childName in LegacyCanvasChildren)
        {
            var child = canvas.transform.Find(childName);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        // Remove broken partial panels from failed runs.
        foreach (var childName in new[] { "PauseController", "GameOverController", "GuideController", "PausePanel" })
        {
            var child = canvas.transform.Find(childName);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }
    }
}
