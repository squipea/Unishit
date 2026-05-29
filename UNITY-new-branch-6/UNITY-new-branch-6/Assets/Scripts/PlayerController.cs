using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float jumpForce = 12f;

    [Header("Jump")]
    public int maxJumps = 2;

    [Header("Dash")]
    public float dashSpeed = 40f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("Flip Settings")]
    public SpriteRenderer spriteRenderer;
    public Transform attackPoint;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCombat playerCombat;
    private Collider2D playerCollider;

    private float moveInput;
    private int jumpCount;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDashing;
    private bool canDash = true;
    private float attackPointStartX;
    private float defaultGravityScale = 3f;

    private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];

    public bool IsGrounded => isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerCollider = GetComponent<Collider2D>();

        if (speed <= 0) speed = 5f;
        if (maxJumps <= 0) maxJumps = 2;
        if (jumpForce <= 0) jumpForce = 12f;
        if (dashSpeed <= 0) dashSpeed = 20f;
        if (dashDuration <= 0) dashDuration = 0.2f;
        if (groundLayer.value == 0) groundLayer = (1 << 7) | (1 << 9);

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackPoint != null)
            attackPointStartX = Mathf.Abs(attackPoint.localPosition.x);

        if (rb != null)
        {
            if (rb.gravityScale > 0f)
                defaultGravityScale = rb.gravityScale;
            else
                rb.gravityScale = defaultGravityScale;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (GetComponent<Interactable>() == null)
        {
            gameObject.AddComponent<Interactable>();
        }

        // Automatically setup DOOR1 at runtime if it exists in the scene
        GameObject door1 = GameObject.Find("DOOR1");
        if (door1 != null)
        {
            if (door1.GetComponent<BossTransitionDoor>() == null)
            {
                door1.AddComponent<BossTransitionDoor>();
            }

            if (door1.GetComponent<InitialSoldierStageTransition>() == null)
            {
                InitialSoldierStageTransition transition = door1.AddComponent<InitialSoldierStageTransition>();
                transition.bossSpawnPosition = new Vector3(-52.1f, -0.5f, 0f);
                transition.bossStageTitle = "1-4: Boss level";
                transition.hideBossStageUntilTransition = false;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isPaused))
            return;

        UpdateGroundedState();

        moveInput = Input.GetAxisRaw("Horizontal");
        HandleFlip();
        UpdateAnimatorGroundedState();
        HandleDash();

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            if (playerCombat == null || !playerCombat.IsBlocking)
                Jump();
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isDashing)
            return;

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float currentSpeed = speed;
        if (playerCombat != null && playerCombat.IsBlocking)
            currentSpeed *= 0.3f;

        float targetX = moveInput * currentSpeed;
        // Smoothly transition the horizontal speed (acceleration/deceleration)
        float smoothX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, speed * Time.fixedDeltaTime * 12f);
        rb.linearVelocity = new Vector2(smoothX, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (rb == null)
            return;

        if (rb.gravityScale <= 0f)
            rb.gravityScale = defaultGravityScale;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount++;
        SetGrounded(false);
    }

    public void ResetMovementAfterTeleport()
    {
        StopAllCoroutines();
        isDashing = false;
        canDash = true;

        if (rb != null)
        {
            rb.gravityScale = defaultGravityScale;
            rb.linearVelocity = Vector2.zero;
        }

        jumpCount = 0;
        UpdateGroundedState();
        UpdateAnimatorGroundedState();
    }

    private void UpdateGroundedState()
    {
        bool grounded = false;

        if (groundCheck != null)
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!grounded && playerCollider != null)
        {
            grounded = playerCollider.IsTouchingLayers(groundLayer);

            if (!grounded)
            {
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
                        grounded = true;
                        break;
                    }
                }
            }
        }

        SetGrounded(grounded);
    }

    private void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (grounded)
            jumpCount = 0;
    }

    private void UpdateAnimatorGroundedState()
    {
        if (animator == null)
            return;

        animator.SetFloat("speed", Mathf.Abs(moveInput));
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f && isGrounded);
    }

    void HandleDash()
    {
        if (!canDash || (GameManager.Instance != null && GameManager.Instance.isGameOver))
            return;

        if (playerCombat != null && playerCombat.IsBlocking)
            return;

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            int direction = isFacingRight ? 1 : -1;
            if (moveInput > 0) direction = 1;
            else if (moveInput < 0) direction = -1;

            StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(int direction)
    {
        if (rb == null)
            yield break;

        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        Vector2 dashVelocity = new Vector2(direction * dashSpeed, 0f);

        if (animator != null)
        {
            animator.SetBool("isDodging", true);
            animator.SetTrigger("Dash");
        }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashVelocity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (animator != null)
            animator.SetBool("isDodging", false);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleFlip()
    {
        if (moveInput > 0 && !isFacingRight)
            Flip();

        if (moveInput < 0 && isFacingRight)
            Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        if (spriteRenderer != null)
            spriteRenderer.flipX = !isFacingRight;

        if (attackPoint != null)
        {
            Vector3 pos = attackPoint.localPosition;
            pos.x = isFacingRight ? attackPointStartX : -attackPointStartX;
            attackPoint.localPosition = pos;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
            SetGrounded(true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
            SetGrounded(true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UpdateGroundedState();
    }

    private bool IsGroundCollision(Collision2D collision)
    {
        if (collision.collider == null || collision.collider.isTrigger)
            return false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
                return true;
        }

        return false;
    }
}
