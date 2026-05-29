using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public enum ObjectiveMode
    {
        EliminateEnemies = 0,
        PressButtons = 1,
        Forest = 2
    }

    private const string ForestSceneName = "Forest";

    [Header("Level Settings")]
    public string nextSceneName = "City";
    public float transitionDelay = 2f;

    [Header("Enemy Objective")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Forest Gate")]
    [SerializeField] private bool requireBossStageInForest = false;
    public bool bossStageActive = false;

    [Header("Objective Mode")]
    [SerializeField] private ObjectiveMode objectiveMode = ObjectiveMode.EliminateEnemies;

    [Header("Button Objective")]
    [SerializeField] private int requiredButtons = 4;
    [SerializeField] private int pressedButtons = 0;

    private bool objectivesActive = true;
    private bool isTransitioning = false;

    private float initializationTime;
    private readonly float minTimeBeforeCheck = 2f;

    public bool ObjectivesActive => objectivesActive;
    public ObjectiveMode CurrentObjectiveMode => objectiveMode;
    public int RequiredButtons => requiredButtons;
    public int PressedButtons => pressedButtons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        initializationTime = Time.time;
        ConfigureForestModeForScene();

        Debug.Log(
            $"LevelManager started in {SceneManager.GetActiveScene().name}. " +
            $"Mode={objectiveMode}. Active={objectivesActive}. " +
            $"BossActive={bossStageActive}. Enemies={GetRemainingEnemyCount()}"
        );

        UpdateObjectiveHUD();
    }

    private void Update()
    {
        if (isTransitioning) return;
        if (Time.time - initializationTime < minTimeBeforeCheck) return;
        if (!objectivesActive) return;

        if (ShouldWaitForBossStage())
            return;

        if (IsObjectiveComplete())
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private bool ShouldWaitForBossStage()
    {
        bool isForest = SceneManager.GetActiveScene().name == ForestSceneName;
        return isForest && requireBossStageInForest && !bossStageActive;
    }

    private void ConfigureForestModeForScene()
    {
        if (objectiveMode != ObjectiveMode.Forest)
        {
            objectivesActive = true;
            return;
        }

        if (SceneManager.GetActiveScene().name != ForestSceneName)
        {
            objectiveMode = ObjectiveMode.EliminateEnemies;
            objectivesActive = true;
            Debug.LogWarning($"[{nameof(LevelManager)}] Forest mode only works in '{ForestSceneName}'. Using EliminateEnemies.");
            return;
        }

        objectivesActive = false;
    }

    public bool TryActivateForestObjectives()
    {
        if (objectiveMode != ObjectiveMode.Forest || objectivesActive)
            return false;

        objectivesActive = true;
        
        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.OnObjectivesActivated();
        }
        else
        {
            UpdateObjectiveHUD();
        }

        Debug.Log($"[{nameof(LevelManager)}] Forest objectives activated.");
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleStartObjectivesCollision(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleStartObjectivesCollision(other.gameObject);
    }

    private void HandleStartObjectivesCollision(GameObject collidedObj)
    {
        if (gameObject.name == "StartObjectives" && (collidedObj.CompareTag("Player") || collidedObj.GetComponentInParent<PlayerHealth>() != null))
        {
            TryActivateForestObjectives();
        }
        else if (gameObject.CompareTag("Player") && collidedObj.name == "StartObjectives")
        {
            TryActivateForestObjectives();
        }
    }

    public void ActivateBossStage()
    {
        bossStageActive = true;
        UpdateObjectiveHUD();

        Debug.Log("[LevelManager] Boss stage activated.");
    }

    public void BeginButtonObjective(int required = 4)
    {
        objectiveMode = ObjectiveMode.PressButtons;
        requiredButtons = Mathf.Max(1, required);
        pressedButtons = 0;

        UpdateObjectiveHUD();
    }

    public void RegisterButtonPressed()
    {
        if (objectiveMode != ObjectiveMode.PressButtons)
            return;

        pressedButtons = Mathf.Clamp(pressedButtons + 1, 0, requiredButtons);
        UpdateObjectiveHUD();
    }

    public bool IsObjectiveComplete()
    {
        if (!objectivesActive)
            return false;

        if (objectiveMode == ObjectiveMode.PressButtons)
            return false; // Under PressButtons mode, the scene transition is handled by the door GameObject, not automatically by LevelManager.

        return GetRemainingEnemyCount() <= 0;
    }

    public int GetRemainingEnemyCount()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        int count = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
                count++;
        }

        return count;
    }

    private IEnumerator TransitionToNextLevel()
    {
        Debug.Log($"Objective complete. Checking again before loading {nextSceneName}...");

        yield return new WaitForSeconds(1f);

        if (!IsObjectiveComplete())
        {
            Debug.Log("Transition cancelled. Objective is no longer complete.");
            isTransitioning = false;
            yield break;
        }

        Debug.Log($"Level Complete! Starting fade-out to {nextSceneName}.");

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.StartFadeOut(transitionDelay);
        }
        else
        {
            Debug.LogWarning("ScreenFader.Instance not found. Loading scene without fade.");
        }

        yield return new WaitForSeconds(transitionDelay);

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log($"Loading next scene: {nextSceneName}");

        if (GameFlowManager.Instance == null)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        switch (nextSceneName)
        {
            case "MainMenu":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.MainMenu);
                break;

            case "Cutscene":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Cutscene);
                break;

            case "Forest":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Forest);
                break;

            case "City":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.City);
                break;

            case "Laboratory":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Laboratory);
                break;

            case "Dialog":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Dialog);
                break;

            case "Credits":
                GameFlowManager.Instance.JumpToState(GameFlowManager.GameState.Credits);
                break;

            default:
                SceneManager.LoadScene(nextSceneName);
                break;
        }
    }

    private void UpdateObjectiveHUD()
    {
        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }
}