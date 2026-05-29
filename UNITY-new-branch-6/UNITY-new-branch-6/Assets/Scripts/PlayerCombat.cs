using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayer;
    public int damage = 1;
    public float attackCooldown = 0.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Block Settings")]
    [SerializeField] private Color blockColor = new Color(0.5f, 0.8f, 1.0f, 0.9f);
    private bool isBlocking;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;

    public bool IsBlocking => isBlocking;

    private float nextAttackTime;

    [Header("Jump Fallback")]
    public float fallbackJumpForce = 12f;
    public int fallbackMaxJumps = 2;
    public Transform fallbackGroundCheck;
    public float fallbackGroundCheckRadius = 0.25f;
    public LayerMask fallbackGroundLayer;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private int fallbackJumpCount;
    private float fallbackDefaultGravityScale = 3f;
    private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (rb.gravityScale > 0f)
                fallbackDefaultGravityScale = rb.gravityScale;
            else
                rb.gravityScale = fallbackDefaultGravityScale;
        }

        playerController = GetComponent<PlayerController>();

        if (fallbackMaxJumps <= 0)
            fallbackMaxJumps = 2;
        if (fallbackJumpForce <= 0)
            fallbackJumpForce = 12f;
        if (fallbackGroundLayer.value == 0)
            fallbackGroundLayer = (1 << 7) | (1 << 9);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnDisable()
    {
        if (isBlocking)
        {
            isBlocking = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    void Update()
    {
        // Don't process input if dead or game over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Block input (Right Click held down)
        if (Input.GetMouseButton(1))
        {
            if (!isBlocking)
            {
                isBlocking = true;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = blockColor;
                }
            }
        }
        else
        {
            if (isBlocking)
            {
                isBlocking = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        // Attack 1 (Left Click)
        if (!isBlocking && Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            Attack(0);
            nextAttackTime = Time.time + attackCooldown;
        }

        // Attack 2 (F Key or Middle Mouse Click)
        if (!isBlocking && (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(2)) && Time.time >= nextAttackTime)
        {
            Attack(1);
            nextAttackTime = Time.time + attackCooldown;
        }

        HandleJumpFallback();
    }

    void HandleJumpFallback()
    {
        if (playerController != null && playerController.enabled)
            return;

        bool grounded = IsFallbackGrounded();
        if (grounded)
            fallbackJumpCount = 0;

        if (Input.GetKeyDown(KeyCode.Space) && !isBlocking && fallbackJumpCount < fallbackMaxJumps && rb != null)
        {
            if (rb.gravityScale <= 0f)
                rb.gravityScale = fallbackDefaultGravityScale;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fallbackJumpForce);
            fallbackJumpCount++;
            grounded = false;
        }

        if (animator != null)
        {
            animator.SetBool("isGrounded", grounded);
            animator.SetBool("isJumping", !grounded);
        }
    }

    bool IsFallbackGrounded()
    {
        if (fallbackGroundCheck != null
            && Physics2D.OverlapCircle(fallbackGroundCheck.position, fallbackGroundCheckRadius, fallbackGroundLayer))
        {
            return true;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            return false;

        if (playerCollider.IsTouchingLayers(fallbackGroundLayer))
            return true;

        int contactCount = playerCollider.GetContacts(groundContacts);
        for (int i = 0; i < contactCount; i++)
        {
            Collider2D otherCollider = groundContacts[i].collider == playerCollider
                ? groundContacts[i].otherCollider
                : groundContacts[i].collider;

            if (otherCollider != null
                && !otherCollider.isTrigger
                && Mathf.Abs(groundContacts[i].normal.y) > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    void Attack(int step)
    {
        if (animator != null)
        {
            animator.SetInteger("AttackNum", step);
            animator.SetTrigger("isAttacking");
        }
    }

    // Call this using Animation Event
    public void DealAttackDamage()
    {
        Debug.Log("Player attack damage triggered");
        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint is missing.");
            return;
        }

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            // Try different health/AI components
            Enemy e = enemy.GetComponentInParent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                continue;
            }

            EnemyHealth eh = enemy.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
                continue;
            }

            CrystalEnemyAI ce = enemy.GetComponentInParent<CrystalEnemyAI>();
            if (ce != null)
            {
                ce.TakeDamage(damage);
                continue;
            }

            TentacleBossAI tb = enemy.GetComponentInParent<TentacleBossAI>();
            if (tb != null)
            {
                tb.TakeDamage(damage);
                continue;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
