using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject continueButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "TinyWizard";

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (continueButton != null)
            continueButton.SetActive(SaveManager.HasSave());
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Continue()
    {
        SaveManager.SaveData data = SaveManager.Load();
        if (data != null)
        {
            SceneManager.sceneLoaded += OnSceneLoadedForContinue;
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Play();
        }
    }

    private void OnSceneLoadedForContinue(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForContinue;
        SaveManager.SaveData data = SaveManager.Load();
        SaveManager.ApplySave(data);
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
