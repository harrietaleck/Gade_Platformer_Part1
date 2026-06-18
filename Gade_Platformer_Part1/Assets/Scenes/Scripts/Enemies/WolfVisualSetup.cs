using UnityEngine;

// Ensures wolf enemies use the correct URP material and uniform scale.
public static class WolfVisualSetup
{
    const string MaterialPath = "Assets/Materials/WolfEnemyMat.mat";
    static Material _wolfMaterial;

    public static void ApplyToEnemy(GameObject enemy)
    {
        var wolfVisual = enemy.transform.Find("WolfVisual");
        if (wolfVisual == null) return;

        ApplyMaterial(wolfVisual);

        // Only normalise the visual root — do not touch bone scales inside the rig.
        var s = wolfVisual.localScale;
        float uniform = Mathf.Max(s.x, Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z)));
        if (uniform > 0.01f)
            wolfVisual.localScale = Vector3.one * uniform;
    }

    public static void ApplyMaterial(Transform wolfRoot)
    {
        var mat = GetMaterial();
        if (mat == null) return;

        foreach (var smr in wolfRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            smr.sharedMaterial = mat;

        foreach (var mr in wolfRoot.GetComponentsInChildren<MeshRenderer>(true))
            mr.sharedMaterial = mat;
    }

    static Material GetMaterial()
    {
        if (_wolfMaterial != null) return _wolfMaterial;

#if UNITY_EDITOR
        _wolfMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
#endif
        if (_wolfMaterial == null)
            _wolfMaterial = Resources.Load<Material>("WolfEnemyMat");

        return _wolfMaterial;
    }
}
