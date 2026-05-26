using System.Collections;
using UnityEngine;

public class OwlAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public EnemyHealth enemyHealth;

    [Header("Hover Settings")]
    public float hoverHeight = 4f;
    public float hoverSpeed = 1f;
    public float hoverAmplitude = 0.5f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float attackCooldown = 3f;

    [Header("Attack Settings")]
    public float diveSpeed = 8f;
    public float returnSpeed = 4f;
    public int damage = 1;

    private Vector3 startPos;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float hoverOffset;

    public SoundManager soundManager;

    void Awake()
    {
        soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    void Start()
    {
        startPos = transform.position;
        hoverOffset = Random.Range(0f, 2f * Mathf.PI);

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (player == null || isAttacking || !enabled || (GameManager.Instance != null && GameManager.Instance.isPaused)) return;

        Hover();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(SwoopAttack());
        }

        // Flip based on player position
        if (player.position.x < transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    void Hover()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
    }

    IEnumerator SwoopAttack()
    {
        isAttacking = true;
        if (soundManager != null && soundManager.owlAttack != null)
            soundManager.PlaySFX(soundManager.owlAttack);
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null) animator.SetTrigger("Attack");

        Vector3 targetPos = player.position;
        
        // Dive
        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, diveSpeed * Time.deltaTime);
            yield return null;
        }

        // Slight pause at bottom
        yield return new WaitForSeconds(0.2f);

        // Return to start height (at current X)
        Vector3 returnPos = new Vector3(transform.position.x, startPos.y, transform.position.z);
        while (Vector2.Distance(transform.position, returnPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, returnPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandlePlayerCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandlePlayerCollision(collision);
    }

    private void HandlePlayerCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = collision.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
