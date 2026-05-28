using UnityEngine;
using System.Collections;

public class SoldierAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 20f;
    public float patrolRange = 10f;
    public float movementSmoothing = 0.1f;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private Transform player;
    private float currentVelocityX;

    [Header("Attack Settings")]
    public float attackRange = 8.0f;
    public float attackCooldown = 2.0f;
    public int attackDamage = 1;
    public Transform attackPoint;
    public float attackCircleRadius = 3.0f;
    public LayerMask playerLayer;
    public bool canMoveDuringAttack = false;
    private float lastAttackTime;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;

    void Start()
    {
        startPosition = transform.position;
        SetNewPatrolTarget();
        
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();

        // Force Player layer mask to 10 to avoid hitting non-player objects
        playerLayer = 1 << 10;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.enabled == false) return;
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        // Ensure player reference
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            if (player == null) return;
        }

        // SYNC FLIP: Only face the player if not currently attacking or transitioning to attack
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttacking = stateInfo.IsName("Attack") || animator.IsInTransition(0);
        
        if (!isAttacking)
        {
            UpdateFacingDirection();
            animator.speed = 1.0f; // Reset speed when not attacking
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);

        // Determine behavior
        if (horizontalDistance <= attackRange && Mathf.Abs(transform.position.y - player.position.y) < 5f)
        {
            Attack();
        }
else if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            ChasePlayer();
        }
        else if (!isAttacking)
        {
            Patrol();
        }

        UpdateAnimation();
    }

    void Attack()
    {
        // SYNC MOVEMENT: Track player movement during the windup/attack
        bool isAttackingAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        if (player != null)
        {
            float lungeDirection = (player.position.x > transform.position.x) ? 1f : -1f;
            
            if (isAttackingAnimation)
            {
                // Active lunge during swing
                rb.linearVelocity = new Vector2(lungeDirection * 1.5f, rb.linearVelocity.y);
                animator.speed = 1.2f; // Slight speed up for impact feel
            }
            else
            {
                // Slow down during windup to stay close
                rb.linearVelocity = new Vector2(lungeDirection * 0.5f, rb.linearVelocity.y);
            }
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
    }

    public void DealDamage()
    {
        if (player == null) return;

        // SYNC ATTACK: Dynamically calculate the damage point towards the player's actual X position
        // but clamp it within the attack range to maintain fairness.
        float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, attackRange);
        Vector3 damageCenter = new Vector3(targetX, transform.position.y, transform.position.z);

        // Use OverlapCircleAll to ensure we catch the player even if they have multiple colliders
        Collider2D[] hits = Physics2D.OverlapCircleAll(damageCenter, attackCircleRadius, playerLayer);
        foreach (var hit in hits)
        {
            // Try to find PlayerHealth on the hit object or its parents
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                break; // Damage only once per event
            }
        }
    }

    void Patrol()
    {
        MoveTo(patrolTarget, moveSpeed);
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            SetNewPatrolTarget();
        }
    }

    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        patrolTarget = new Vector3(startPosition.x + randomX, transform.position.y, transform.position.z);
    }

    void ChasePlayer()
    {
        if (player == null) return;
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, transform.position.z);
        MoveTo(targetPos, chaseSpeed);
    }

    void MoveTo(Vector3 target, float speed)
    {
        float targetVelocityX = (target.x > transform.position.x) ? speed : -speed;
        
        // Smoother Velocity Transition
        float newVelocityX = Mathf.SmoothDamp(rb.linearVelocity.x, targetVelocityX, ref currentVelocityX, movementSmoothing);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);

        // Face player/movement direction smoothly
        UpdateFacingDirection();
    }

    void UpdateFacingDirection()
    {
        if (player != null)
        {
            // Direct flip based on player position for perfect synchronization
            Vector3 scale = transform.localScale;
            if (player.position.x < transform.position.x)
            {
                scale.x = -Mathf.Abs(scale.x);
            }
            else
            {
                scale.x = Mathf.Abs(scale.x);
            }
            transform.localScale = scale;
        }
    }

    // Removed UpdateAttackPointFlip as localScale handles child flipping automatically

    void UpdateAnimation()
    {
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackCircleRadius);
        }
    }
}


