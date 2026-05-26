using UnityEngine;
using System.Collections;

public class TentacleBossAI : MonoBehaviour
{
    [Header("Health")]
    public int health = 5;
    private int maxHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float followSmoothing = 0.1f;
    private float currentVelocityX;

    [Header("Combat Settings")]
    public float detectionRange = 12f;
    public float attackRange = 8f;
    public float attackCooldown = 2f;
    public float phase1ExposeTime = 3f;
    public float phase2ExposeTime = 1.5f;
    public float attackInterval = 1f;
    public float attackIntervalPhase2 = 0.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private Collider2D tipCollider;
    private Transform player;
    private bool isExposed = false;
    private bool isDead = false;
    private float lastAttackTime;

    public enum BossPhase { Phase1, Phase2, Frenzy }
    public BossPhase currentPhase = BossPhase.Phase1;

    [Header("Visuals")]
    public float baseScale = 2.0f;
    private Vector3 originalScale;

    void Start()
    {
        maxHealth = health;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        
        // Ensure we are on the Enemy layer (8)
        SetLayerRecursive(gameObject, 8);

        // Force Player layer mask to 10 to avoid hitting crystals (Layer 0)
        playerLayer = 1 << 10;

        originalScale = new Vector3(baseScale, baseScale, 1f);
        transform.localScale = originalScale;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (bossCollider != null) bossCollider.isTrigger = true;

        Transform tip = transform.Find("TentacleTip");
        if (tip != null)
        {
            tipCollider = tip.GetComponent<Collider2D>();
            tipCollider.enabled = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackCircleRadius = 2f;
    public LayerMask playerLayer;

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isPaused)) return;

        // CRITICAL: Check if all crystals are destroyed
        if (Object.FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None).Length == 0)
        {
            Die();
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        // SYNC FLIP: Face the player instantly, but NOT during the attack animation
        // This ensures the animation finishes in the direction it started.
        bool isAttackingAnimation = animator != null && (animator.GetCurrentAnimatorStateInfo(0).IsName("TentacleAttack") || animator.IsInTransition(0));
        if (!isAttackingAnimation)
        {
            Vector3 currentScale = transform.localScale;
            if (player.position.x > transform.position.x)
            {
                currentScale.x = baseScale;
            }
            else
            {
                currentScale.x = -baseScale;
            }
            transform.localScale = currentScale;
            
            if (animator != null) animator.speed = 1.0f; // Reset speed
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Attack if in range
        if (distanceToPlayer <= detectionRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    // Called via Animation Event for smooth, accurate damage placement
    public void DealDamage()
    {
        if (player == null) return;

        // SYNC ATTACK: Dynamically calculate the damage point towards the player's actual X position
        // but clamp it within the attack range to maintain fairness.
        float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, attackRange);
        Vector3 damageCenter = new Vector3(targetX, transform.position.y, transform.position.z);

        Collider2D[] hits = Physics2D.OverlapCircleAll(damageCenter, attackCircleRadius, playerLayer);
        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            else
            {
                // Help debug if we are hitting crystals or other objects
                Debug.Log($"Tentacle hit: {hit.gameObject.name} on layer {LayerMask.LayerToName(hit.gameObject.layer)}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackCircleRadius);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (animator != null)
        {
            // Increase animation speed slightly to feel more responsive if needed
            animator.speed = 1.2f;
            animator.SetTrigger("Attack");
        }
    }

    public void ShrinkDown()
    {
        if (isDead) return;
        Die();
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        health -= damage;
        StartCoroutine(HurtFlash());

        if (health <= 0) Die();
        else if (health <= 1) currentPhase = BossPhase.Frenzy;
        else if (health <= 3) currentPhase = BossPhase.Phase2;
    }

    IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = Color.white;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null)
        {
            animator.speed = 1;
            animator.SetTrigger("Die");
        }
        
        if (bossCollider != null) bossCollider.enabled = false;
        if (tipCollider != null) tipCollider.enabled = false;

        // Only exit boss mode if all crystals are destroyed
        if (Object.FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None).Length == 0)
        {
            if (BossCameraController.Instance != null)
                BossCameraController.Instance.ExitBossMode();
        }

        // Disable script so it's not counted by HUD/LevelManager
this.enabled = false;

        // Update the Objective HUD if it exists
        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.UpdateUI();
        }

        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(1);
        }
    }
}

