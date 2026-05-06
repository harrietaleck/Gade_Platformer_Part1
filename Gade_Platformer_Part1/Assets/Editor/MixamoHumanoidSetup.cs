#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Fixes Mixamo-style FBX import via the ModelImporter API (YAML avatar fileIDs are fragile).
/// Run once: Tools → Fix → Mixamo — Humanoid import + animator motions
/// </summary>
public static class MixamoHumanoidSetup
{
    const string CharacterPath = "Assets/Player_Assets/Erika Archer_Wiping Sweat_&idle.fbx";
    const string RunningPath = "Assets/Player_Assets/Erika Archer_Running_animation_noskin.fbx";
    const string JumpPath = "Assets/Player_Assets/Erika Archer_JumpingAnimation_noSkin.fbx";
    const string ControllerPath = "Assets/Player_Assets/Erica Animation Controller.controller";
    const string IdleClipPath = "Assets/Player_Assets/Idle.anim";

    [MenuItem("Tools/Fix/Mixamo — Humanoid import + animator motions")]
    public static void Run()
    {
        var charImporter = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
        if (charImporter == null)
        {
            Debug.LogError("MixamoHumanoidSetup: Character FBX not found at " + CharacterPath);
            return;
        }

        charImporter.animationType = ModelImporterAnimationType.Human;
        charImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        charImporter.SaveAndReimport();

        Avatar avatar = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(CharacterPath))
        {
            if (obj is Avatar a)
            {
                avatar = a;
                break;
            }
        }

        if (avatar == null)
        {
            Debug.LogError("MixamoHumanoidSetup: No Avatar on character after Human rig reimport. Open the character FBX → Rig and fix mapping manually.");
            return;
        }

        void CopyHumanoidFromCharacter(string animPath)
        {
            var imp = AssetImporter.GetAtPath(animPath) as ModelImporter;
            if (imp == null)
            {
                Debug.LogError("MixamoHumanoidSetup: Missing " + animPath);
                return;
            }

            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            imp.sourceAvatar = avatar;
            imp.SaveAndReimport();
        }

        CopyHumanoidFromCharacter(RunningPath);
        CopyHumanoidFromCharacter(JumpPath);

        AnimationClip PickMixamoClip(string animPath)
        {
            AnimationClip fallback = null;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(animPath))
            {
                if (obj is not AnimationClip clip)
                    continue;
                if (clip.name == "mixamo.com")
                    return clip;
                fallback ??= clip;
            }

            return fallback;
        }

        var runClip = PickMixamoClip(RunningPath);
        var jumpClip = PickMixamoClip(JumpPath);
        if (runClip == null || jumpClip == null)
        {
            Debug.LogError("MixamoHumanoidSetup: Could not find AnimationClip sub-assets. Check Console for import errors on the FBX files.");
            return;
        }

        var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClipPath);
        if (idleClip == null)
        {
            Debug.LogError("MixamoHumanoidSetup: " + IdleClipPath + " not found.");
            return;
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("MixamoHumanoidSetup: Controller not found at " + ControllerPath);
            return;
        }

        var stateMachine = controller.layers[0].stateMachine;
        foreach (var child in stateMachine.states)
        {
            var state = child.state;
            switch (state.name)
            {
                case "Idle":
                    state.motion = idleClip;
                    break;
                case "Walk":
                    state.motion = runClip;
                    state.speed = 0.58f;
                    break;
                case "Run":
                    state.motion = runClip;
                    state.speed = 1f;
                    break;
                case "Fall":
                    state.motion = jumpClip;
                    state.speed = 1f;
                    break;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("MixamoHumanoidSetup: Done. Character + run/jump FBX use Human rig (copy avatar). Controller Idle=Idle.anim, Walk/Run=running clip, Fall=jump clip.");
    }
}
#endif
