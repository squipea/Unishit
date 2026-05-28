using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private HealthBar healthBar;

    private int currentHealth;
    public SoundManager soundManager;

    void Awake()
    {
        soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        Debug.Log($"{gameObject.name} took {damage} damage. HP left: {currentHealth}");

        // Damage Flash
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(DamageFlash(sr));
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("Damage");
        }

        if (currentHealth <= 0)
        {
            soundManager.PlaySFX(soundManager.growl);
            Die();
        }
    }

    private IEnumerator DamageFlash(SpriteRenderer sr)
    {
        Color originalColor = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} defeated.");
        
        // Prevent falling
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Animator animator = GetComponent<Animator>();
        bool hasDieAnimation = false;
        
        if (animator != null)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "Die")
                {
                    hasDieAnimation = true;
                    break;
                }
            }
        }

        if (hasDieAnimation)
        {
            animator.SetTrigger("Die");
            DisableEnemy();
            Destroy(gameObject, 2f); // Reduced from 15s to be more reasonable
        }
        else
        {
            DisableEnemy();
            Destroy(gameObject);
        }
    }

    private void DisableEnemy()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders) col.enabled = false;
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this) script.enabled = false;
        }
        
        // Disable this script too so it's not counted by HUD/LevelManager
        this.enabled = false;

        // Update the Objective HUD if it exists
        if (ObjectiveHUD.Instance != null)
        {
            ObjectiveHUD.Instance.UpdateUI();
        }
    }
}