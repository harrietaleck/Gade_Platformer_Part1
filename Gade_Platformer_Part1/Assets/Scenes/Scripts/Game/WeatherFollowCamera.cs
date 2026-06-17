// ============================================================
// WeatherFollowCamera.cs
//
// Keeps a weather effect (rain / snow / fog particle system)
// centred on the main camera's XZ position at all times, so
// the weather always covers the player regardless of where
// they travel in the level. The Y position is fixed at the
// value set in the editor, so rain always falls from the same
// height above the scene.
// ============================================================

using UnityEngine;

public class WeatherFollowCamera : MonoBehaviour
{
    private Transform _cam;
    private float     _fixedY;   // preserve the spawn height set in editor

    void Start()
    {
        _fixedY = transform.position.y;

        var mainCam = Camera.main;
        if (mainCam != null)
            _cam = mainCam.transform;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        // Follow camera X/Z, keep fixed Y so rain always falls from the same height
        transform.position = new Vector3(_cam.position.x, _fixedY, _cam.position.z);
    }
}
