using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    public string nextSceneName = "City";
    public float transitionDelay = 2f;

    private bool isTransitioning = false;

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
        // Log the initial state to debug why transition might start or not start
        Debug.Log($"LevelManager started in {SceneManager.GetActiveScene().name}. Objectives met: {AreAllObjectivesEliminated()}");
    }

    private void Update()
    {
        if (isTransitioning || Time.time - initializationTime < minTimeBeforeCheck) return;

        // Check for enemies
        if (AreAllObjectivesEliminated())
        {
            // Optional: If you want to ensure there WERE enemies before finishing
            // we could add a check here. But usually, if objectives are 0, level is done.
            
            // To prevent instant transition on empty levels, we could check a timer or flag
            // But let's assume if objectives are met, we start.
            
            isTransitioning = true;
            StartCoroutine(TransitionToNextLevel());
        }
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
        
        // Re-verify after the wait
        if (!AreAllObjectivesEliminated()) 
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
