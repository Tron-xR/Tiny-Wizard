using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor script to set up the Interaction System in the current scene.
/// Adds to the "Tiny Wizard" menu for easy access.
/// 
/// Run this AFTER running "Tiny Wizard → Setup Scene" so the Player already exists.
/// 
/// What this does:
/// 1. Adds InteractionController to the Player
/// 2. Creates HoldPoint under Player
/// 3. Creates Canvas with InteractionPrompt UI
/// 4. Wires references between components
/// 5. Sets up the Interactable layer
/// 
/// Usage: Tiny Wizard → Setup Interaction System
/// </summary>
public class InteractionSetupHelper
{
    [MenuItem("Tiny Wizard/Setup Interaction System")]
    public static void SetupInteractionSystem()
    {
        // === 1. FIND OR CREATE PLAYER ===
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            EditorUtility.DisplayDialog("Player Not Found",
                "Run 'Tiny Wizard → Setup Scene' first to create the Player.",
                "OK");
            return;
        }

        // === 2. ADD INTERACTION CONTROLLER ===
        InteractionController controller = player.GetComponent<InteractionController>();
        if (controller == null)
            controller = player.AddComponent<InteractionController>();

        // === 3. FIND OR CREATE CAMERA ===
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = GameObject.FindFirstObjectByType<Camera>();
        }

        if (cam != null)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("interactionCamera").objectReferenceValue = cam;
            so.ApplyModifiedProperties();
        }

        // === 4. CREATE HOLD POINT ===
        Transform holdPoint = player.transform.Find("HoldPoint");
        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(player.transform);
            hp.transform.localPosition = new Vector3(0, 1.2f, 1f);
            holdPoint = hp.transform;
        }

        SerializedObject so2 = new SerializedObject(controller);
        so2.FindProperty("holdPoint").objectReferenceValue = holdPoint;
        so2.ApplyModifiedProperties();

        // === 5. FIND OR CREATE INPUT HANDLER ===
        PlayerInputHandler inputHandler = player.GetComponent<PlayerInputHandler>();
        if (inputHandler == null)
        {
            inputHandler = player.AddComponent<PlayerInputHandler>();
        }

        SerializedObject so3 = new SerializedObject(controller);
        so3.FindProperty("inputHandler").objectReferenceValue = inputHandler;
        so3.ApplyModifiedProperties();

        // === 6. CREATE CANVAS + INTERACTION PROMPT ===
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Find or create InteractionPrompt text
        Text promptText = null;
        GameObject promptGO = GameObject.Find("InteractionPrompt");
        if (promptGO == null)
        {
            promptGO = new GameObject("InteractionPrompt");
            promptGO.transform.SetParent(canvas.transform);

            // Centered at the bottom of the screen
            RectTransform rt = promptGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 50);
            rt.sizeDelta = new Vector2(600, 50);

            promptText = promptGO.AddComponent<Text>();
            promptText.text = "Press E to interact";
            promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptText.fontSize = 24;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color = Color.white;
        }
        else
        {
            promptText = promptGO.GetComponent<Text>();
            if (promptText == null)
                promptText = promptGO.AddComponent<Text>();

            promptText.text = "Press E to interact";
            promptText.fontSize = 24;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color = Color.white;
        }

        // Find or create InteractionUI
        InteractionUI ui = promptGO.GetComponent<InteractionUI>();
        if (ui == null)
            ui = promptGO.AddComponent<InteractionUI>();

        SerializedObject so4 = new SerializedObject(ui);
        so4.FindProperty("promptText").objectReferenceValue = promptText;
        so4.ApplyModifiedProperties();

        // Wire UI reference into controller
        SerializedObject so5 = new SerializedObject(controller);
        so5.FindProperty("interactionUI").objectReferenceValue = ui;
        so5.ApplyModifiedProperties();

        // === 7. SET UP INTERACTABLE LAYER ===
        SetupInteractableLayer();

        // === 8. MARK SCENE DIRTY ===
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Interaction System Ready",
            "Setup complete!\n\n" +
            "What was added:\n" +
            "- InteractionController on Player\n" +
            "- HoldPoint child under Player\n" +
            "- Canvas with InteractionPrompt\n" +
            "- References wired automatically\n\n" +
            "Next steps:\n" +
            "1. Create objects with InteractableObject scripts\n" +
            "2. Assign them to the 'Interactable' layer\n" +
            "3. Press Play and look at them — prompt appears!\n" +
            "4. Press E to interact, pick up, or push\n\n" +
            "See Assets/Scripts/Interaction/ for all scripts.",
            "OK");
    }

    /// <summary>
    /// Creates the "Interactable" layer if it doesn't already exist.
    /// Layer index starts at 8 (first user-defined layer).
    /// </summary>
    private static void SetupInteractableLayer()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == "Interactable")
                return;
        }

        // Find the first empty user layer slot (starting at index 8)
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = "Interactable";
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }
}
