using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public AudioClip clickSound;
    private bool isPaused = false;

    void Start()
    {
        UIEventSystemBootstrap.EnsureExists();

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        
        isPaused = false;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);

        EnsureUIAudio();
    }

    void EnsureUIAudio()
    {
        if (UIAudioHelper.Instance != null)
            return;

        GameObject audioObject = new GameObject("UIAudioHelper");
        UIAudioHelper helper = audioObject.AddComponent<UIAudioHelper>();
        helper.Configure(null, clickSound);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed. isPaused: " + isPaused);
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void SetAllAnimatorsSpeed(float speed)
    {
        Animator[] allAnimators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator anim in allAnimators)
        {
            anim.speed = speed;
        }
    }

    public void Resume()
    {
        Debug.Log("Resuming game...");
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        
        Time.timeScale = 1f;
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
        
        SetAllAnimatorsSpeed(1f);
        isPaused = false;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Pause()
    {
        Debug.Log("Pausing game...");
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        if (UIAudioHelper.Instance != null && pauseMenuUI != null)
            UIAudioHelper.Instance.WireButtonsUnder(pauseMenuUI.transform);
        
        Time.timeScale = 0f;
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);
        
        SetAllAnimatorsSpeed(0f);
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.QuitToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
