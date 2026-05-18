using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorSetupHelper
{
    [MenuItem("Tiny Wizard/Create Animator Controller")]
    public static void CreateAnimatorController()
    {
        string path = "Assets/Animations/PlayerController.controller";

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

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

        EditorUtility.DisplayDialog("Success", "Animator Controller created at:\n" + path +
            "\n\nNow assign it to your Player/Model's Animator component!", "OK");
    }
}
