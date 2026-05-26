using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public enum GameState { MainMenu, Cutscene, Forest, City, Laboratory, Dialog, Credits }
    public GameState currentState = GameState.MainMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        currentState = GameState.Cutscene;
        LoadCurrentState();
    }

    public void CompleteCurrentState()
    {
        switch (currentState)
        {
            case GameState.Cutscene: currentState = GameState.Forest; break;
            case GameState.Forest: currentState = GameState.City; break;
            case GameState.City: currentState = GameState.Laboratory; break;
            case GameState.Laboratory: currentState = GameState.Dialog; break;
            case GameState.Dialog: currentState = GameState.Credits; break;
            case GameState.Credits: currentState = GameState.MainMenu; break;
        }
        LoadCurrentState();
    }

    public void JumpToState(GameState state)
    {
        currentState = state;
        LoadCurrentState();
    }

    public void LoadCurrentState()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                SceneManager.LoadScene("MainMenu");
                break;
            case GameState.Cutscene:
            case GameState.Dialog:
                SceneManager.LoadScene("Cutscene");
                break;
            case GameState.Forest:
                SceneManager.LoadScene("Forest");
                break;
            case GameState.City:
                SceneManager.LoadScene("City");
                break;
            case GameState.Laboratory:
                SceneManager.LoadScene("Laboratory");
                break;
            case GameState.Credits:
                SceneManager.LoadScene("Credits");
                break;
        }
    }
    
    public void QuitToMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
