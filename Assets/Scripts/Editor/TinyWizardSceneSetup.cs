using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class TinyWizardSceneSetup
{
    [MenuItem("Tiny Wizard/Setup Scene")]
    public static void SetupScene()
    {
        GameObject playerRoot = new GameObject("Player");
        playerRoot.tag = "Player";

        GameObject model = new GameObject("Model");
        model.transform.SetParent(playerRoot.transform);
        model.transform.localPosition = Vector3.zero;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(model.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);

        Collider cubeCollider = cube.GetComponent<Collider>();
        if (cubeCollider != null)
            Object.DestroyImmediate(cubeCollider);

        GameObject cameraTarget = new GameObject("CameraTarget");
        cameraTarget.transform.SetParent(playerRoot.transform);
        cameraTarget.transform.localPosition = new Vector3(0, 1.6f, 0);

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(playerRoot.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        Rigidbody rb = playerRoot.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.linearDamping = 5f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        CapsuleCollider capsule = playerRoot.AddComponent<CapsuleCollider>();
        capsule.radius = 0.3f;
        capsule.height = 1.8f;
        capsule.center = new Vector3(0, 0.9f, 0);

        playerRoot.AddComponent<Animator>();
        PlayerInput playerInput = playerRoot.AddComponent<PlayerInput>();
        playerInput.defaultActionMap = "Player";
        playerInput.defaultControlScheme = "Keyboard&Mouse";
        playerRoot.AddComponent<PlayerInputHandler>();
        playerRoot.AddComponent<GroundChecker>();
        playerRoot.AddComponent<PlayerController>();

        SetupGroundLayer();

        GameObject groundFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundFloor.name = "Ground";
        groundFloor.transform.position = new Vector3(0, -1, 0);
        groundFloor.transform.localScale = Vector3.one * 10;
        groundFloor.layer = LayerMask.NameToLayer("Ground");

        Object.DestroyImmediate(groundFloor.GetComponent<Collider>());
        BoxCollider groundCollider = groundFloor.AddComponent<BoxCollider>();
        groundCollider.size = new Vector3(10, 0.1f, 10);

        GameObject light = new GameObject("Directional Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = 1f;
        light.transform.eulerAngles = new Vector3(50, -30, 0);

        GameObject cameraObj = new GameObject("Main Camera");
        Camera cameraComponent = cameraObj.AddComponent<Camera>();
        cameraComponent.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 3, -6);
        cameraObj.transform.eulerAngles = new Vector3(15, 0, 0);

        ThirdPersonCamera tpc = cameraObj.AddComponent<ThirdPersonCamera>();
        tpc.SetTarget(cameraTarget.transform);

        GameObject cameraPivot = new GameObject("CameraPivot");
        cameraPivot.transform.position = cameraTarget.transform.position + Vector3.up * 0.5f;

        string[] guids = AssetDatabase.FindAssets("t:InputActionAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("TinyWizardControls"))
            {
                InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (actions != null)
                    playerInput.actions = actions;
                break;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Setup Complete", "Scene setup finished!\n\n" +
            "Input actions were auto-assigned to PlayerInput.\n" +
            "Next: Run 'Tiny Wizard → Create Animator Controller'\n" +
            "Then press Play to test.", "OK");
    }

    private static void SetupGroundLayer()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == "Ground")
                return;
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = "Ground";
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }
}
