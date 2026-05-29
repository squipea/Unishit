using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovements : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;
    public Animator animator;
    public LayerMask groundLayer;
    [Tooltip("Scene edge walls only (Default layer). Do not include Ground.")]
    [FormerlySerializedAs("obstacleLayer")]
    public LayerMask wallLayer;
    public Transform attackPoint;

    [Header("Movement Settings")]
    public float speed = 8f;
    public float jumpForce = 15f;
    public float groundCheckRadius = 0.3f;
    public float wallCheckSkin = 0.05f;

    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.5f;

    [Header("Combat Settings")]
    public float attackRange = 2.5f;
    public LayerMask enemyLayer;
    public int damage = 1;
    public float comboResetTime = 1f;

    private float inputX;
    private bool isGrounded = false;
    private int jumpCounter = 0;
    private bool isAttacking = false;
    private bool isDashing = false;
    private bool canDash = true;
    private int comboStep = 0;
    private float lastAttackTime;
    private float attackPointLocalX;
    private PhysicsMaterial2D noFrictionMaterial;

    public SoundManager soundManager;

    void Awake()
    {
        soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (animator == null) animator = GetComponent<Animator>();

        if (attackPoint == null)
        {
            var ap = transform.Find("AttackPoint");
            if (ap != null) attackPoint = ap;
            else
            {
                GameObject newAP = new GameObject("AttackPoint");
                newAP.transform.SetParent(transform);
                newAP.transform.localPosition = new Vector3(1.5f, 0, 0);
                attackPoint = newAP.transform;
            }
        }

        attackPointLocalX = Mathf.Abs(attackPoint.localPosition.x);

        // If not set, default to groundLayer so we also block against level geometry (e.g. Squares).
        if (wallLayer.value == 0)
            wallLayer = groundLayer.value != 0 ? groundLayer : (1 << 0);

        noFrictionMaterial = Resources.Load<PhysicsMaterial2D>("NoFriction");
        if (noFrictionMaterial != null)
        {
            if (rb != null)
                rb.sharedMaterial = noFrictionMaterial;
            if (boxCollider != null)
                boxCollider.sharedMaterial = noFrictionMaterial;

            // Also apply to level geometry so corners/edges don't "grab" the player.
            ApplyMaterialToSceneColliders(noFrictionMaterial, groundLayer | wallLayer);
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    static void ApplyMaterialToSceneColliders(PhysicsMaterial2D material, LayerMask layers)
    {
        if (material == null || layers.value == 0)
            return;

        Collider2D[] all = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (Collider2D col in all)
        {
            if (col == null)
                continue;
            if (((1 << col.gameObject.layer) & layers.value) == 0)
                continue;

            col.sharedMaterial = material;
        }
    }

    void Update()
    {
        if (isDashing || (GameManager.Instance != null && (GameManager.Instance.isPaused || GameManager.Instance.isGameOver))) return;

        CheckGrounded();

        if (Keyboard.current != null)
        {
            float targetX = 0;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) targetX = 1;
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) targetX = -1;
            inputX = targetX;

            if ((Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame) && canDash)
            {
                int dashDir = inputX != 0 ? (int)Mathf.Sign(inputX) : (spriteRenderer.flipX ? -1 : 1);
                soundManager.StopRunSFX();
                soundManager.PlaySFX(soundManager.dash);
                StartCoroutine(PerformDash(dashDir));
            }
            
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (isGrounded)
                {
                    PerformJump();
                }
                else if (jumpCounter > 0)
                {
                    
                    PerformJump();
                    jumpCounter--;
                }
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            StartAttack();
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        UpdateAnimator();

        if (inputX > 0.1f)
        {
            spriteRenderer.flipX = false;
            Vector3 pos = attackPoint.localPosition;
            pos.x = attackPointLocalX;
            attackPoint.localPosition = pos;
            
            
        }
        else if (inputX < -0.1f)
        {
            spriteRenderer.flipX = true;
            Vector3 pos = attackPoint.localPosition;
            pos.x = -attackPointLocalX;
            attackPoint.localPosition = pos;
            
        }
        
        else
        {
            soundManager.StopRunSFX();
        }

        if ((inputX > 0.1f || inputX < -0.1f) && isGrounded)
        {
            PlayRunSFX();
        }
        else
        {
            soundManager.StopRunSFX();
        }

    }

    void FixedUpdate()
    {
        if (isDashing) return;
        float desiredX = inputX;
        if (desiredX != 0f)
        {
            int dir = desiredX > 0 ? 1 : -1;
            if (IsBlockedHorizontally(dir))
            {
                desiredX = 0f;
            }
        }
        rb.linearVelocity = new Vector2(desiredX * speed, rb.linearVelocity.y);
    }

    bool IsBlockedHorizontally(int direction)
    {
        if (direction == 0 || boxCollider == null)
            return false;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Use a collider cast so we match the player's actual body shape (more reliable than a few raycasts).
        var filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;

        const int maxHits = 8;
        RaycastHit2D[] hits = new RaycastHit2D[maxHits];

        float distance = Mathf.Max(0.01f, wallCheckSkin + 0.08f);
        int hitCount = boxCollider.Cast(Vector2.right * direction, filter, hits, distance);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider == null || hit.collider.gameObject == gameObject)
                continue;

            // Floors/ceilings have mostly vertical normals; only stop for walls.
            if (Mathf.Abs(hit.normal.y) < 0.65f)
                return true;
        }

        return false;
    }

    void CheckGrounded()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        // Use a small box at the feet
        Vector2 size = new Vector2(boxCollider.size.x * 0.8f, 0.2f);
        Vector2 center = (Vector2)transform.position + boxCollider.offset + new Vector2(0, -boxCollider.size.y / 2f);
        
        Collider2D hit = Physics2D.OverlapBox(center, size, 0, groundLayer);
        isGrounded = hit != null && hit.gameObject != gameObject;

        if (isGrounded)
        {
            jumpCounter = 1; // Double jump reset
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
        soundManager.PlaySFX(soundManager.jump);
        soundManager.StopRunSFX();
        if (animator != null)
        {
            animator.SetBool("isGrounded", false);
            animator.SetBool("isJumping", true);
            animator.Play("Jump", -1, 0f);
        }
    }

    IEnumerator PerformDash(int direction)
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        Vector2 dashVelocity = new Vector2(direction * dashSpeed, 0f);

        if (animator != null)
        {
            animator.SetBool("isDodging", true);
            animator.SetTrigger("Dash");
        }

        float elapsed = 0;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashVelocity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (animator != null) animator.SetBool("isDodging", false);

        rb.gravityScale = originalGravity;
        rb.constraints = originalConstraints;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetInteger("AttackNum", comboStep);
            animator.SetTrigger("isAttacking");
            soundManager.PlaySFX(comboStep == 0 ? soundManager.attack1 : (comboStep == 1 ? soundManager.attack2 : soundManager.attack3));
        }

        comboStep = (comboStep + 1) % 3;
        StartCoroutine(ResetAttack());
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRunning", Mathf.Abs(inputX) > 0.1f);
        animator.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isJumping", !isGrounded);
    }

    public void DealAttackDamage()
    {
        if (attackPoint == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        Collider2D[] hitEnemies = new Collider2D[10];
        int count = Physics2D.OverlapCircle(attackPoint.position, attackRange, filter, hitEnemies);

        for (int i = 0; i < count; i++)
        {
            var enemy = hitEnemies[i];

            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null) health = enemy.GetComponentInParent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                continue;
            }

            var ce = enemy.GetComponentInParent<CrystalEnemyAI>();
            if (ce != null)
            {
                ce.TakeDamage(damage);
                continue;
            }

            var tb = enemy.GetComponentInParent<TentacleBossAI>();
            if (tb != null)
            {
                tb.TakeDamage(damage);
                continue;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    void PlayRunSFX()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame && !Keyboard.current.leftShiftKey.wasPressedThisFrame && isGrounded)
        {
            soundManager.PlayRunSFX(soundManager.run);
        }
    }
}