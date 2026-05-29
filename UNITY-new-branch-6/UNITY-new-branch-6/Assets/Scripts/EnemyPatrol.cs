using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform attackPoint;

    [Header("Patrol Settings")]
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 3f;

    [Tooltip("-1 = start moving left, 1 = start moving right")]
    public int startingDirection = -1;

    [Header("Detection Settings")]
    public float detectionRange = 6f;
    public float chaseSpeed = 2.5f;

    [Header("Attack Settings")]
    public float attackRange = 3.5f;
    public float attackHitRadius = 1.2f;
    public int attackDamage = 1;
    public float attackCooldown = 1.2f;

    [Header("Bear Facing")]
    public bool bearFacesRightByDefault = false;

    private Vector3 startPosition;
    private int patrolDirection;

    private bool isAttacking;
    private bool canAttack = true;
    private bool dealtDamageThisAttack;
    private float attackPointStartX;

    public BoxCollider2D plyrCollider;

    public SoundManager soundManager;

    void Awake()
    {
        soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

        void Start()
    {
        startPosition = transform.position;

        patrolDirection = startingDirection >= 0 ? 1 : -1;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (attackPoint != null)
            attackPointStartX = Mathf.Abs(attackPoint.localPosition.x);

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (plyrCollider == null && player != null)
            plyrCollider = player.GetComponent<BoxCollider2D>();

        ApplyFacing(patrolDirection);
    }

    void Update()
    {
        if (player == null || !enabled) return;

        if (isAttacking)
{
            SetWalking(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        SetWalking(true);

        transform.position += Vector3.right * patrolDirection * patrolSpeed * Time.deltaTime;

        float distanceFromStart = transform.position.x - startPosition.x;

        if (distanceFromStart >= patrolDistance)
        {
            patrolDirection = -1;
            ApplyFacing(patrolDirection);
        }
        else if (distanceFromStart <= -patrolDistance)
        {
            patrolDirection = 1;
            ApplyFacing(patrolDirection);
        }
    }

    void ChasePlayer()
    {
            
        SetWalking(true);
        

        float directionToPlayer = player.position.x - transform.position.x;
        int chaseDirection = directionToPlayer > 0 ? 1 : -1;

        ApplyFacing(chaseDirection);

        transform.position += Vector3.right * chaseDirection * chaseSpeed * Time.deltaTime;
    }

    void TryAttack()
    {
        SetWalking(false);

        float directionToPlayer = player.position.x - transform.position.x;
        int attackDirection = directionToPlayer > 0 ? 1 : -1;

        ApplyFacing(attackDirection);

        if (canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;
        dealtDamageThisAttack = false;

        Debug.Log("Bear attack triggered");

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");

            // Let the animation event handle the damage timing.
            // Just wait for the full cooldown of the attack.
            yield return new WaitForSeconds(attackCooldown);
        }
        else
        {
            // Fallback if there is no animator component:
            // Deal damage after a short delay, then wait for the rest of the cooldown.
            yield return new WaitForSeconds(attackCooldown * 0.45f);
            DealBearAttackDamage();
            yield return new WaitForSeconds(attackCooldown * 0.55f);
        }

        isAttacking = false;
        canAttack = true;
    }

    // Called by BearAttack animation event
    public void DealBearAttackDamage()
    {
        if (dealtDamageThisAttack) return;
        dealtDamageThisAttack = true;

        Debug.Log("Bear attack event fired");

        if (attackPoint == null)
        {
            Debug.LogWarning("BearAttackPoint is missing.");
            return;
        }

        // Check hits at both the forward attackPoint AND the bear's body center to hit the player if they stand inside the bear
        System.Collections.Generic.List<Collider2D> hitsList = new System.Collections.Generic.List<Collider2D>(
            Physics2D.OverlapCircleAll(attackPoint.position, attackHitRadius)
        );
        hitsList.AddRange(Physics2D.OverlapCircleAll(transform.position, attackHitRadius * 1.2f));

        foreach (Collider2D hit in hitsList)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Bear damaged player");
                soundManager.PlaySFX(soundManager.bearAttack);
                return;
            }
        }
    }

    void ApplyFacing(int direction)
    {
        if (spriteRenderer != null)
        {
            if (bearFacesRightByDefault)
            {
                spriteRenderer.flipX = direction < 0;
            }
            else
            {
                spriteRenderer.flipX = direction > 0;
            }
        }

        if (attackPoint != null)
        {
            Vector3 pos = attackPoint.localPosition;
            pos.x = attackPointStartX * direction;
            attackPoint.localPosition = pos;
        }
    }

    void SetWalking(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", walking);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRadius);
        }
    }
}