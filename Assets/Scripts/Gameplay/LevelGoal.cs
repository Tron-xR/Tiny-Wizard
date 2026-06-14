using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelGoal : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private GameObject winCanvas;

    [Header("Settings")]
    [SerializeField] private float winDelay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        PauseManager pause = FindFirstObjectByType<PauseManager>();
        if (pause != null)
            pause.enabled = false;

        if (winCanvas != null)
            winCanvas.SetActive(true);

        Time.timeScale = 0.5f;

        yield return new WaitForSecondsRealtime(winDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
