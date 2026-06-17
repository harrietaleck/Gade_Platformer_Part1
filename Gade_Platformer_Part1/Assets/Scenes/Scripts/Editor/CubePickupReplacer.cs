// ============================================================
// CubePickupReplacer.cs  —  Tools ▶ Replace Cube Pickups (Current Scene)
//
// Finds every CollectablePickup whose root GameObject has a
// MeshRenderer directly on it (= a Unity primitive / coloured
// cube placeholder). Skips any object that is already a child
// of the BagPickups holder placed by BagPickupPlacer.
//
// Replacement mapping:
//   WinterClothing → Bag.4.prefab
//   FoodSupply     → Bag.5.prefab
//   ThermalStone   → Bag.4.prefab  (ThermalStones that are cubes)
//
// Each replacement gets a BoxCollider trigger and the same
// CollectablePickup settings (type, scoreValue, itemAmount).
// Parent, position, rotation, and scale are all preserved.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CubePickupReplacer
{
    const string Bag4Path       = "Assets/LowPoly Fantasy Bags/Prefab/Bag.4.prefab";
    const string Bag5Path       = "Assets/LowPoly Fantasy Bags/Prefab/Bag.5.prefab";
    const string CollectSFXPath = "Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav";

    [MenuItem("Tools/Replace Cube Pickups (Current Scene)")]
    public static void ReplaceCubePickups()
    {
        var bag4 = AssetDatabase.LoadAssetAtPath<GameObject>(Bag4Path);
        var bag5 = AssetDatabase.LoadAssetAtPath<GameObject>(Bag5Path);
        var collectSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(CollectSFXPath);

        if (bag4 == null || bag5 == null)
        {
            EditorUtility.DisplayDialog("Cube Pickup Replacer",
                $"Could not load bag prefabs:\n{Bag4Path}\n{Bag5Path}", "OK");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        int replaced = 0;
        int skipped  = 0;

        // Collect all CollectablePickup instances first — modifying the
        // hierarchy mid-loop would invalidate the iteration.
        var all = Object.FindObjectsOfType<CollectablePickup>(includeInactive: true);

        foreach (var pickup in all)
        {
            var go = pickup.gameObject;

            // Skip bags that BagPickupPlacer already placed
            if (go.transform.parent != null &&
                go.transform.parent.name == "BagPickups")
            {
                skipped++;
                continue;
            }

            // Only replace objects that ARE the mesh themselves (primitive cubes).
            // Proper prefab pickups have their mesh on a child (CrystalVisual etc.)
            if (go.GetComponent<MeshRenderer>() == null)
            {
                skipped++;
                continue;
            }

            // Decide which bag to use
            GameObject bagPrefab = pickup.collectableType == CollectableType.FoodSupply
                ? bag5 : bag4;

            // Capture data before destruction
            var pos    = go.transform.position;
            var rot    = go.transform.rotation;
            var parent = go.transform.parent;
            var type   = pickup.collectableType;
            var score  = pickup.scoreValue;
            var amount = pickup.itemAmount;
            var lives  = pickup.livesToRestore;

            // Destroy the old cube
            Undo.DestroyObjectImmediate(go);

            // Instantiate replacement
            var newGo = (GameObject)PrefabUtility.InstantiatePrefab(bagPrefab, parent);
            Undo.RegisterCreatedObjectUndo(newGo, "Replace Cube Pickup");
            newGo.transform.position = pos;
            newGo.transform.rotation = rot;
            newGo.transform.localScale = Vector3.one * 1.5f;

            // Give the replacement the same pickup settings
            var newPickup = newGo.AddComponent<CollectablePickup>();
            newPickup.collectableType = type;
            newPickup.scoreValue      = score;
            newPickup.itemAmount      = amount;
            newPickup.livesToRestore  = lives;
            newPickup.collectSFX      = collectSFX;   // ← pickup sound

            // Replace concave MeshCollider with BoxCollider trigger
            foreach (var c in newGo.GetComponents<Collider>())
                Object.DestroyImmediate(c);

            var col = newGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size      = new Vector3(1.2f, 1.2f, 1.2f);
            col.center    = new Vector3(0f, 0.6f, 0f);

            EditorUtility.SetDirty(newGo);
            replaced++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Cube Pickup Replacer",
            $"Done in '{sceneName}'.\n" +
            $"  • Replaced: {replaced} cube pickups\n" +
            $"  • Skipped:  {skipped} (already bags or mesh-on-child)\n\n" +
            "Press Ctrl+S to save.",
            "OK");
    }
}
#endif
