using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private HealthBar healthBar;

    private int currentHealth;
    public SoundManager soundManager;
    private bool fallbackTransitionStarted;

    void Awake()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            soundManager = audioObj.GetComponent<SoundManager>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        Debug.Log($"{gameObject.name} took {damage} damage. HP left: {currentHealth}");

        // Damage Flash
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(DamageFlash(sr));
         
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("Damage");
        }

        if (currentHealth <= 0)
        {
            if (soundManager != null)

                if (!gameObject.name.ToLower().Contains("owl"))
                {
                    soundManager.PlaySFX(soundManager.growl);
                }else
                {
                    soundManager.PlaySFX(soundManager.owlDeath);
                }
                    Die();
        }
    }

    private IEnumerator DamageFlash(SpriteRenderer sr)
    {
        if(gameObject.name != "Soldier")
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
        else
        {
            Color originalColor = Color.white;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} defeated.");

        if (gameObject.name.ToLower().Contains("owl"))
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            

            foreach (SpriteRenderer sr in renderers)
            {
                sr.color = Color.black;
                StartCoroutine(DeadOwl());
            }
        }

        

        InitialSoldierStageTransition initialStageTransition = GetComponent<InitialSoldierStageTransition>();
        // Automatic transition is disabled; player must interact with the door to transition.
        /*
        if (initialStageTransition != null)
        {
            initialStageTransition.TriggerTransition();
        }
        else if (ShouldUseLaboratorySoldierFallback())
        {
            fallbackTransitionStarted = true;
            StartCoroutine(FallbackLaboratoryBossTransition());
        }
        */
        
        // Prevent falling
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
        }

        Animator animator = GetComponent<Animator>();
        bool hasDieAnimation = false;
        
        if (animator != null)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "Die")
                {
                    hasDieAnimation = true;
                    break;
                }
            }
        }

        if (hasDieAnimation)
        {
            animator.SetTrigger("Die");
            DisableEnemy();
            Destroy(gameObject, (initialStageTransition != null || fallbackTransitionStarted) ? 8f : 2f); // Keep transition host alive long enough for fade/teleport.
        }
        else
        {
            DisableEnemy();
            if (initialStageTransition != null || fallbackTransitionStarted)
                Destroy(gameObject, 8f);
            else
                Destroy(gameObject);
        }
    }

    private IEnumerator DeadOwl()
    { 
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private bool ShouldUseLaboratorySoldierFallback()
    {
        return SceneManager.GetActiveScene().name == "Laboratory"
            && GetComponent<SoldierAI>() != null
            && gameObject.activeInHierarchy;
    }

    private IEnumerator FallbackLaboratoryBossTransition()
    {
        const float fadeDuration = 1.2f;
        Vector3 bossSpawnPosition = new Vector3(24f, -2.35f, 0f);

        yield return new WaitForSeconds(1f);

        if (HasActiveSoldiersRemaining())
        {
            fallbackTransitionStarted = false;
            yield break;
        }

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.StartFadeOut(fadeDuration);

        yield return new WaitForSeconds(fadeDuration);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObj.transform.position = bossSpawnPosition;

            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetMovementAfterTeleport();
            }
            else
            {
                Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.gravityScale = 3f;
                    playerRb.linearVelocity = Vector2.zero;
                }
            }
        }

        if (LevelManager.Instance != null)
            LevelManager.Instance.ActivateBossStage();

        if (BossCameraController.Instance != null)
            BossCameraController.Instance.UnlockBossStageAndEnter();

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.StartFadeIn(fadeDuration);
    }

    private bool HasActiveSoldiersRemaining()
    {
        SoldierAI[] soldiers = FindObjectsByType<SoldierAI>(FindObjectsSortMode.None);
        foreach (SoldierAI soldier in soldiers)
        {
            if (soldier != null && soldier.enabled && soldier.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }

    private void DisableEnemy()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders) col.enabled = false;
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this && !(script is InitialSoldierStageTransition)) script.enabled = false;
        }
        
        // Disable this script too so it's not counted by HUD/LevelManager
        this.enabled = false;

        // Update the Objective HUD if it exists
        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.UpdateUI();
        }
    }
}
