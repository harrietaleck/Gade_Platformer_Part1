// ============================================================
// BagFixer.cs  —  Tools ▶ Fix Bag Scale and Colliders (Current Scene)
//
// Finds every bag pickup (CollectablePickup with a bag prefab mesh)
// and:
//   1. Scales it down to 0.8  (was 1.5 — too large)
//   2. Ensures it has a TRIGGER BoxCollider for OnTriggerEnter pickup
//   3. Ensures it also has a SOLID BoxCollider so the player cannot
//      walk straight through the bag model
//
// Run once per scene (Beginner, Advanced, Expert) then Ctrl+S.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BagFixer
{
    const float TargetScale   = 0.8f;

    const string CollectSFXPath =
        "Assets/Casual Game Sounds U6/CasualGameSounds/collectSound.wav";

    // Trigger box  — slightly larger, used for OnTriggerEnter pickup
    static readonly Vector3 TriggerSize   = new Vector3(1.4f, 1.4f, 1.4f);
    static readonly Vector3 TriggerCenter = new Vector3(0f,  0.7f, 0f);

    // Solid box — snug around the bag model, blocks player movement
    static readonly Vector3 SolidSize     = new Vector3(0.9f, 1.0f, 0.9f);
    static readonly Vector3 SolidCenter   = new Vector3(0f,  0.5f, 0f);

    [MenuItem("Tools/Fix Bag Scale and Colliders (Current Scene)")]
    public static void FixBags()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int fixed_count  = 0;

        // Load pickup SFX once — assign to every bag
        var collectSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(CollectSFXPath);

        foreach (var pickup in Object.FindObjectsOfType<CollectablePickup>(includeInactive: true))
        {
            var go = pickup.gameObject;

            // ── Scale ────────────────────────────────────────────────────
            go.transform.localScale = Vector3.one * TargetScale;

            // ── Colliders ────────────────────────────────────────────────
            // Remove all existing colliders first (may be wrong size/type)
            foreach (var c in go.GetComponents<Collider>())
                Object.DestroyImmediate(c);

            // Trigger — picked up via OnTriggerEnter
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size      = TriggerSize;
            trigger.center    = TriggerCenter;

            // Solid — prevents player from walking through the model
            var solid  = go.AddComponent<BoxCollider>();
            solid.isTrigger = false;
            solid.size      = SolidSize;
            solid.center    = SolidCenter;

            // ── Pickup SFX ───────────────────────────────────────────────
            if (collectSFX != null && pickup.collectSFX == null)
                pickup.collectSFX = collectSFX;

            EditorUtility.SetDirty(go);
            fixed_count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Bag Fixer",
            $"Fixed {fixed_count} bag(s) in '{sceneName}':\n" +
            $"  • Scale set to {TargetScale}\n" +
            $"  • Trigger BoxCollider  (pickup detection)\n" +
            $"  • Solid   BoxCollider  (blocks player walk-through)\n\n" +
            "Press Ctrl+S to save.",
            "OK");
    }
}
#endif
