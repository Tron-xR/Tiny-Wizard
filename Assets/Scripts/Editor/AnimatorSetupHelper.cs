using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AnimatorSetupHelper
{
    private const string FBX_PATH = "Assets/Animations/PlayerAnimation.fbx";
    private const string MODEL_FBX_PATH = "Assets/Animations/animated_wizard_final.fbx";
    private const string CONTROLLER_PATH = "Assets/Animations/PlayerController.controller";

    [MenuItem("Tiny Wizard/Setup Player Animations")]
    public static void SetupPlayerAnimations()
    {
        ResetModelFBXClips();
        ConfigureFBXClips();
        CreateAnimatorController();
        AssignClipsToController();
        AssignControllerToPlayer();
        EditorUtility.DisplayDialog("Done", "Player animations set up!\nClips: idle, walk, run, jump\nController assigned to Player Animator.", "OK");
    }

    private static void ConfigureFBXClips()
    {
        ConfigureSingleFBX(FBX_PATH);
    }

    private static void ResetModelFBXClips()
    {
        ModelImporter importer = AssetImporter.GetAtPath(MODEL_FBX_PATH) as ModelImporter;
        if (importer == null) return;
        if (importer.clipAnimations == null || importer.clipAnimations.Length == 0) return;
        importer.clipAnimations = new ModelImporterClipAnimation[0];
        importer.SaveAndReimport();
        Debug.Log("Reset clips on " + MODEL_FBX_PATH);
    }

    private static void ConfigureSingleFBX(string path)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning("FBX not found at: " + path);
            return;
        }

        int[] firstFrames = { 0, 56, 130, 190 };
        int[] lastFrames = { 56, 130, 190, 247 };
        string[] names = { "idle", "walk", "run", "jump" };
        bool[] loops = { true, true, true, false };

        ModelImporterClipAnimation[] clips = new ModelImporterClipAnimation[4];
        for (int i = 0; i < 4; i++)
        {
            clips[i] = new ModelImporterClipAnimation
            {
                name = names[i],
                firstFrame = firstFrames[i],
                lastFrame = lastFrames[i],
                loop = loops[i],
                loopTime = loops[i]
            };
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        Debug.Log("Clips configured for " + path + ": idle(0-56), walk(56-130), run(130-190), jump(190-247)");
    }

    public static void CreateAnimatorController()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH) != null)
            AssetDatabase.DeleteAsset(CONTROLLER_PATH);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("JumpTrigger", AnimatorControllerParameterType.Trigger);

        AnimatorControllerLayer baseLayer = controller.layers[0];
        baseLayer.name = "Base Layer";

        AnimatorState idleState = baseLayer.stateMachine.AddState("Idle", new Vector3(250, 0, 0));
        AnimatorState walkState = baseLayer.stateMachine.AddState("Walk", new Vector3(250, 100, 0));
        AnimatorState runState = baseLayer.stateMachine.AddState("Run", new Vector3(250, 200, 0));
        AnimatorState jumpState = baseLayer.stateMachine.AddState("Jump", new Vector3(250, 300, 0));
        AnimatorState fallState = baseLayer.stateMachine.AddState("Fall", new Vector3(250, 400, 0));

        baseLayer.stateMachine.defaultState = idleState;

        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.hasExitTime = false;
        idleToWalk.exitTime = 0.8f;
        idleToWalk.duration = 0.1f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.hasExitTime = false;
        walkToIdle.exitTime = 0.9f;
        walkToIdle.duration = 0.2f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

        AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
        walkToRun.hasExitTime = false;
        walkToRun.exitTime = 0.8f;
        walkToRun.duration = 0.15f;
        walkToRun.AddCondition(AnimatorConditionMode.Greater, 0.6f, "Speed");

        AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
        runToWalk.hasExitTime = false;
        runToWalk.exitTime = 0.8f;
        runToWalk.duration = 0.15f;
        runToWalk.AddCondition(AnimatorConditionMode.Less, 0.5f, "Speed");

        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.hasExitTime = false;
        runToIdle.exitTime = 0.9f;
        runToIdle.duration = 0.2f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

        AnimatorStateTransition anyToJump = baseLayer.stateMachine.AddAnyStateTransition(jumpState);
        anyToJump.hasExitTime = false;
        anyToJump.exitTime = 0.7f;
        anyToJump.duration = 0.1f;
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "JumpTrigger");

        AnimatorStateTransition jumpToFall = jumpState.AddTransition(fallState);
        jumpToFall.hasExitTime = false;
        jumpToFall.exitTime = 0.7f;
        jumpToFall.duration = 0.05f;
        jumpToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

        AnimatorStateTransition fallToIdle = fallState.AddTransition(idleState);
        fallToIdle.hasExitTime = false;
        fallToIdle.exitTime = 0.9f;
        fallToIdle.duration = 0.1f;
        fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        AnimatorStateTransition fallToWalk = fallState.AddTransition(walkState);
        fallToWalk.hasExitTime = false;
        fallToWalk.exitTime = 0.9f;
        fallToWalk.duration = 0.1f;
        fallToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        fallToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("Animator Controller created at: " + CONTROLLER_PATH);
    }

    private static void AssignClipsToController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        if (controller == null)
        {
            Debug.LogWarning("Controller not found. Create it first.");
            return;
        }

        Object[] fbxClips = AssetDatabase.LoadAllAssetRepresentationsAtPath(FBX_PATH);

        AssignClipToState(controller, "Idle", fbxClips, "idle");
        AssignClipToState(controller, "Walk", fbxClips, "walk");
        AssignClipToState(controller, "Run", fbxClips, "run");
        AssignClipToState(controller, "Jump", fbxClips, "jump");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("Animation clips assigned to controller states.");
    }

    private static void AssignClipToState(AnimatorController controller, string stateName, Object[] clips, string clipName)
    {
        foreach (Object obj in clips)
        {
            if (obj is AnimationClip clip && clip.name == clipName)
            {
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    foreach (ChildAnimatorState state in layer.stateMachine.states)
                    {
                        if (state.state.name == stateName)
                        {
                            state.state.motion = clip;
                            return;
                        }
                    }
                }
            }
        }

        Debug.LogWarning($"Clip '{clipName}' not found in FBX for state '{stateName}'.");
    }

    private static void AssignControllerToPlayer()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        if (controller == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("No Player found to assign animator controller.");
            return;
        }

        Animator animator = player.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator found on Player or its children.");
            return;
        }

        animator.runtimeAnimatorController = controller;
        animator.avatar = null;
        EditorUtility.SetDirty(animator);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"Animator Controller assigned to {animator.gameObject.name}.");
    }

    [MenuItem("Tiny Wizard/Replace Player Model")]
    public static void ReplacePlayerModel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "No Player found in scene.", "OK");
            return;
        }

        string modelPath = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_FBX_PATH) != null ? MODEL_FBX_PATH : FBX_PATH;
        GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (fbxPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "No model FBX found at:\n" + MODEL_FBX_PATH + "\nor\n" + FBX_PATH, "OK");
            return;
        }

        Transform modelCube = player.transform.Find("Model Cube");

        // If Model Cube was already deleted by a previous run, clean up the invisible FBX model and recreate
        if (modelCube == null)
        {
            Transform oldModel = player.transform.Find("Model");
            if (oldModel != null)
                Object.DestroyImmediate(oldModel.gameObject);

            // Recreate Model Cube
            GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.name = "Model Cube";
            newCube.transform.SetParent(player.transform, false);
            newCube.transform.localPosition = new Vector3(0, 0.3f, 0);
            newCube.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
            newCube.GetComponent<Collider>().enabled = false;
            modelCube = newCube.transform;

            // Move player's Animator back to player if missing
            if (player.GetComponent<Animator>() == null)
            {
                Animator animOnChild = player.GetComponentInChildren<Animator>();
                if (animOnChild != null)
                {
                    Animator newPlayerAnim = player.AddComponent<Animator>();
                    newPlayerAnim.runtimeAnimatorController = animOnChild.runtimeAnimatorController;
                    Object.DestroyImmediate(animOnChild);
                }
            }
        }

        Transform castOrigin = modelCube.Find("CastOrigin");
        Vector3 savedCastPos = castOrigin != null ? castOrigin.localPosition : Vector3.zero;
        Vector3 savedCastScale = castOrigin != null ? castOrigin.localScale : Vector3.one;

        Object fbxInstance = PrefabUtility.InstantiatePrefab(fbxPrefab);
        GameObject fbxGO = fbxInstance as GameObject;
        bool hasMesh = fbxGO != null && fbxGO.GetComponentInChildren<Renderer>() != null;
        Object.DestroyImmediate(fbxGO);

        if (!hasMesh)
        {
            EditorUtility.DisplayDialog("No Mesh Found",
                "The FBX file contains only animation data (no mesh).\n\n" +
                "The cube will be kept as the visual model. The Animator\n" +
                "will be moved to Model Cube so animations apply to it.\n\n" +
                "Run 'Tiny Wizard > Setup Player Animations' to finish setup.",
                "OK");

            if (castOrigin != null)
                castOrigin.SetParent(modelCube, true);

            Animator sourceAnim = player.GetComponent<Animator>();
            if (sourceAnim != null)
            {
                Animator cubeAnim = modelCube.gameObject.AddComponent<Animator>();
                cubeAnim.runtimeAnimatorController = sourceAnim.runtimeAnimatorController;
                cubeAnim.avatar = sourceAnim.avatar;
                Object.DestroyImmediate(sourceAnim);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return;
        }

        Vector3 savedPos = modelCube.localPosition;
        Vector3 savedScale = modelCube.localScale;
        Quaternion savedRot = modelCube.localRotation;

        Object.DestroyImmediate(modelCube.gameObject);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab, player.transform);
        instance.name = "Model";
        instance.transform.localPosition = savedPos;
        instance.transform.localRotation = savedRot;
        instance.transform.localScale = savedScale;

        Animator instanceAnim = instance.GetComponent<Animator>();
        if (instanceAnim != null)
            Object.DestroyImmediate(instanceAnim);

        if (castOrigin != null)
        {
            GameObject newCast = new GameObject("CastOrigin");
            newCast.transform.SetParent(instance.transform, false);
            newCast.transform.localPosition = savedCastPos;
            newCast.transform.localScale = savedCastScale;
        }

        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null)
        {
            Animator newAnim = instance.AddComponent<Animator>();
            newAnim.runtimeAnimatorController = playerAnim.runtimeAnimatorController;
            newAnim.avatar = playerAnim.avatar;
            Object.DestroyImmediate(playerAnim);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Done", "Player model replaced with FBX mesh.\n\nRun 'Tiny Wizard > Setup Player Animations' to assign the controller and set up animations.", "OK");
    }
}
