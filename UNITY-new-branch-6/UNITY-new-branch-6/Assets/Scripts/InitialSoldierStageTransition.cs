using UnityEngine;
using System.Collections;

public class InitialSoldierStageTransition : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Vector3 bossSpawnPosition = new Vector3(-3f, -3f, 0f);
    public float fadeDuration = 1.2f;
    public float delayAfterDeath = 1.0f;
    public string bossStageTitle = "1-4: Boss level";

    [Header("Stage Separation")]
    [Tooltip("Optional objects that belong only to the laboratory side and should be hidden after the miniboss dies.")]
    public GameObject[] laboratoryStageObjects;
    [Tooltip("Optional objects that belong only to the boss side and should stay hidden until the miniboss dies.")]
    public GameObject[] bossStageObjects;
    public bool hideBossStageUntilTransition = true;
    public string initialStageParentName = "InitialStageParent";

    private EnemyHealth enemyHealth;
    private bool transitionTriggered = false;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        // When attached to a door, no EnemyHealth is expected — that's fine.

        if (hideBossStageUntilTransition)
        {
            SetStageObjectsActive(bossStageObjects, false);
        }
    }

    private void Update()
    {
        // Monitor the EnemyHealth component; if enabled turns false (died) or HP is <= 0
        if (enemyHealth != null && !transitionTriggered)
        {
            // EnemyHealth disables its own script component upon death
            if (!enemyHealth.enabled)
            {
                TriggerTransition();
            }
        }
    }

    public void TriggerTransition()
    {
        if (transitionTriggered)
            return;

        transitionTriggered = true;
        StartCoroutine(PerformTransition());
    }

    private IEnumerator PerformTransition()
    {
        Debug.Log("Initial soldier defeated! Starting boss level transition...");
        
        // Wait for death animation to start/complete slightly
        yield return new WaitForSeconds(delayAfterDeath);

        if (HasActiveSoldiersRemaining())
        {
            transitionTriggered = false;
            yield break;
        }

        // Start Screen Fade Out
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.StartFadeOut(fadeDuration);
        }
        else
        {
            Debug.LogWarning("ScreenFader.Instance not found! Proceeding with teleportation immediately.");
        }

        yield return new WaitForSeconds(fadeDuration);

        SetStageObjectsActive(bossStageObjects, true);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ActivateBossStage();
        }

        // Teleport the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Vector3 targetPosition = bossSpawnPosition;
            GameObject door2 = GameObject.Find("DOOR2");
            if (door2 != null)
            {
                targetPosition = door2.transform.position;
            }
            else
            {
                Debug.LogWarning("DOOR2 GameObject not found! Falling back to bossSpawnPosition.");
            }

            playerObj.transform.position = targetPosition;
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetMovementAfterTeleport();
            }
            else
            {
                Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 3f;
                    rb.linearVelocity = Vector2.zero;
                }
            }

            Debug.Log($"Teleported player to position: {targetPosition}");
        }
        else
        {
            Debug.LogError("Player GameObject not found! Cannot teleport.");
        }

        // Wait a tiny frame for Cinemachine camera to snap
        yield return new WaitForSeconds(0.1f);

        if (BossCameraController.Instance != null)
        {
            BossCameraController.Instance.UnlockBossStage();
        }
        else
        {
            Debug.LogWarning("BossCameraController.Instance not found! Boss music/camera may not start after teleport.");
        }

        // Start Screen Fade In
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.StartFadeIn(fadeDuration);
        }

        // Show boss level title popup
        GameObject bossTitleGO = GameObject.Find("BossLevelTitleText");
        if (bossTitleGO != null)
        {
            SceneTitleDisplay display = bossTitleGO.GetComponent<SceneTitleDisplay>();
            if (display != null)
            {
                display.ShowTitle(bossStageTitle);
            }
        }
        else
        {
            Debug.LogWarning("BossLevelTitleText GameObject not found! Cannot show boss title.");
        }

        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.UpdateUI();
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("LevelManager.Instance not found! Boss stage objectives may not track correctly.");
        }

        yield return new WaitForSeconds(fadeDuration);
    }

    private void SetStageObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void HideLaboratoryStage()
    {
        SetStageObjectsActive(laboratoryStageObjects, false);

        if (laboratoryStageObjects != null && laboratoryStageObjects.Length > 0)
            return;

        GameObject initialStage = GameObject.Find(initialStageParentName);
        if (initialStage != null)
            initialStage.SetActive(false);
    }

    private bool HasActiveSoldiersRemaining()
    {
        SoldierAI[] soldiers = FindObjectsByType<SoldierAI>(FindObjectsSortMode.None);
        foreach (SoldierAI soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeInHierarchy)
            {
                EnemyHealth eh = soldier.GetComponent<EnemyHealth>();
                if (eh != null && eh.enabled)
                    return true;
            }
        }

        return false;
    }
}
