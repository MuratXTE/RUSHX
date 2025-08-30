using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    
    private bool isPaused = false;
    
    void Start()
    {
        // Initialize pause menu state
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // Ensure game starts unpaused
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    void Update()
    {
        // Optional: Allow ESC key to toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    /// <summary>
    /// Pauses the game and shows the pause menu
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        Debug.Log("Game Paused");
    }
    
    /// <summary>
    /// Resumes the game and hides the pause menu
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        Debug.Log("Game Resumed");
    }
    
    /// <summary>
    /// Restarts the current scene/level
    /// </summary>
    public void RestartGame()
    {
        // Ensure time scale is reset before reloading
        Time.timeScale = 1f;
        
        // Get current scene name and reload it
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        
        Debug.Log("Game Restarted");
    }
    
    /// <summary>
    /// Loads the main menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        // Ensure time scale is reset before loading main menu
        Time.timeScale = 1f;
        
        // Load main menu scene - update "MainMenu" to your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
        
        Debug.Log("Loading Main Menu");
    }
    
    /// <summary>
    /// Alternative main menu method if you have a different scene name
    /// </summary>
    public void LoadMainMenuByIndex(int sceneIndex = 0)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }
    
    /// <summary>
    /// Quits the application (works in builds, not in editor)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    /// <summary>
    /// Check if game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Force set pause state without UI changes (useful for other scripts)
    /// </summary>
    public void SetPauseState(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }
}
