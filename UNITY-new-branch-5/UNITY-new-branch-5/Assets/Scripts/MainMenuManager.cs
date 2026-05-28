using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Level Selection")]
    public GameObject levelSelectionModal;

    [Header("Audio")]
    public AudioClip menuMusic;
    public AudioClip clickSound;

    void Start()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);

        UIEventSystemBootstrap.EnsureExists();
        SetupMenuAudio();
        HideLevelSelection();
    }

    void SetupMenuAudio()
    {
        GameObject audioObject = GameObject.Find("UIAudioHelper");
        if (audioObject == null)
            audioObject = new GameObject("UIAudioHelper");

        UIAudioHelper audioHelper = audioObject.GetComponent<UIAudioHelper>();
        if (audioHelper == null)
            audioHelper = audioObject.AddComponent<UIAudioHelper>();

        audioHelper.Configure(menuMusic, clickSound);
        audioHelper.PlayMenuMusic();
        audioHelper.WireButtonsUnder(transform);
    }

    public void StartGame()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.StartNewGame();
        }
        else
        {
            // Fallback if no GameFlowManager is in scene (for testing)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Cutscene");
        }
    }

    public void ShowLevelSelection()
    {
        if (levelSelectionModal != null)
        {
            levelSelectionModal.SetActive(true);
        }
    }

    public void HideLevelSelection()
    {
        if (levelSelectionModal != null)
        {
            levelSelectionModal.SetActive(false);
        }
    }

    public void LoadForest()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Forest);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Forest");
    }

    public void LoadCity()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.City);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("City");
    }

    public void LoadLaboratory()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Laboratory);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Laboratory");
    }

    public void LoadGame()
    {
        ShowLevelSelection();
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
