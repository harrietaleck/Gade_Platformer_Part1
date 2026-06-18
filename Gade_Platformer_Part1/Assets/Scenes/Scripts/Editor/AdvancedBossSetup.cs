// ============================================================
// AdvancedBossSetup.cs  —  Tools ▶ Setup Advanced Boss (Wolfboss_A)
//
// Replaces the red-cube "boss" placeholder in the Advanced scene
// with the Wolfboss_A.prefab. Preserves the boss position and
// wires up all required components:
//   • NavMeshAgent   (already in prefab)
//   • BossAIagent    startNode = "PathA"
//   • AiAttack       hitRadius = 3, cooldown = 2
//   • WolfBossAnimDriver
//
// BossAIagent uses the static GraphSetup.Graphs singleton so it
// automatically finds the BossGraphSet object in the scene.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class AdvancedBossSetup
{
    const string WolfbossPath =
        "Assets/Hatogame_new/BossMonsterPack1/Wolfboss/Perfabs/Wolfboss_A.prefab";

    [MenuItem("Tools/Setup Advanced Boss (Wolfboss_A)")]
    public static void SetupBoss()
    {
        if (SceneManager.GetActiveScene().name != "Advanced")
        {
            EditorUtility.DisplayDialog("Advanced Boss Setup",
                "Please open the Advanced scene first.", "OK");
            return;
        }

        var wolfbossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WolfbossPath);
        if (wolfbossPrefab == null)
        {
            EditorUtility.DisplayDialog("Advanced Boss Setup",
                $"Could not find prefab at:\n{WolfbossPath}", "OK");
            return;
        }

        // ── Find and record the old boss ──────────────────────────────────
        var oldBoss = GameObject.Find("boss");
        Vector3 bossPos = oldBoss != null
            ? oldBoss.transform.position
            : new Vector3(-8.79f, 6.7f, -12.46f); // fallback to known position

        if (oldBoss != null)
        {
            Undo.DestroyObjectImmediate(oldBoss);
            Debug.Log("[AdvancedBossSetup] Old 'boss' cube destroyed.");
        }

        // Also remove any previous WolfBoss if re-running
        var prev = GameObject.Find("WolfBoss");
        if (prev != null) Undo.DestroyObjectImmediate(prev);

        // ── Instantiate Wolfboss_A ────────────────────────────────────────
        var go = (GameObject)PrefabUtility.InstantiatePrefab(wolfbossPrefab);
        Undo.RegisterCreatedObjectUndo(go, "Setup Advanced Boss");
        go.name = "WolfBoss";
        go.transform.position = bossPos;
        go.transform.rotation = Quaternion.identity;

        // ── NavMeshAgent ─────────────────────────────────────────────────
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent == null) agent = go.AddComponent<NavMeshAgent>();
        agent.speed        = 5f;
        agent.angularSpeed = 180f;
        agent.stoppingDistance = 1.5f;
        agent.radius       = 0.5f;
        agent.height       = 2f;
        agent.autoTraverseOffMeshLink = true;

        // ── BossAIagent ───────────────────────────────────────────────────
        var bossAI = go.GetComponent<BossAIagent>();
        if (bossAI == null) bossAI = go.AddComponent<BossAIagent>();
        bossAI.startNode = "PathA";

        // ── AiAttack ──────────────────────────────────────────────────────
        var aiAttack = go.GetComponent<AiAttack>();
        if (aiAttack == null) aiAttack = go.AddComponent<AiAttack>();
        // Set fields via SerializedObject so Unity records the change properly
        var so = new SerializedObject(aiAttack);
        var hitProp  = so.FindProperty("hitRadius");
        var cdProp   = so.FindProperty("attackCooldown");
        if (hitProp != null) hitProp.floatValue = 3f;
        if (cdProp  != null) cdProp.floatValue  = 2f;
        so.ApplyModifiedProperties();

        // ── WolfBossAnimDriver ────────────────────────────────────────────
        if (go.GetComponent<WolfBossAnimDriver>() == null)
            go.AddComponent<WolfBossAnimDriver>();

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Advanced Boss Setup",
            $"WolfBoss placed at {bossPos}.\n\n" +
            "Components:\n" +
            "  ✓ NavMeshAgent\n" +
            "  ✓ BossAIagent  (startNode = PathA)\n" +
            "  ✓ AiAttack     (hitRadius = 3, cooldown = 2)\n" +
            "  ✓ WolfBossAnimDriver\n\n" +
            "BossAIagent uses GraphSetup.Graphs static singleton —\n" +
            "BossGraphSet in the scene provides the graph automatically.\n\n" +
            "Press Ctrl+S to save.",
            "OK");
    }
}
#endif
