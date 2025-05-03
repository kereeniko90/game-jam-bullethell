using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class PlayerHealth : HealthSystem
{
    [Header("Player Health Settings")]
    [SerializeField] private float respawnInvulnerabilityTime = 1.5f;
    
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Hit Feedback")]
    [SerializeField] private float cameraShakeAmount = 0.1f;
    [SerializeField] private float cameraShakeDuration = 0.2f;
    
    private bool isDead = false;
    
    public override void Start()
    {
        base.Start();
        
        // Initialize UI
        UpdateUI();
    }
    
    public override void TakeDamage(int amount)
    {
        // Don't take damage if already dead
        if (isDead)
            return;
            
        base.TakeDamage(amount);
        
        // Update UI after taking damage
        UpdateUI();
        
        // Camera shake effect (if you have a camera shake system)
        // CameraShaker.Instance.Shake(cameraShakeDuration, cameraShakeAmount);
    }
    
    public override void Heal(int amount)
    {
        // Don't heal if dead
        if (isDead)
            return;
            
        base.Heal(amount);
        
        // Update UI after healing
        UpdateUI();
    }
    
    protected override void Die()
    {
        if (isDead)
            return;
            
        isDead = true;
        
        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Notify GameManager of player death
        // Instead of destroying the player, we might want to handle game over differently
        //GameManager.Instance?.PlayerDied();
        
        // Disable player controls
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Make the player visually dead (optional)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
    }
    
    private void UpdateUI()
    {
        float healthPct = GetHealthPercentage();
        
        // Update health bar slider
        if (healthBar != null)
        {
            healthBar.value = healthPct;
        }
        
        // Update health bar color based on health percentage
        if (healthBarFill != null && healthGradient != null)
        {
            healthBarFill.color = healthGradient.Evaluate(healthPct);
        }
        
        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    // Method to reset player after death (called by GameManager perhaps)
    public void Revive()
    {
        if (!isDead)
            return;
            
        isDead = false;
        
        // Reset health
        currentHealth = maxHealth;
        
        // Re-enable player
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Reset visuals
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Temporary invulnerability
        StartCoroutine(ExtendedInvulnerabilityRoutine());
        
        // Update UI
        UpdateUI();
    }
    
    private IEnumerator ExtendedInvulnerabilityRoutine()
    {
        isInvulnerabilityActive = true;
        
        // Add visual feedback for invulnerability (flashing)
        if (spriteRenderer != null)
        {
            float endTime = Time.time + respawnInvulnerabilityTime;
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
            yield return new WaitForSeconds(respawnInvulnerabilityTime);
        }
        
        isInvulnerabilityActive = false;
    }
    
    // Additional player-specific functionality
    public bool IsDead()
    {
        return isDead;
    }
}