using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        Debug.Log($"GameManager: SetPaused({paused}). TimeScale is now {Time.timeScale}");
        
        // Hide/Show mouse if needed
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    [Header("UI References")]
    public GameObject gameOverPanel; // Assign a UI panel in the inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over: " + reason);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // You can add more logic here, like stopping time or disabling input
        // Time.timeScale = 0; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
