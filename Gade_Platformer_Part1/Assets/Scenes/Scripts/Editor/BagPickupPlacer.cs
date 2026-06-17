// ============================================================
// BagPickupPlacer.cs  —  Editor utility: Tools ▶ Place Bag Pickups
//
// Places Bag.2 (WinterClothing) and Bag.3 (FoodSupply) pickups
// across the currently open scene. Each bag gets:
//   • CollectablePickup  (correct enum type)
//   • BoxCollider        (isTrigger = true, sized for pickup)
//   • Tag = "Collectable"
//
// Placement positions are chosen to be clearly visible and
// reachable on each level's platforms. Run once per scene,
// then Ctrl+S to save.
//
// Bag.2.prefab  → WinterClothing  (blue-ish bag)
// Bag.3.prefab  → FoodSupply      (brown bag)
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BagPickupPlacer
{
    const string Bag2Path       = "Assets/LowPoly Fantasy Bags/Prefab/Bag.2.prefab";
    const string Bag3Path       = "Assets/LowPoly Fantasy Bags/Prefab/Bag.3.prefab";
    const string CollectSFXPath = "Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav";

    // ── Per-scene spawn positions ──────────────────────────────────────────
    // Each entry: world position to place a bag in a given scene.
    // Two WinterClothing (Bag.2) and two FoodSupply (Bag.3) per scene.

    static readonly Vector3[] BeginnerBag2Positions = {
        new Vector3(  5f, 2f, -10f),
        new Vector3(-10f, 2f, -25f),
    };
    static readonly Vector3[] BeginnerBag3Positions = {
        new Vector3( 15f, 2f, -15f),
        new Vector3( -5f, 2f, -30f),
    };

    static readonly Vector3[] AdvancedBag2Positions = {
        new Vector3(  8f, 4f,  -15f),
        new Vector3(-12f, 4f,  -40f),
    };
    static readonly Vector3[] AdvancedBag3Positions = {
        new Vector3( 12f, 4f,  -25f),
        new Vector3( -8f, 4f,  -50f),
    };

    static readonly Vector3[] ExpertBag2Positions = {
        new Vector3( 10f, 3f, -80f),
        new Vector3(-15f, 3f,-120f),
    };
    static readonly Vector3[] ExpertBag3Positions = {
        new Vector3(-10f, 3f, -90f),
        new Vector3( 15f, 3f,-130f),
    };

    // ─────────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Place Bag Pickups (Current Scene)")]
    public static void PlaceBagPickups()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        var bag2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Bag2Path);
        var bag3Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Bag3Path);

        if (bag2Prefab == null || bag3Prefab == null)
        {
            EditorUtility.DisplayDialog("Bag Pickup Placer",
                $"Could not load bag prefabs.\nExpected:\n{Bag2Path}\n{Bag3Path}", "OK");
            return;
        }

        Vector3[] bag2Positions, bag3Positions;
        switch (sceneName)
        {
            case "Beginner":
                bag2Positions = BeginnerBag2Positions;
                bag3Positions = BeginnerBag3Positions;
                break;
            case "Advanced":
                bag2Positions = AdvancedBag2Positions;
                bag3Positions = AdvancedBag3Positions;
                break;
            case "Expert":
                bag2Positions = ExpertBag2Positions;
                bag3Positions = ExpertBag3Positions;
                break;
            default:
                EditorUtility.DisplayDialog("Bag Pickup Placer",
                    $"Scene '{sceneName}' has no defined bag positions.\nAdd positions for this scene in BagPickupPlacer.cs.",
                    "OK");
                return;
        }

        // Create a parent holder to keep the hierarchy tidy.
        var holder = new GameObject("BagPickups");
        Undo.RegisterCreatedObjectUndo(holder, "Place Bag Pickups");

        int count = 0;
        var collectSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(CollectSFXPath);

        // ── WinterClothing bags (Bag.2) ───────────────────────────────
        foreach (var pos in bag2Positions)
        {
            SpawnBag(bag2Prefab, pos, CollectableType.WinterClothing,
                     $"Bag_WinterClothing_{count}", holder.transform, collectSFX);
            count++;
        }

        // ── FoodSupply bags (Bag.3) ────────────────────────────────────
        foreach (var pos in bag3Positions)
        {
            SpawnBag(bag3Prefab, pos, CollectableType.FoodSupply,
                     $"Bag_FoodSupply_{count}", holder.transform, collectSFX);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Bag Pickup Placer",
            $"Placed {count} bag pickups in '{sceneName}'.\nPress Ctrl+S to save the scene.",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────

    static void SpawnBag(GameObject prefab, Vector3 position,
                          CollectableType type, string goName, Transform parent,
                          AudioClip collectSFX = null)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = goName;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 1.5f; // slightly larger for visibility

        // CollectablePickup — handles OnTriggerEnter with Player.
        var pickup = go.AddComponent<CollectablePickup>();
        pickup.collectableType = type;
        pickup.scoreValue  = 15;
        pickup.itemAmount  = 1;
        pickup.collectSFX  = collectSFX;   // ← pickup sound for every bag type

        // BoxCollider trigger — required for OnTriggerEnter to fire.
        // The LowPoly bag prefabs have concave MeshColliders, which Unity
        // does NOT allow as triggers. We remove any existing Collider(s)
        // and add a simple BoxCollider with isTrigger = true instead.
        // The bag is a pure pickup — it does not need to block movement.
        foreach (var c in go.GetComponents<Collider>())
            Object.DestroyImmediate(c);

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(1.2f, 1.2f, 1.2f);
        col.center    = new Vector3(0f, 0.6f, 0f);

        EditorUtility.SetDirty(go);
    }
}
#endif
