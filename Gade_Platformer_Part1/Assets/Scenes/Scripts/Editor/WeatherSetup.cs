// ============================================================
// WeatherSetup.cs  —  Editor utility: Tools ▶ Setup Weather Effects
//
// Instantiates rain, snow, and fog VFX prefabs from the
// MyWeeklyGoal/May2026/RainSnowCloudEffect package into the
// currently open scene. Each effect gets a WeatherFollowCamera
// component so it tracks the player camera at runtime.
//
// Run once per scene (Beginner, Advanced, Expert), then Ctrl+S.
// Re-running removes the old Weather holder and places a fresh one.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WeatherSetup
{
    // ── Asset paths ──────────────────────────────────────────────────────────
    const string RainPath = "Assets/MyWeeklyGoal/May2026/RainSnowCloudEffect/Prefabs/PF_RainVFX_Group.prefab";
    const string SnowPath = "Assets/MyWeeklyGoal/May2026/RainSnowCloudEffect/Prefabs/PF_Snow_VFX_Group.prefab";
    const string FogPath  = "Assets/MyWeeklyGoal/May2026/RainSnowCloudEffect/Prefabs/PF_Fog_VFX_Group.prefab";

    // ── Spawn heights ─────────────────────────────────────────────────────────
    // Rain/snow emit downward from above; fog sits near ground level.
    const float RainHeight = 35f;
    const float SnowHeight = 30f;
    const float FogHeight  =  3f;

    [MenuItem("Tools/Setup Weather Effects (Current Scene)")]
    public static void SetupWeather()
    {
        var rainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RainPath);
        var snowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SnowPath);
        var fogPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(FogPath);

        bool anyMissing = rainPrefab == null || snowPrefab == null || fogPrefab == null;
        if (anyMissing)
        {
            EditorUtility.DisplayDialog("Weather Setup",
                "One or more weather prefabs could not be found.\n\n" +
                $"Rain: {(rainPrefab == null ? "MISSING" : "OK")}\n" +
                $"Snow: {(snowPrefab == null ? "MISSING" : "OK")}\n" +
                $"Fog:  {(fogPrefab  == null ? "MISSING" : "OK")}\n\n" +
                "Check the MyWeeklyGoal/May2026/RainSnowCloudEffect/Prefabs/ folder.",
                "OK");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // Remove any existing Weather holder so re-running is safe
        var existing = GameObject.Find("Weather");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        // Root holder
        var holder = new GameObject("Weather");
        Undo.RegisterCreatedObjectUndo(holder, "Setup Weather Effects");

        // Spawn each effect
        SpawnEffect(rainPrefab, "Rain",  new Vector3(0f, RainHeight, 0f), holder.transform);
        SpawnEffect(snowPrefab, "Snow",  new Vector3(0f, SnowHeight, 0f), holder.transform);
        SpawnEffect(fogPrefab,  "Fog",   new Vector3(0f, FogHeight,  0f), holder.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Weather Setup",
            $"Weather effects added to '{sceneName}':\n" +
            $"  • Rain  (height {RainHeight})\n" +
            $"  • Snow  (height {SnowHeight})\n" +
            $"  • Fog   (height {FogHeight})\n\n" +
            "Each effect follows the camera via WeatherFollowCamera.\n" +
            "Press Ctrl+S to save.",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────

    static void SpawnEffect(GameObject prefab, string goName, Vector3 position, Transform parent)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = goName;
        go.transform.position = position;

        // Add the follow-camera script so weather tracks the player at runtime
        if (go.GetComponent<WeatherFollowCamera>() == null)
            go.AddComponent<WeatherFollowCamera>();

        // Make sure all child particle systems loop and play on awake
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.loop       = true;
            main.playOnAwake = true;
        }

        EditorUtility.SetDirty(go);
    }
}
#endif
