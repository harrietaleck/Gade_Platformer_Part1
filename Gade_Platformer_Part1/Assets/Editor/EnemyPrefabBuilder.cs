using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyPrefabBuilder
{
    const string WolfPrefabPath = "Assets/Wolf_Animated/Prefabs/Wolf.prefab";
    const string WolfMaterialPath = "Assets/Materials/WolfEnemyMat.mat";
    const string EnemyPrefabFolder = "Assets/Prefabs/Enemies";

    static readonly string[] GameScenes = {
        "Assets/Scenes/Scenes/Beginner.unity",
        "Assets/Scenes/Scenes/Advanced.unity",
        "Assets/Scenes/Scenes/Expert.unity"
    };

    [MenuItem("Tools/GameSetup/Step 6 - Build Wolf Enemy Prefabs")]
    public static void BuildWolfEnemyPrefabs()
    {
        EnsureFolder(EnemyPrefabFolder);

        var fast   = BuildEnemyPrefab<FastEnemy>("WolfEnemy_Fast",   patrolSpeed: 8f);
        var normal = BuildEnemyPrefab<NormEnemy>("WolfEnemy_Normal", patrolSpeed: 5f);
        var heavy  = BuildEnemyPrefab<HeavyEnemy>("WolfEnemy_Heavy", patrolSpeed: 0f, addPatrol: false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EnemySetup] Wolf enemy prefabs created in " + EnemyPrefabFolder);
    }

    [MenuItem("Tools/GameSetup/Step 7 - Replace Capsule Enemies With Wolf")]
    public static void ReplaceCapsuleEnemiesWithWolf()
    {
        var wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WolfPrefabPath);
        if (wolfPrefab == null)
        {
            Debug.LogError("[EnemySetup] Wolf prefab not found at " + WolfPrefabPath);
            return;
        }

        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int count = 0;

            foreach (var enemy in FindAllEnemyObjects())
            {
                EnsureEnemyScript(enemy);
                NormalizeEnemyTransform(enemy);
                if (ApplyWolfVisual(enemy, wolfPrefab))
                    count++;
            }

            WireEnemyFactory(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[EnemySetup] Converted {count} enemies in {scene.name}");
        }
    }

    static System.Collections.Generic.List<GameObject> FindAllEnemyObjects()
    {
        var found = new System.Collections.Generic.HashSet<GameObject>();

        foreach (var enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            found.Add(enemy.gameObject);

        // Also pick up capsule enemies named "Enemy ..." that lack an Enemy subclass.
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in activeScene.GetRootGameObjects())
            CollectEnemyObjects(root.transform, found);

        return new System.Collections.Generic.List<GameObject>(found);
    }

    static void CollectEnemyObjects(Transform t, System.Collections.Generic.HashSet<GameObject> found)
    {
        if (IsCapsuleEnemy(t))
            found.Add(t.gameObject);

        for (int i = 0; i < t.childCount; i++)
            CollectEnemyObjects(t.GetChild(i), found);
    }

    static bool IsCapsuleEnemy(Transform t)
    {
        if (t.GetComponent<CapsuleCollider>() == null) return false;
        var name = t.name;
        return name == "Enemy" || name.StartsWith("Enemy (");
    }

    static void EnsureEnemyScript(GameObject enemy)
    {
        if (enemy.GetComponent<Enemy>() != null) return;
        enemy.AddComponent<NormEnemy>();
    }

    [MenuItem("Tools/GameSetup/Step 8 - Fix Wolf Texture and Scale")]
    public static void FixWolfEnemyAppearance()
    {
        var wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WolfPrefabPath);
        var wolfMat = AssetDatabase.LoadAssetAtPath<Material>(WolfMaterialPath);
        if (wolfMat == null)
            Debug.LogWarning("[EnemySetup] WolfEnemyMat not found — texture fix skipped.");

        // Rebuild prefabs with corrected scale/material.
        BuildWolfEnemyPrefabs();

        foreach (var scenePath in GameScenes)
        {
            if (!File.Exists(scenePath)) continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int fixedCount = 0;

            foreach (var enemy in FindAllEnemyObjects())
            {
                EnsureEnemyScript(enemy);
                NormalizeEnemyTransform(enemy);

                var wolfVisual = enemy.transform.Find("WolfVisual");
                if (wolfVisual == null && wolfPrefab != null)
                    ApplyWolfVisual(enemy, wolfPrefab);
                else
                    wolfVisual = enemy.transform.Find("WolfVisual");

                if (wolfVisual != null)
                {
                    wolfVisual.localPosition = new Vector3(0f, -0.9f, 0f);
                    wolfVisual.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    if (wolfMat != null)
                        WolfVisualSetup.ApplyMaterial(wolfVisual);
                    WolfVisualSetup.ApplyToEnemy(enemy);
                }

                var enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                    enemyScript.Initialize();

                fixedCount++;
            }

            WireEnemyFactory(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[EnemySetup] Fixed {fixedCount} wolf enemies in {scene.name}");
        }
    }

    static void NormalizeEnemyTransform(GameObject enemy)
    {
        var t = enemy.transform;
        if (t.parent != null)
        {
            // Enemies parented under scaled platforms inherit a stretched look.
            var worldPos = t.position;
            var worldRot = t.rotation;
            t.SetParent(null, true);
            t.position = worldPos;
            t.rotation = worldRot;
        }

        t.localScale = Vector3.one;
    }

    [MenuItem("Tools/GameSetup/Step 6+7 - Build Wolf Enemies (Full)")]
    public static void BuildAndReplaceAllWolfEnemies()
    {
        BuildWolfEnemyPrefabs();
        ReplaceCapsuleEnemiesWithWolf();
    }

    static GameObject BuildEnemyPrefab<T>(string prefabName, float patrolSpeed, bool addPatrol = true)
        where T : Enemy
    {
        string path = $"{EnemyPrefabFolder}/{prefabName}.prefab";

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        var root = new GameObject(prefabName);
        SetupEnemyRoot(root, patrolSpeed, addPatrol);
        root.AddComponent<T>();

        var wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WolfPrefabPath);
        if (wolfPrefab != null)
            ApplyWolfVisual(root, wolfPrefab);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void SetupEnemyRoot(GameObject root, float patrolSpeed, bool addPatrol)
    {
        var col = root.AddComponent<CapsuleCollider>();
        col.height = 2f;
        col.radius = 0.5f;
        col.center = Vector3.zero;

        var agent = root.AddComponent<NavMeshAgent>();
        agent.height = 2f;
        agent.radius = 0.5f;
        agent.speed = patrolSpeed > 0 ? patrolSpeed : 2f;

        root.AddComponent<AiControllerA>();
        root.AddComponent<AiAttack>();

        if (addPatrol)
        {
            var patrol = root.AddComponent<Patrol>();
            patrol.speed = patrolSpeed;
        }
    }

    static bool ApplyWolfVisual(GameObject enemy, GameObject wolfPrefab)
    {
        // Remove capsule mesh so only the wolf is visible.
        var meshFilter   = enemy.GetComponent<MeshFilter>();
        var meshRenderer = enemy.GetComponent<MeshRenderer>();
        if (meshFilter != null)   Object.DestroyImmediate(meshFilter, true);
        if (meshRenderer != null) Object.DestroyImmediate(meshRenderer, true);

        // Skip if wolf visual already attached.
        var existing = enemy.transform.Find("WolfVisual");
        if (existing != null) return false;

        var wolf = (GameObject)PrefabUtility.InstantiatePrefab(wolfPrefab, enemy.transform);
        wolf.name = "WolfVisual";
        wolf.transform.localPosition = new Vector3(0f, -0.9f, 0f);
        wolf.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        wolf.transform.localScale    = Vector3.one * 0.35f;

        foreach (var col in wolf.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col, true);

        WolfVisualSetup.ApplyMaterial(wolf.transform);

        if (enemy.GetComponent<EnemyWolfAnimator>() == null)
            enemy.AddComponent<EnemyWolfAnimator>();

        var enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
            enemyScript.Initialize();

        return true;
    }

    static void WireEnemyFactory(UnityEngine.SceneManagement.Scene scene)
    {
        var factory = Object.FindFirstObjectByType<AIEnemyFactory>();
        if (factory == null) return;

        factory.fastEnemyPrefab   = LoadPrefab("WolfEnemy_Fast");
        factory.normalEnemyPrefab = LoadPrefab("WolfEnemy_Normal");
        factory.heavyEnemyPrefab  = LoadPrefab("WolfEnemy_Heavy");
    }

    static GameObject LoadPrefab(string name)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>($"{EnemyPrefabFolder}/{name}.prefab");
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        }
    }
}
