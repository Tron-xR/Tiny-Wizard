using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class HealthSetupWindow : EditorWindow
{
    [MenuItem("Tools/Health System Setup")]
    public static void ShowWindow()
    {
        GetWindow<HealthSetupWindow>("Health System Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Health System Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Add PlayerHealth to Player", GUILayout.Height(30)))
        {
            AddPlayerHealth();
        }

        if (GUILayout.Button("Create Health UI", GUILayout.Height(30)))
        {
            CreateHealthUI();
        }

        if (GUILayout.Button("Wire Everything", GUILayout.Height(30)))
        {
            WireReferences();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "1. Click 'Add PlayerHealth to Player'\n" +
            "2. Click 'Create Health UI'\n" +
            "3. Click 'Wire Everything'",
            MessageType.Info
        );
    }

    private static void AddPlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "No Player GameObject found in the scene.", "OK");
            return;
        }

        if (player.GetComponent<PlayerHealth>() != null)
        {
            EditorUtility.DisplayDialog("Already Exists", "PlayerHealth already exists on the Player.", "OK");
            return;
        }

        PlayerHealth health = player.AddComponent<PlayerHealth>();
        Renderer renderer = player.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            SerializedObject so = new SerializedObject(health);
            so.FindProperty("playerRenderer").objectReferenceValue = renderer;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("PlayerHealth added to Player.");
    }

    private static void CreateHealthUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in the scene.", "OK");
            return;
        }

        if (canvas.GetComponentInChildren<HealthUI>() != null)
        {
            EditorUtility.DisplayDialog("Already Exists", "HealthUI already exists in the scene.", "OK");
            return;
        }

        GameObject healthUIObj = new GameObject("HealthUI", typeof(RectTransform), typeof(CanvasRenderer));
        healthUIObj.transform.SetParent(canvas.transform, false);
        HealthUI healthUI = healthUIObj.AddComponent<HealthUI>();

        RectTransform rootRT = healthUIObj.GetComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0, 0);
        rootRT.anchorMax = new Vector2(0, 0);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = new Vector2(200, 40);
        rootRT.sizeDelta = new Vector2(300, 30);

        GameObject bgObj = new GameObject("HealthBar_Background", typeof(RectTransform), typeof(CanvasRenderer));
        bgObj.transform.SetParent(healthUIObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillObj = new GameObject("HealthBar_Fill", typeof(RectTransform), typeof(CanvasRenderer));
        fillObj.transform.SetParent(healthUIObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        fillImage.color = Color.green;

        RectTransform fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2, 2);
        fillRT.offsetMax = new Vector2(-2, -2);

        GameObject textObj = new GameObject("HealthText", typeof(RectTransform), typeof(CanvasRenderer));
        textObj.transform.SetParent(healthUIObj.transform, false);
        Text healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (healthText.font == null)
            healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        healthText.fontSize = 14;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        healthText.text = "100 / 100";

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        SerializedObject so = new SerializedObject(healthUI);
        so.FindProperty("healthFill").objectReferenceValue = fillImage;
        so.FindProperty("healthText").objectReferenceValue = healthText;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Health UI created successfully.");
    }

    private static void WireReferences()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
        {
            EditorUtility.DisplayDialog("Error", "No PlayerHealth found. Click 'Add PlayerHealth to Player' first.", "OK");
            return;
        }

        HealthUI ui = FindFirstObjectByType<HealthUI>();
        if (ui == null)
        {
            EditorUtility.DisplayDialog("Error", "No HealthUI found. Click 'Create Health UI' first.", "OK");
            return;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Health system wired. PlayerHealth and HealthUI are connected.");

        EditorUtility.DisplayDialog("Success", "Health system is set up!\n\n" +
            "The UI will automatically find PlayerHealth at runtime.", "OK");
    }
}
