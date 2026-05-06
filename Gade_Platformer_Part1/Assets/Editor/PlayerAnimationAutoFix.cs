using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor utilities for the Player rig:
///   * Extract Walk/Fall clips from their Mixamo FBXs into proper .anim files.
///   * Rebuild the Erica Animation Controller with Idle/Walk/Run/Jump/Fall.
///   * Re-wire Player.animator to the child Animator.
/// </summary>
public static class PlayerAnimationAutoFix
{
    private const string PlayerPath        = "Player";
    private const string PlayerAssetsDir   = "Assets/Player_Assets";
    private const string ControllerPath    = PlayerAssetsDir + "/Erica Animation Controller.controller";
    private const string IdleAnimPath      = PlayerAssetsDir + "/Idle.anim";
    private const string WalkAnimPath      = PlayerAssetsDir + "/Walk.anim";
    private const string RunAnimPath       = PlayerAssetsDir + "/Run.anim";
    private const string JumpAnimPath      = PlayerAssetsDir + "/Jump.anim";
    private const string FallAnimPath      = PlayerAssetsDir + "/Fall.anim";
    private const string IdleFbxPath       = PlayerAssetsDir + "/Erika Archer_Wiping Sweat_&idle.fbx";
    private const string WalkFbxPath       = PlayerAssetsDir + "/WalkingEricaAnimation_WithoutSkin.fbx";
    private const string RunFbxPath        = PlayerAssetsDir + "/Erika Archer_Running_animation_noskin.fbx";
    private const string JumpFbxPath       = PlayerAssetsDir + "/Erika Archer_JumpingAnimation_noSkin.fbx";
    private const string FallFbxPath       = PlayerAssetsDir + "/FallinAnimation_noSkin.fbx";

    // State integers must match Player.cs constants.
    private const int StateIdle = 0;
    private const int StateWalk = 1;
    private const int StateRun  = 2;
    private const int StateFall = 3;
    private const int StateJump = 4;
    private const string StateParam = "State";

    [MenuItem("Tools/Fix/Player Animation Wiring")]
    public static void RebuildEverything()
    {
        // Re-extract every clip from its source FBX so we know each .anim
        // actually contains the animation its filename advertises.
        ExtractClipFromFbx(IdleFbxPath, IdleAnimPath, loopTime: true);
        ExtractClipFromFbx(WalkFbxPath, WalkAnimPath, loopTime: true);
        ExtractClipFromFbx(RunFbxPath,  RunAnimPath,  loopTime: true);
        ExtractClipFromFbx(JumpFbxPath, JumpAnimPath, loopTime: false); // one-shot
        ExtractClipFromFbx(FallFbxPath, FallAnimPath, loopTime: true);
        BuildController();
        WireSceneReferences();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PlayerAnimationAutoFix: rebuild complete.");
    }

    // --- Clip extraction ------------------------------------------------------

    private static void ExtractClipFromFbx(string fbxPath, string outPath, bool loopTime)
    {
        var src = FindHumanoidClip(fbxPath);
        if (src == null)
        {
            Debug.LogError($"PlayerAnimationAutoFix: no AnimationClip found inside '{fbxPath}'.");
            return;
        }

        // Canonical pattern for cloning an FBX-embedded clip into a standalone .anim:
        // new AnimationClip() + EditorUtility.CopySerialized(src, dst). Object.Instantiate()
        // does NOT reliably deep-copy curves of importer-owned clips.
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(outPath);
        if (existing != null)
        {
            // Preserve the existing asset's GUID so any Animator references stay intact.
            EditorUtility.CopySerialized(src, existing);
            existing.name = System.IO.Path.GetFileNameWithoutExtension(outPath);
            existing.legacy = false;   // CopySerialized may have inherited m_Legacy=1 from the FBX clip

            var s = AnimationUtility.GetAnimationClipSettings(existing);
            s.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(existing, s);

            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            Debug.Log($"PlayerAnimationAutoFix: refreshed '{outPath}' " +
                      $"(loopTime={loopTime}, srcLegacy={src.legacy}, srcLen={src.length:F3}s).");
        }
        else
        {
            var dst = new AnimationClip();
            EditorUtility.CopySerialized(src, dst);
            dst.name = System.IO.Path.GetFileNameWithoutExtension(outPath);
            dst.legacy = false;

            var s = AnimationUtility.GetAnimationClipSettings(dst);
            s.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(dst, s);

            AssetDatabase.CreateAsset(dst, outPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"PlayerAnimationAutoFix: created '{outPath}' " +
                      $"(loopTime={loopTime}, srcLegacy={src.legacy}, srcLen={src.length:F3}s).");
        }
    }

    private static AnimationClip FindHumanoidClip(string fbxPath)
    {
        // FBX importers with legacyGenerateAnimations != 0 emit BOTH a Mecanim clip
        // and a Legacy clip. Mecanim/humanoid is what we want — pick non-legacy first,
        // then fall back to anything sensible.
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

        AnimationClip legacyFallback = null;
        foreach (var o in assets)
        {
            if (o is AnimationClip c && !c.name.StartsWith("__"))
            {
                if (!c.legacy) return c;
                legacyFallback = c;
            }
        }
        return legacyFallback;
    }

    // --- Controller rebuild ---------------------------------------------------

    private static void BuildController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"PlayerAnimationAutoFix: controller missing at '{ControllerPath}'.");
            return;
        }

        EnsureIntParam(controller, StateParam);

        var sm = controller.layers[0].stateMachine;
        // Wipe existing states so we have a deterministic graph every time.
        foreach (var s in sm.states) sm.RemoveState(s.state);
        foreach (var t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);

        var idle = sm.AddState("Idle", new Vector3(30,  190, 0));
        var walk = sm.AddState("Walk", new Vector3(280, 190, 0));
        var run  = sm.AddState("Run",  new Vector3(280, 60,  0));
        var fall = sm.AddState("Fall", new Vector3(30,  330, 0));
        var jump = sm.AddState("Jump", new Vector3(280, 330, 0));

        idle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleAnimPath);
        walk.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkAnimPath);
        run.motion  = AssetDatabase.LoadAssetAtPath<AnimationClip>(RunAnimPath);
        fall.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(FallAnimPath);
        jump.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(JumpAnimPath);

        sm.defaultState = idle;

        var states = new (AnimatorState s, int id)[]
        {
            (idle, StateIdle), (walk, StateWalk), (run, StateRun), (fall, StateFall), (jump, StateJump)
        };

        // Use AnyState transitions: simpler graph, every state can interrupt to the new one.
        foreach (var target in states)
        {
            var t = sm.AddAnyStateTransition(target.s);
            t.AddCondition(AnimatorConditionMode.Equals, target.id, StateParam);
            t.duration         = target.id == StateJump ? 0.05f : 0.12f;
            t.hasExitTime      = false;
            t.hasFixedDuration = true;
            t.canTransitionToSelf = false;
        }

        EditorUtility.SetDirty(controller);
        Debug.Log("PlayerAnimationAutoFix: rebuilt animator graph (Idle/Walk/Run/Jump/Fall).");
    }

    private static void EnsureIntParam(AnimatorController controller, string name)
    {
        foreach (var p in controller.parameters)
        {
            if (p.name == name)
            {
                if (p.type != AnimatorControllerParameterType.Int)
                    Debug.LogWarning($"PlayerAnimationAutoFix: parameter '{name}' exists with non-int type.");
                return;
            }
        }
        controller.AddParameter(name, AnimatorControllerParameterType.Int);
    }

    // --- Scene wiring ---------------------------------------------------------

    private static void WireSceneReferences()
    {
        var player = GameObject.Find(PlayerPath);
        if (player == null)
        {
            Debug.LogError("PlayerAnimationAutoFix: 'Player' not found in active scene.");
            return;
        }

        var animator = player.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Debug.LogError("PlayerAnimationAutoFix: no Animator under Player.");
            return;
        }

        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"PlayerAnimationAutoFix: controller missing at '{ControllerPath}'.");
            return;
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        var playerScript = player.GetComponent<Player>();
        if (playerScript == null)
        {
            Debug.LogError("PlayerAnimationAutoFix: Player component missing on Player.");
            return;
        }

        playerScript.animator = animator;

        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(playerScript);
        EditorSceneManager.MarkSceneDirty(player.scene);
    }
}
