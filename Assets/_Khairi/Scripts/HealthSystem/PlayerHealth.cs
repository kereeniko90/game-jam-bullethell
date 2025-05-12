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
    [SerializeField] private GameObject spriteToDestroy;

    private bool isPlayerDead = false;
    private PlayerController playerController;

    public override void Start()
    {
        base.Start();

        // Initialize UI
        UpdateUI();
        playerController = GetComponent<PlayerController>();
    }

    public override void TakeDamage(int amount)
    {
        // Don't take damage if already dead
        if (isDead)
            return;

        if (playerController.GetDashingStatus()) return;
        SoundManager.Instance.PlaySound(SoundManager.Sound.PlayerHit);
        TimeUI.Instance.ResetCombo();
        base.TakeDamage(amount);
        Debug.Log($"Player took {amount} damage. Health: {currentHealth}/{maxHealth}");
        Debug.Log($"<color=red>Current health is {currentHealth}");
        // Update UI after taking damage
        UpdateUI();

        // Camera shake effect (if you have a camera shake system)
        // CameraShaker.Instance.Shake(cameraShakeDuration, cameraShakeAmount);
    }

    public override void Heal(int amount)
    {
        // Don't heal if dead
        if (isPlayerDead)
            return;

        base.Heal(amount);

        // Update UI after healing
        UpdateUI();
    }

    protected override void Die()
    {
        if (isPlayerDead)
            return;

        isPlayerDead = true;

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Disable player controls
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable collider
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }

        // Destroy the sprite object
        if (spriteToDestroy != null)
        {
            Destroy(spriteToDestroy);
        }

        // Make the player visually dead (optional)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0, 0, 0, 0);
        }

        // Show the game over UI
        if (GameOverUI.Instance != null)
        {
            // Get survival time from GameManager if available
            float survivalTime = 0f;
            if (GameManager.Instance != null)
            {
                survivalTime = GameManager.Instance.survivalTime;
            }

            // Show game over screen with survival time
            StartCoroutine(ShowGameOverAfterDelay(2f));
        }
        else
        {
            Debug.LogWarning("GameOverUI instance not found! Please make sure it's in the scene.");
        }
    }

     private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Show the game over UI
        if (GameOverUI.Instance != null)
        {
            // Get survival time from GameManager if available
            float survivalTime = 0f;
            if (GameManager.Instance != null)
            {
                survivalTime = GameManager.Instance.survivalTime;
            }
            
            // Show game over screen with survival time
            GameOverUI.Instance.ShowGameOver(survivalTime);
        }
        else
        {
            Debug.LogWarning("GameOverUI instance not found! Please make sure it's in the scene.");
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
        if (!isPlayerDead)
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
    public bool IsPlayerDead()
    {
        return isPlayerDead;
    }
}