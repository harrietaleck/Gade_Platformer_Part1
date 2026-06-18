using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CollectibleCrystalSetup
{
    const string CrystalPrefabPath = "Assets/RPG CRYSTALS/URP/Prefab URP/Crystal_1.prefab";
    const string ThermalStonePrefabPath = "Assets/Prefabs/Collectibles/ThermalStone.prefab";
    const string VisualChildName = "CrystalVisual";
    const float CrystalVisualScale = 1.15f;

    static readonly string[] GameScenes =
    {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity",
    };

    [MenuItem("Tools/GameSetup/Step 10 - Apply RPG Crystal Thermal Stones")]
    public static void ApplyRpgCrystalThermalStones()
    {
        var crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CrystalPrefabPath);
        if (crystalPrefab == null)
        {
            Debug.LogError("[Collectibles] RPG crystal prefab not found at: " + CrystalPrefabPath);
            return;
        }

        ApplyCrystalToThermalStonePrefab(crystalPrefab);

        int sceneCount = UpdateSceneThermalStones(crystalPrefab);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Collectibles] Applied '{crystalPrefab.name}' to thermal stone prefab and {sceneCount} scene pickup(s).");
    }

    static void ApplyCrystalToThermalStonePrefab(GameObject crystalPrefab)
    {
        var root = PrefabUtility.LoadPrefabContents(ThermalStonePrefabPath);
        if (root == null)
        {
            Debug.LogError("[Collectibles] ThermalStone prefab not found.");
            return;
        }

        ApplyCrystalVisual(root, crystalPrefab);
        root.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(root, ThermalStonePrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[Collectibles] Updated ThermalStone.prefab with RPG crystal visual.");
    }

    static int UpdateSceneThermalStones(GameObject crystalPrefab)
    {
        int updated = 0;

        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            foreach (var root in scene.GetRootGameObjects())
                updated += UpdateThermalStonesRecursive(root.transform, crystalPrefab, ref sceneDirty);

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        return updated;
    }

    static int UpdateThermalStonesRecursive(Transform t, GameObject crystalPrefab, ref bool sceneDirty)
    {
        int updated = 0;

        if (IsThermalStonePickup(t.gameObject))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject))
            {
                PrefabUtility.RevertPrefabInstance(t.gameObject, InteractionMode.AutomatedAction);
                updated++;
                sceneDirty = true;
            }
            else if (ApplyCrystalVisual(t.gameObject, crystalPrefab))
            {
                updated++;
                sceneDirty = true;
            }
        }

        for (int i = 0; i < t.childCount; i++)
            updated += UpdateThermalStonesRecursive(t.GetChild(i), crystalPrefab, ref sceneDirty);

        return updated;
    }

    static bool IsThermalStonePickup(GameObject go)
    {
        if (go.GetComponent<ThermalStonePickup>() != null)
            return true;

        var generic = go.GetComponent<CollectablePickup>();
        return generic != null && generic.collectableType == CollectableType.ThermalStone;
    }

    static bool ApplyCrystalVisual(GameObject pickupRoot, GameObject crystalPrefab)
    {
        RemovePlaceholderRenderer(pickupRoot);

        var existing = pickupRoot.transform.Find(VisualChildName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject, true);

        var crystal = (GameObject)PrefabUtility.InstantiatePrefab(crystalPrefab, pickupRoot.transform);
        crystal.name = VisualChildName;
        crystal.transform.localPosition = Vector3.zero;
        crystal.transform.localRotation = Quaternion.identity;
        crystal.transform.localScale = Vector3.one * CrystalVisualScale;

        foreach (var col in crystal.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col, true);

        pickupRoot.transform.localScale = Vector3.one;
        FitTriggerCollider(pickupRoot);
        return true;
    }

    static void RemovePlaceholderRenderer(GameObject root)
    {
        var meshFilter = root.GetComponent<MeshFilter>();
        var meshRenderer = root.GetComponent<MeshRenderer>();
        if (meshFilter != null) Object.DestroyImmediate(meshFilter, true);
        if (meshRenderer != null) Object.DestroyImmediate(meshRenderer, true);
    }

    static void FitTriggerCollider(GameObject root)
    {
        var bounds = CalculateBounds(root);
        if (bounds.size.sqrMagnitude < 0.0001f) return;

        var center = root.transform.InverseTransformPoint(bounds.center);
        var size = bounds.size;

        var sphere = root.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.center = center;
            sphere.radius = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 0.5f;
            return;
        }

        var box = root.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.center = center;
            box.size = size;
        }
    }

    static Bounds CalculateBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(root.transform.position, Vector3.one * 0.5f);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }
}
