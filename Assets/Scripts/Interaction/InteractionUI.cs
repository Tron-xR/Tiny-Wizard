using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the on-screen interaction prompt.
/// Automatically shows when the player looks at an interactable, hides otherwise.
/// 
/// How to set up:
/// 1. Create a Canvas (UI → Canvas) or use an existing one
/// 2. Set Canvas Render Mode to "Screen Space - Overlay"
/// 3. Create a child Text (UI → Text - Legacy) or TextMeshPro
/// 4. Name it "InteractionPrompt"
/// 5. Assign the Text component reference here
/// 6. Configure position, font size, and colour in the Inspector
/// 
/// If using TextMeshPro, simply replace the Text reference with TMP_Text
/// and import the right namespace (TMPro). The same logic applies.
/// 
/// Canvas hierarchy:
///   Canvas (Screen Space - Overlay)
///   └── InteractionPrompt (Text)
///       └── [optional background panel for readability]
/// 
/// Inspector defaults:
/// - Default Prompt: "Press E to interact" (fallback if no specific text)
/// - Hidden at start: the prompt starts invisible until needed
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text promptText;

    [Header("Display Settings")]
    [SerializeField] private string defaultPrompt = "Press E to interact";

    [Header("Colours")]
    [SerializeField] private Color hiddenColor = new Color(1, 1, 1, 0);

    // Current prompt string being displayed
    private string currentText = "";

    // Original visible colour to restore when showing the prompt
    private Color visibleColor;

    // ===== INITIALISATION =====

    private void OnEnable()
    {
        if (promptText == null)
            TryFindPromptText();

        if (promptText != null)
        {
            visibleColor = promptText.color;
            promptText.color = hiddenColor;
            promptText.text = defaultPrompt;
        }
    }

    /// <summary>
    /// Auto-find the InteractionPrompt text component if not assigned.
    /// Searches children and then the entire scene.
    /// </summary>
    private void TryFindPromptText()
    {
        promptText = GetComponentInChildren<Text>();

        if (promptText == null)
        {
            GameObject found = GameObject.Find("InteractionPrompt");
            if (found != null)
                promptText = found.GetComponent<Text>();
        }
    }

    // ===== PUBLIC METHODS =====

    /// <summary>
    /// Show the interaction prompt with a specific message.
    /// </summary>
    /// <param name="text">The prompt text (e.g. "Pick up bread", "Push spoon")</param>
    public void ShowPrompt(string text)
    {
        if (promptText == null) return;

        currentText = text;
        promptText.text = text;
        promptText.color = visibleColor;
    }

    /// <summary>
    /// Hide the interaction prompt and reset to default.
    /// </summary>
    public void HidePrompt()
    {
        if (promptText == null) return;

        promptText.text = "";
        currentText = "";
        promptText.color = hiddenColor;
    }

    /// <summary>
    /// Set a custom prompt override that persists until changed.
    /// </summary>
    public void SetCustomPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        currentText = text;
    }

    /// <summary>
    /// The currently displayed prompt text.
    /// </summary>
    public string CurrentText => currentText;
}
