#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Dresses the open (or all) levels with LowPoly Winter Pack scenery:
// snow pines along the path, ice mountains as backdrop, a cabin, light poles.
public static class EnvironmentBeautifyTool
{
    const string Tree1 = "Assets/FinottiGames/LowPoly_WinterPack/Prefabs/Builds-Snow/PinesTree_01_Snow.prefab";
    const string Tree2 = "Assets/FinottiGames/LowPoly_WinterPack/Prefabs/Builds-Snow/PinesTree_02_Snow.prefab";
    const string Tree3 = "Assets/FinottiGames/LowPoly_WinterPack/Prefabs/Builds-Snow/PinesTree_03_Snow.prefab";
    const string Mountain1 = "Assets/FinottiGames/LowPoly_WinterPack/Environment/IceMountain01.prefab";
    const string Mountain2 = "Assets/FinottiGames/LowPoly_WinterPack/Environment/IceMountain02.prefab";
    const string Cabin = "Assets/FinottiGames/LowPoly_WinterPack/Prefabs/Builds-Snow/LogHouse_01_Snow.prefab";
    const string Pole = "Assets/FinottiGames/LowPoly_WinterPack/Prefabs/Builds-Snow/LightPole_Snow.prefab";

    static readonly string[] LevelPaths =
    {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity",
    };

    [MenuItem("Tools/Beautify Environment (All Levels)")]
    public static void BeautifyAll()
    {
        string current = SceneManager.GetActiveScene().path;
        var report = new System.Text.StringBuilder();
        foreach (var path in LevelPaths)
            report.AppendLine(BeautifyScene(path));

        if (!string.IsNullOrEmpty(current))
            EditorSceneManager.OpenScene(current);

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("Environment Beautify", report.ToString(), "OK");
    }

    [MenuItem("Tools/Beautify Environment (Current Scene)")]
    public static void BeautifyCurrent()
    {
        string path = SceneManager.GetActiveScene().path;
        string report = BeautifyScene(path);
        Debug.Log(report);
        EditorUtility.DisplayDialog("Environment Beautify", report, "OK");
    }

    static string BeautifyScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Scene: " + scene.name);

        // Remove previous auto-scenery so re-running is safe
        var existing = GameObject.Find("AutoWinterScenery");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var root = new GameObject("AutoWinterScenery");
        Undo.RegisterCreatedObjectUndo(root, "Beautify Environment");

        // Gather platform / player bounds for placement
        float minZ = float.MaxValue, maxZ = float.MinValue;
        float midX = 0f;
        int platformCount = 0;
        foreach (var t in Object.FindObjectsOfType<Transform>())
        {
            string n = t.name;
            if (!n.StartsWith("Platform") && n != "Player" && !n.Contains("Igloo") &&
                !n.Contains("Checkpoint"))
                continue;
            if (t.parent != null && t.parent.name != scene.name &&
                t.parent.GetComponentInParent<Canvas>() != null)
                continue;

            // Only root-ish gameplay objects
            if (t.parent != null && t.parent.parent != null &&
                t.parent.name != "Igloo")
            {
                // allow Igloo children skipped; use Igloo root
                if (n.Contains("Igloo") && t.parent.name == "Igloo") continue;
            }

            bool isRootGameplay =
                t.parent == null ||
                n.StartsWith("Platform") ||
                n == "Player" ||
                n == "Igloo" ||
                n.StartsWith("Checkpoint");

            if (!isRootGameplay) continue;

            midX += t.position.x;
            platformCount++;
            minZ = Mathf.Min(minZ, t.position.z);
            maxZ = Mathf.Max(maxZ, t.position.z);
        }

        if (platformCount == 0)
        {
            midX = 0f;
            minZ = -40f;
            maxZ = 40f;
        }
        else
        {
            midX /= platformCount;
        }

        float length = Mathf.Max(40f, maxZ - minZ);
        sb.AppendLine($"  Path span Z [{minZ:F1} .. {maxZ:F1}] midX={midX:F1}");

        var trees = new[] { Tree1, Tree2, Tree3 };
        int treeCount = 0;

        // Left and right tree lines along the level
        for (int side = -1; side <= 1; side += 2)
        {
            float baseX = midX + side * 22f;
            int steps = Mathf.Clamp(Mathf.RoundToInt(length / 8f), 8, 22);
            for (int i = 0; i < steps; i++)
            {
                float t = steps == 1 ? 0.5f : i / (float)(steps - 1);
                float z = Mathf.Lerp(minZ - 8f, maxZ + 12f, t);
                float x = baseX + side * Random.Range(0f, 10f) + Random.Range(-3f, 3f);
                float y = Random.Range(-0.5f, 0.5f);
                float scale = Random.Range(1.6f, 3.2f);
                float yaw = Random.Range(0f, 360f);

                string prefabPath = trees[i % trees.Length];
                var go = Place(prefabPath, root.transform,
                    new Vector3(x, y, z),
                    Quaternion.Euler(0f, yaw, 0f),
                    Vector3.one * scale);
                if (go != null) treeCount++;

                // Extra denser row further out
                if (i % 2 == 0)
                {
                    float x2 = baseX + side * Random.Range(14f, 24f);
                    float s2 = Random.Range(2.2f, 4.0f);
                    var go2 = Place(trees[(i + 1) % trees.Length], root.transform,
                        new Vector3(x2, y - 0.2f, z + Random.Range(-2f, 2f)),
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                        Vector3.one * s2);
                    if (go2 != null) treeCount++;
                }
            }
        }
        sb.AppendLine("  Trees placed: " + treeCount);

        // Ice mountains as distant backdrop (left, right, ahead)
        int mountains = 0;
        mountains += Place(Mountain1, root.transform,
            new Vector3(midX - 70f, -8f, (minZ + maxZ) * 0.5f),
            Quaternion.Euler(0f, 25f, 0f), Vector3.one * 2.5f) != null ? 1 : 0;
        mountains += Place(Mountain2, root.transform,
            new Vector3(midX + 75f, -10f, minZ + length * 0.3f),
            Quaternion.Euler(0f, -40f, 0f), Vector3.one * 2.8f) != null ? 1 : 0;
        mountains += Place(Mountain1, root.transform,
            new Vector3(midX + 20f, -12f, maxZ + 55f),
            Quaternion.Euler(0f, 180f, 0f), Vector3.one * 3.2f) != null ? 1 : 0;
        mountains += Place(Mountain2, root.transform,
            new Vector3(midX - 30f, -14f, minZ - 45f),
            Quaternion.Euler(0f, 10f, 0f), Vector3.one * 2.4f) != null ? 1 : 0;
        sb.AppendLine("  Mountains placed: " + mountains);

        // Cabin near the end of the path
        var cabin = Place(Cabin, root.transform,
            new Vector3(midX + 14f, 0f, maxZ + 6f),
            Quaternion.Euler(0f, -35f, 0f), Vector3.one * 1.4f);
        sb.AppendLine("  Cabin: " + (cabin != null ? "OK" : "FAILED"));

        // Light poles along the route
        int poles = 0;
        int poleSteps = 5;
        for (int i = 0; i < poleSteps; i++)
        {
            float t = i / (float)(poleSteps - 1);
            float z = Mathf.Lerp(minZ + 5f, maxZ - 2f, t);
            float x = midX + ((i % 2 == 0) ? -8f : 8f);
            var p = Place(Pole, root.transform,
                new Vector3(x, 0f, z),
                Quaternion.identity, Vector3.one * 1.2f);
            if (p != null) poles++;
        }
        sb.AppendLine("  Light poles: " + poles);

        // Cooler winter lighting
        var light = Object.FindObjectOfType<Light>();
        if (light != null && light.type == LightType.Directional)
        {
            light.color = new Color(0.78f, 0.88f, 1f);
            light.intensity = Mathf.Clamp(light.intensity, 0.9f, 1.35f);
            light.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            EditorUtility.SetDirty(light);
            sb.AppendLine("  Lighting: winter cool tone");
        }

        // Soft ambient if RenderSettings available
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.68f, 0.85f);
        RenderSettings.ambientEquatorColor = new Color(0.45f, 0.52f, 0.58f);
        RenderSettings.ambientGroundColor = new Color(0.35f, 0.38f, 0.42f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.72f, 0.82f, 0.92f);
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 160f;
        sb.AppendLine("  Fog + ambient: winter haze");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("  Saved.");
        return sb.ToString();
    }

    static GameObject Place(string prefabPath, Transform parent,
                            Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Missing prefab: " + prefabPath);
            return null;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.rotation = rot;
        go.transform.localScale = scale;

        // Strip colliders so scenery never blocks the player path
        foreach (var col in go.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col);

        return go;
    }
}
#endif
