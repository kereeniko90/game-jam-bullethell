using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected bool isInvulnerable = false;
    [SerializeField] protected float invulnerabilityDuration = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject hitEffect;
    [SerializeField] protected GameObject deathEffect;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Color hitFlashColor = Color.red;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public UnityEvent<int> OnDamageTaken;        // (damageAmount)
    public UnityEvent OnDeath;

    protected bool isInvulnerabilityActive = false;
    protected Color originalColor;
    public bool isDead = false;

    protected virtual void Awake()
    {
        // Store reference to the sprite renderer if not set
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public virtual void Start()
    {
        // Initialize health to max at start
        currentHealth = maxHealth;
    }

    // IDamageable implementation
    public virtual void TakeDamage(int amount)
    {
        // If invulnerable, no damage is taken
        if (isInvulnerabilityActive || isInvulnerable)
            return;

        // Apply damage
        currentHealth -= amount;

        // Trigger damage event
        OnDamageTaken?.Invoke(amount);

        // Trigger health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);


        // Visual feedback
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Flash sprite if available
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }

        // Temporary invulnerability
        if (invulnerabilityDuration > 0)
        {
            StartCoroutine(InvulnerabilityRoutine());
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
            
        }
    }

    public virtual void Heal(int amount)
    {
        // Increase health but don't exceed max
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Trigger health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public virtual void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;

        // Ensure current health doesn't exceed new max
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Trigger health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public virtual bool IsAlive()
    {
        return currentHealth > 0;
    }

    protected virtual void Die()
    {   
        isDead = true;
        // Spawn death effect if provided
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Base implementation just destroys the game object
        Destroy(gameObject);
    }

    protected System.Collections.IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        // Flash to hit color
        spriteRenderer.color = hitFlashColor;

        // Wait a short time
        yield return new WaitForSeconds(0.1f);

        // Restore original color
        spriteRenderer.color = originalColor;
    }

    protected System.Collections.IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerabilityActive = true;

        // Add visual feedback for invulnerability (flashing)
        if (spriteRenderer != null)
        {
            float endTime = Time.time + invulnerabilityDuration;
            bool visible = false;

            // Flash while invulnerable
            while (Time.time < endTime)
            {
                visible = !visible;
                spriteRenderer.enabled = visible;
                yield return new WaitForSeconds(0.1f);
            }

            // Ensure sprite is visible when done
            spriteRenderer.enabled = true;
        }
        else
        {
            // Just wait for the duration if no sprite renderer
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerabilityActive = false;
    }

    // Getters for current health status
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;

    public bool IsDead() => isDead;
}

public interface IDamageable
{
    void TakeDamage(int amount);
}
