using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerController playerController;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;
        HandleAnimations();
    }

    void HandleAnimations()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);
        bool isGrounded = playerController != null
            ? playerController.IsGrounded
            : Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        // Updated to match New Player Animator
        animator.SetFloat("speed", speed);
        animator.SetBool("isRunning", speed > 0.1f && isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", !isGrounded);
    }
}