using UnityEngine;

public class CrystalEnemyAI : MonoBehaviour
{
    [Header("Health")]
    public int coreHealth = 3;
    public int shieldHealth = 8;
    public int maxShieldHealth = 8;
    private int maxCoreHealth;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float checkDistance = 1f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isDead;
    private bool shieldActive = true;
    private Color defaultColor = Color.white;

    private EnemyWorldHealthBar worldHealthBar;

    void Start()
    {
        maxCoreHealth = coreHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            defaultColor = spriteRenderer.color;

        if (CrystalBossController.Instance != null)
            shieldActive = CrystalBossController.Instance.IsCrystalShielded;
        else
            shieldActive = true;

        UpdateShieldVisual();

        // Create world-space health bar
        worldHealthBar = gameObject.AddComponent<EnemyWorldHealthBar>();
        worldHealthBar.offset = new Vector3(0f, 2.0f, 0f);
        worldHealthBar.barWidth = 1.4f;
        worldHealthBar.barHeight = 0.2f;
        worldHealthBar.fillColor = new Color(0.3f, 0.7f, 1f, 1f); // Cyan for shielded
        worldHealthBar.lowHealthColor = new Color(1f, 0.3f, 0.3f, 1f);
        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isPaused))
            return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, checkDistance, groundLayer);
        _ = hit.collider != null;
    }

    public void SetShieldActive(bool active)
    {
        shieldActive = active;
        if (active)
            shieldHealth = maxShieldHealth;

        UpdateShieldVisual();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead)
            return;

        if (shieldActive)
        {
            shieldHealth -= damage;
            if (shieldHealth < 0)
                shieldHealth = 0;
            UpdateHealthBar();
            return;
        }

        coreHealth -= damage;
        UpdateHealthBar();
        if (coreHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (animator != null)
        {
            animator.speed = 1;
            animator.SetTrigger("Die");
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
            col.enabled = false;

        // Hide the health bar on death
        if (worldHealthBar != null)
            worldHealthBar.Hide();

        enabled = false;

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();

        if (CrystalBossController.Instance != null)
            CrystalBossController.Instance.NotifyCrystalDestroyed();

        Destroy(gameObject, 1.5f);
    }

    void UpdateShieldVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = shieldActive
            ? new Color(0.65f, 0.85f, 1f, 1f)
            : new Color(1f, 0.55f, 0.55f, 1f);
    }

    void UpdateHealthBar()
    {
        if (worldHealthBar == null) return;

        if (shieldActive)
        {
            worldHealthBar.fillColor = new Color(0.3f, 0.7f, 1f, 1f); // Cyan for shield
            worldHealthBar.SetHealth(shieldHealth, maxShieldHealth);
        }
        else
        {
            worldHealthBar.fillColor = new Color(1f, 0.55f, 0.1f, 1f); // Orange for core
            worldHealthBar.SetHealth(coreHealth, maxCoreHealth);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);
    }
}
