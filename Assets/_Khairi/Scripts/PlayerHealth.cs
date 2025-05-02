using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float healthRegenCooldown = 30f; // Optional: time before health regenerates
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float blinkRate = 0.1f; // How fast to blink when invincible
    
    [Header("Audio")]
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;
    
    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerDeath;
    
    // Private variables
    private int currentHealth;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    // Game Manager references (optional)
    //private GameManager gameManager;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Create audio source if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Try to find the game manager
        //gameManager = FindObjectOfType<GameManager>();
    }
    
    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Trigger initial health update event
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void TakeDamage(int damage)
    {
        // If invincible, ignore damage
        if (isInvincible) return;
        
        // Apply damage
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Invoke event for UI updates
        OnHealthChanged?.Invoke(currentHealth);
        
        // Play damage audio
        if (damageSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSFX);
        }
        
        // Show damage effect
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Start invincibility
        StartCoroutine(InvincibilityFrames());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Play death audio
        if (deathSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }
        
        // Show death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Invoke death event
        OnPlayerDeath?.Invoke();
        
        // Notify game manager if available
        // if (gameManager != null)
        // {
        //     gameManager.PlayerDied();
        // }
        
        // Disable player object but don't destroy immediately
        // This allows death effects and audio to play
        GetComponent<PlayerController>().enabled = false;
        
        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        
        // Hide player sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Schedule destruction after a delay
        Destroy(gameObject, 2f);
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Visual feedback with blinking
        if (spriteRenderer != null)
        {
            float endTime = Time.time + invincibilityDuration;
            
            while (Time.time < endTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkRate);
            }
            
            // Ensure sprite is visible when invincibility ends
            spriteRenderer.enabled = true;
        }
        else
        {
            // If no sprite renderer, just wait
            yield return new WaitForSeconds(invincibilityDuration);
        }
        
        isInvincible = false;
    }
    
    // Optional: Health restoration method
    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    // Optional: Full health restoration
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    // Getter for current health (useful for UI)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // Getter for max health (useful for UI)
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    // Method to check if player is currently invincible
    public bool IsInvincible()
    {
        return isInvincible;
    }
    
    // Optional: Add temporary invincibility (for power-ups)
    public void AddTemporaryInvincibility(float duration)
    {
        StartCoroutine(TemporaryInvincibility(duration));
    }
    
    private IEnumerator TemporaryInvincibility(float duration)
    {
        // If already invincible, extend the duration
        if (isInvincible)
        {
            // Stop existing coroutines
            StopCoroutine("InvincibilityFrames");
        }
        
        isInvincible = true;
        
        // Visual feedback
        if (spriteRenderer != null)
        {
            float endTime = Time.time + duration;
            
            while (Time.time < endTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkRate);
            }
            
            // Ensure sprite is visible after effect
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
        
        isInvincible = false;
    }
}