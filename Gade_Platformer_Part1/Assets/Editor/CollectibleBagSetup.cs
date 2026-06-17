using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CollectibleBagSetup
{
    const string VisualChildName = "BagVisual";

    static readonly (string prefabPath, string[] preferredBagNames)[] CollectibleTargets =
    {
        ("Assets/Prefabs/Collectibles/WinterClothing.prefab", new[] { "bag_03", "bag03", "bag_3", "bag3", "bag_01", "bag01" }),
        ("Assets/Prefabs/Collectibles/FoodSupply.prefab",     new[] { "bag_05", "bag05", "bag_5", "bag5", "bag_02", "bag02" }),
    };

    static readonly string[] GameScenes =
    {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity",
    };

    [MenuItem("Tools/GameSetup/Step 9 - Apply Fantasy Bag Collectible Models")]
    public static void ApplyFantasyBagCollectibleModels()
    {
        var bagPrefabs = FindFantasyBagPrefabs();
        if (bagPrefabs.Count == 0)
        {
            Debug.LogError("[Collectibles] Lowpoly Fantasy Bags prefabs not found. Import the package into Assets first, then run this step again.");
            return;
        }

        Debug.Log($"[Collectibles] Found {bagPrefabs.Count} bag prefab(s): {string.Join(", ", bagPrefabs.Select(p => p.name))}");

        foreach (var target in CollectibleTargets)
            ApplyBagToCollectiblePrefab(target.prefabPath, bagPrefabs, target.preferredBagNames);

        int sceneCount = UpdateSceneCollectibles();
        AssetDatabase.SaveAssets();
        Debug.Log($"[Collectibles] Fantasy bag visuals applied to prefabs and {sceneCount} scene pickup(s).");
    }

    static List<GameObject> FindFantasyBagPrefabs()
    {
        var results = new List<GameObject>();
        var seen = new HashSet<string>();

        foreach (var folder in FindFantasyBagFolders())
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                TryAddBagPrefab(guid, results, seen);
        }

        foreach (var guid in AssetDatabase.FindAssets("bag t:Prefab", new[] { "Assets" }))
            TryAddBagPrefab(guid, results, seen);

        results.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return results;
    }

    static IEnumerable<string> FindFantasyBagFolders()
    {
        var folders = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:Folder", new[] { "Assets" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileName(path).ToLowerInvariant();
            if (name.Contains("fantasy") && name.Contains("bag"))
                folders.Add(path);
        }

        if (folders.Count == 0)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Folder", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path).Equals("VVergis", System.StringComparison.OrdinalIgnoreCase))
                    folders.Add(path);
            }
        }

        return folders;
    }

    static void TryAddBagPrefab(string guid, List<GameObject> results, HashSet<string> seen)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!IsFantasyBagAssetPath(path)) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null || !seen.Add(path)) return;
        results.Add(prefab);
    }

    static bool IsFantasyBagAssetPath(string path)
    {
        var lower = path.ToLowerInvariant();
        if (lower.Contains("lowpoly fantasy bags") || lower.Contains("lowpoly_fantasy_bags"))
            return true;

        if (!lower.Contains("bag") && !lower.Contains("backpack")) return false;

        // Ignore unrelated bag assets from other packs.
        if (lower.Contains("backpack") && !lower.Contains("fantasy")) return false;
        if (lower.Contains("boss") || lower.Contains("player") || lower.Contains("wolf")) return false;

        return lower.Contains("fantasy") || lower.Contains("vergis") || lower.Contains("lowpoly");
    }

    static void ApplyBagToCollectiblePrefab(string collectiblePath, List<GameObject> bagPrefabs, string[] preferredNames)
    {
        var root = PrefabUtility.LoadPrefabContents(collectiblePath);
        if (root == null)
        {
            Debug.LogWarning("[Collectibles] Prefab not found: " + collectiblePath);
            return;
        }

        var bagPrefab = PickBagPrefab(bagPrefabs, preferredNames);
        if (bagPrefab == null)
        {
            PrefabUtility.UnloadPrefabContents(root);
            Debug.LogWarning("[Collectibles] No suitable bag prefab for: " + collectiblePath);
            return;
        }

        ApplyBagVisual(root, bagPrefab);
        PrefabUtility.SaveAsPrefabAsset(root, collectiblePath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"[Collectibles] Applied '{bagPrefab.name}' to {Path.GetFileNameWithoutExtension(collectiblePath)}");
    }

    static GameObject PickBagPrefab(List<GameObject> bagPrefabs, string[] preferredNames)
    {
        foreach (var preferred in preferredNames)
        {
            var match = bagPrefabs.FirstOrDefault(p => NormalizeName(p.name) == NormalizeName(preferred));
            if (match != null) return match;
        }

        foreach (var preferred in preferredNames)
        {
            var match = bagPrefabs.FirstOrDefault(p => NormalizeName(p.name).Contains(NormalizeName(preferred)));
            if (match != null) return match;
        }

        return bagPrefabs.FirstOrDefault();
    }

    static string NormalizeName(string name) => name.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");

    static void ApplyBagVisual(GameObject collectibleRoot, GameObject bagPrefab)
    {
        RemovePlaceholderRenderer(collectibleRoot);

        var existing = collectibleRoot.transform.Find(VisualChildName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject, true);

        var bag = (GameObject)PrefabUtility.InstantiatePrefab(bagPrefab, collectibleRoot.transform);
        bag.name = VisualChildName;
        bag.transform.localPosition = Vector3.zero;
        bag.transform.localRotation = Quaternion.identity;
        bag.transform.localScale = Vector3.one * GetBagScale(collectibleRoot.name);

        foreach (var col in bag.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col, true);

        FitTriggerCollider(collectibleRoot);
    }

    static void RemovePlaceholderRenderer(GameObject root)
    {
        var meshFilter = root.GetComponent<MeshFilter>();
        var meshRenderer = root.GetComponent<MeshRenderer>();
        if (meshFilter != null) Object.DestroyImmediate(meshFilter, true);
        if (meshRenderer != null) Object.DestroyImmediate(meshRenderer, true);
    }

    static float GetBagScale(string collectibleName)
    {
        if (collectibleName.Contains("Thermal")) return 0.45f;
        if (collectibleName.Contains("Food")) return 0.55f;
        return 0.6f;
    }

    static void FitTriggerCollider(GameObject root)
    {
        var bounds = CalculateBounds(root);
        if (bounds.size.sqrMagnitude < 0.0001f) return;

        var center = root.transform.InverseTransformPoint(bounds.center);
        var size = bounds.size;

        var box = root.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.center = center;
            box.size = size;
            return;
        }

        var sphere = root.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.center = center;
            sphere.radius = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 0.5f;
        }
    }

    static Bounds CalculateBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one * 0.5f);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static int UpdateSceneCollectibles()
    {
        int updated = 0;

        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            foreach (var pickup in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (pickup is not CollectablePickup and not ThermalStonePickup) continue;
                if (!PrefabUtility.IsPartOfPrefabInstance(pickup)) continue;

                PrefabUtility.RevertPrefabInstance(pickup.gameObject, InteractionMode.AutomatedAction);
                updated++;
                sceneDirty = true;
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        return updated;
    }
}
