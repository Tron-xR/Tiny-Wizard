using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    public static bool IsPaused { get; private set; }
    public System.Action OnPaused;
    public System.Action OnResumed;

    private PlayerInputHandler inputHandler;

    private void OnEnable()
    {
        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<PlayerInputHandler>();

        if (inputHandler != null)
            inputHandler.PausePressed += TogglePause;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.PausePressed -= TogglePause;
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPaused?.Invoke();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnResumed?.Invoke();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
