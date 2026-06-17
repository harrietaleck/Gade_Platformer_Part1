using UnityEngine;

// Attach this script to any platform GameObject.
// Drag the correct material into the inspector slot —
// it will be applied automatically when the scene starts.
//
// Moving platforms  → assign IcePlatformMaterial
// Static ground     → assign SnowGroundMaterial
[RequireComponent(typeof(Renderer))]
public class PlatformTextureApplier : MonoBehaviour
{
    [Header("Drag the material for this platform here")]
    public Material platformMaterial;

    private void Awake()
    {
        if (platformMaterial == null) return;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material = platformMaterial;

        // Also apply to all child renderers (e.g. compound platform meshes)
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer child in childRenderers)
            child.material = platformMaterial;
    }
}
