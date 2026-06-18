// ============================================================
// GameMusicManager.cs
//
// Plays GAMEPLAYSOUND.mp3 continuously across ALL scenes.
// Uses DontDestroyOnLoad so the music never stops on scene load.
// Singleton — only one instance ever exists at a time.
//
// HOW TO USE:
//   Run Tools > Setup Game Music  (once, on any game scene).
//   The script auto-assigns the clip and persists forever.
// ============================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class GameMusicManager : MonoBehaviour
{
    public static GameMusicManager Instance { get; private set; }

    [Header("Music Clip")]
    public AudioClip musicClip;    // Assign: Assets/GameSoundPlay/GAMEPLAYSOUND.mp3

    [Range(0f, 1f)]
    public float volume = 0.4f;

    private AudioSource _src;

    private void Awake()
    {
        // Singleton — destroy any duplicate that loads in a later scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // Survive scene transitions

        _src = GetComponent<AudioSource>();
        _src.clip        = musicClip;
        _src.loop        = true;
        _src.playOnAwake = false;
        _src.volume      = volume;
        _src.spatialBlend = 0f;   // 2-D (not positional)

        if (musicClip != null && !_src.isPlaying)
            _src.Play();
    }

    // Volume can be adjusted at runtime (e.g. from a settings screen)
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (_src != null) _src.volume = volume;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        if (musicClip == null)
            musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/GameSoundPlay/GAMEPLAYSOUND.mp3");
    }
#endif
}
