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
        /// <summary>Forest only: objectives stay hidden until StartObjective is crossed.</summary>
        Forest = 2
    }

    const string ForestSceneName = "Forest";

    [Header("Level Settings")]
    public string nextSceneName = "City";
    public float transitionDelay = 2f;

    private bool isTransitioning = false;

    [Header("Objective Mode")]
    [SerializeField] private ObjectiveMode objectiveMode = ObjectiveMode.EliminateEnemies;

    [Header("Button Objective (PressButtons mode)")]
    [SerializeField] private int requiredButtons = 4;
    [SerializeField] private int pressedButtons = 0;

    private bool objectivesActive = true;

    public bool ObjectivesActive => objectivesActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private float initializationTime;
    private float minTimeBeforeCheck = 2f;

    private void Start()
    {
        initializationTime = Time.time;
        ConfigureForestModeForScene();
        Debug.Log($"LevelManager started in {SceneManager.GetActiveScene().name}. Mode={objectiveMode}. Active={objectivesActive}. Complete={IsObjectiveComplete()}");
    }

    void ConfigureForestModeForScene()
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

    /// <summary>Called by StartObjective trigger in the Forest scene.</summary>
    public bool TryActivateForestObjectives()
    {
        if (objectiveMode != ObjectiveMode.Forest || objectivesActive)
            return false;

        objectivesActive = true;

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.OnObjectivesActivated();

        Debug.Log($"[{nameof(LevelManager)}] Forest objectives activated.");
        return true;
    }

    private void Update()
    {
        if (isTransitioning || Time.time - initializationTime < minTimeBeforeCheck) return;
        if (!objectivesActive) return;

        if (IsObjectiveComplete())
        {
            // Optional: If you want to ensure there WERE enemies before finishing
            // we could add a check here. But usually, if objectives are 0, level is done.
            
            // To prevent instant transition on empty levels, we could check a timer or flag
            // But let's assume if objectives are met, we start.
            
            isTransitioning = true;
            StartCoroutine(TransitionToNextLevel());
        }
    }

    public ObjectiveMode CurrentObjectiveMode => objectiveMode;

    public int RequiredButtons => requiredButtons;
    public int PressedButtons => pressedButtons;

    public void BeginButtonObjective(int required = 4)
    {
        objectiveMode = ObjectiveMode.PressButtons;
        requiredButtons = Mathf.Max(1, required);
        pressedButtons = 0;

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    public void RegisterButtonPressed()
    {
        if (objectiveMode != ObjectiveMode.PressButtons)
            return;

        pressedButtons = Mathf.Clamp(pressedButtons + 1, 0, requiredButtons);

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    public bool IsObjectiveComplete()
    {
        if (!objectivesActive)
            return false;

        if (objectiveMode == ObjectiveMode.PressButtons)
            return pressedButtons >= requiredButtons;

        return AreAllObjectivesEliminated();
    }

    bool AreAllObjectivesEliminated()
    {
        // Check for all possible enemy types
        if (Object.FindObjectsByType<SoldierAI>(FindObjectsSortMode.None).Length > 0) return false;
        if (Object.FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None).Length > 0) return false;
        if (Object.FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None).Length > 0) return false;
        if (Object.FindObjectsByType<OwlAI>(FindObjectsSortMode.None).Length > 0) return false;
        
        var allHealth = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var enemy in allHealth)
        {
            if (enemy.enabled && enemy.name.ToLower().Contains("bear")) return false;
        }

        return true;
    }

    IEnumerator TransitionToNextLevel()
    {
        // Wait a short moment to ensure death animations/events have finished
        yield return new WaitForSeconds(1.0f);
        
        if (!IsObjectiveComplete())
        {
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
            Debug.LogWarning("ScreenFader.Instance not found for transition!");
        }

        yield return new WaitForSeconds(transitionDelay);

        Debug.Log($"Loading next scene via GameFlowManager.");
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.CompleteCurrentState();
        }
        else
        {
            Debug.LogWarning("GameFlowManager instance not found. Falling back to SceneManager.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
