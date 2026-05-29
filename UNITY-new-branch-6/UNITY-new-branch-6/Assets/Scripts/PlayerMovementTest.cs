using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);
    }
}