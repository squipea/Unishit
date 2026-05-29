using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("How many heart icons the HUD shows.")]
    public int heartsCount = 3;

    [Tooltip("How much damage each heart represents. (e.g. 2 = each heart disappears after 2 damage)")]
    public int damagePerHeart = 2;

    public int maxHealth = 6;
    public int currentHealth;

    [Header("UI")]
    public Image healthBarFill;

    [Header("Animation")]
    public Animator animator;

    [Header("Damage Settings")]
    public float invincibleTime = 1f;

    [Header("Death Settings")]
    public float defeatAnimationTime = 1.5f;

    private bool isInvincible;
    private bool isDead;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerCombat playerCombat;
    private BoxCollider2D boxCollider;

    private Vector3 spawnPoint;

    private float originalGravityScale;

    public SoundManager soundManager;
    private PlayerHeartsUI heartsUI;
    private int healthBeforeDamage;

    void Awake()
    {
        soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    void Start()
    {
        // Hearts define total HP.
        maxHealth = Mathf.Max(1, heartsCount) * Mathf.Max(1, damagePerHeart);
        currentHealth = maxHealth;
        spawnPoint = transform.position;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }

        playerController = GetComponent<PlayerController>();
        playerCombat = GetComponent<PlayerCombat>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        UpdateHealthBar();
        heartsUI = PlayerHeartsUI.EnsureExists();
        heartsUI.Initialize(heartsCount, damagePerHeart);
        heartsUI.RefreshStaticDisplay(currentHealth, heartsCount);
        GameOverModalManager.EnsureExists();
        DamageScreenEffect.EnsureExists();
        CameraShake.EnsureExists();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        if (playerCombat != null && playerCombat.IsBlocking)
        {
            Debug.Log("Player blocked " + damage + " damage!");
            CameraShake.EnsureExists().Shake(0.08f, 0.15f);
            return;
        }

        healthBeforeDamage = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        soundManager.PlaySFX(soundManager.ouch);

        Debug.Log("Player HP: " + currentHealth);

        UpdateHealthBar();
        if (heartsUI != null)
        {
            heartsUI.OnHealthChanged(healthBeforeDamage, currentHealth, heartsCount, damagePerHeart);
            heartsUI.PlayDamagePop();
        }

        bool fatal = currentHealth <= 0;
        float shakeDuration = fatal ? 0.22f : 0.16f;
        float shakeMagnitude = fatal ? 0.4f : 0.4f;
        float flashIntensity = fatal ? 1.15f : 1f;

        CameraShake.EnsureExists().Shake(shakeDuration, shakeMagnitude);
        DamageScreenEffect.EnsureExists().PlayDamageFlash(flashIntensity);

        if (fatal)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        PlayDamageAnimation();

        StartCoroutine(InvincibilityRoutine());
    }

    void PlayDamageAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Damage");
        }
    }

    IEnumerator DieRoutine()
    {
        isDead = true;

        Debug.Log("Defeat triggered by PlayerHealth");

        if (GameManager.Instance != null)
             GameManager.Instance.isGameOver = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (playerController != null)
            playerController.enabled = false;

        if (playerCombat != null)
            playerCombat.enabled = false;

        if (animator != null)
        {
            animator.ResetTrigger("Damage");
            animator.SetTrigger("Defeat");
        }

        soundManager.PlaySFX(soundManager.playerDeath);
        yield return new WaitForSecondsRealtime(defeatAnimationTime);

        GameOverModalManager.EnsureExists().Show();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
}