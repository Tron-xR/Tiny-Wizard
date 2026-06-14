using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialPrompt
    {
        public string promptId = "tutorial_1";
        public GameObject promptCanvas;
        public float displayDuration = 5f;
        public bool dismissOnMove = false;
        public bool dismissOnInteract = false;
        public bool dismissAfterDelay = true;
    }

    [Header("Prompts")]
    [SerializeField] private TutorialPrompt[] prompts;

    [Header("Settings")]
    [SerializeField] private bool resetOnReload = false;

    private PlayerInputHandler inputHandler;
    private int currentPromptIndex = 0;
    private bool isShowing = false;

    private void OnEnable()
    {
        if (!resetOnReload && PlayerPrefs.GetInt("TutorialDone", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        inputHandler = FindFirstObjectByType<PlayerInputHandler>();

        foreach (TutorialPrompt prompt in prompts)
        {
            if (prompt.promptCanvas != null)
                prompt.promptCanvas.SetActive(false);
        }

        ShowNextPrompt();
    }

    private void Update()
    {
        if (!isShowing || currentPromptIndex >= prompts.Length) return;

        TutorialPrompt current = prompts[currentPromptIndex];
        bool shouldDismiss = false;

        if (current.dismissOnMove && inputHandler != null && inputHandler.MoveInput.magnitude > 0.1f)
            shouldDismiss = true;

        if (current.dismissOnInteract && inputHandler != null && Input.GetButtonDown("Fire1"))
            shouldDismiss = true;

        if (shouldDismiss)
        {
            DismissCurrent();
        }
    }

    public void ShowNextPrompt()
    {
        if (currentPromptIndex >= prompts.Length)
        {
            CompleteTutorial();
            return;
        }

        TutorialPrompt prompt = prompts[currentPromptIndex];
        if (prompt.promptCanvas != null)
        {
            prompt.promptCanvas.SetActive(true);
            isShowing = true;
        }

        if (prompt.dismissAfterDelay)
            StartCoroutine(AutoDismiss(prompt.displayDuration));
    }

    private IEnumerator AutoDismiss(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isShowing)
            DismissCurrent();
    }

    private void DismissCurrent()
    {
        if (currentPromptIndex >= prompts.Length) return;

        if (prompts[currentPromptIndex].promptCanvas != null)
            prompts[currentPromptIndex].promptCanvas.SetActive(false);

        isShowing = false;
        currentPromptIndex++;
        ShowNextPrompt();
    }

    private void CompleteTutorial()
    {
        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }
}
