using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Menü sahnenizin adýný buraya yazýn
    }

    public void LoadMainMenuByIndex(int sceneIndex = 0)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void SetPauseState(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }
}
