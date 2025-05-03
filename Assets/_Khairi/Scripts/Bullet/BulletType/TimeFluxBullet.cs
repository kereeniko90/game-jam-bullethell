using System.Collections;
using UnityEngine;

public class TimeFluxBullet : Bullet
{
    [Header("Time Flux Settings")]
    [SerializeField] private float minSpeedMultiplier = 0.2f; // Slowest speed
    [SerializeField] private float maxSpeedMultiplier = 2.5f; // Fastest speed
    [SerializeField] private float changeFrequency = 1.0f; // How often speed changes
    [SerializeField] private float freezeChance = 0.1f; // Chance to freeze per change
    [SerializeField] private float freezeDuration = 0.5f; // How long freeze lasts
    
    private float baseSpeed;
    private float currentSpeedMultiplier = 1.0f;
    private float timeSinceLastChange = 0f;
    private bool isFrozen = false;
    private float freezeTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        baseSpeed = speed;
    }
    
    protected override void OnBulletUpdate()
    {
        if (isFrozen)
        {
            // Handle frozen state
            freezeTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            
            if (freezeTimer <= 0)
            {
                isFrozen = false;
                // Resume movement with current multiplier
                rb.linearVelocity = transform.up * baseSpeed * currentSpeedMultiplier;
            }
            return;
        }
        
        // Time to change speed?
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeFrequency)
        {
            timeSinceLastChange = 0f;
            
            // Check for freeze
            if (Random.value < freezeChance)
            {
                isFrozen = true;
                freezeTimer = freezeDuration;
                rb.linearVelocity = Vector2.zero;
                
                // Visual indicator of freeze
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.cyan;
                }
            }
            else
            {
                // Change speed
                currentSpeedMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
                rb.linearVelocity = transform.up * baseSpeed * currentSpeedMultiplier;
                
                // Visual indicator of speed (optional)
                if (spriteRenderer != null)
                {
                    // Faster = more reddish, slower = more blueish
                    float t = (currentSpeedMultiplier - minSpeedMultiplier) / (maxSpeedMultiplier - minSpeedMultiplier);
                    spriteRenderer.color = Color.Lerp(Color.blue, Color.red, t);
                }
            }
        }
    }
    
    // Resume normal color when bullet unfreezes
    protected override IEnumerator UnstableEffect()
    {
        while (true)
        {
            // Only apply unstable effect if not frozen
            if (!isFrozen && spriteRenderer != null)
            {
                // Pulse effect for unstable time bullets
                float pulse = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(0.6f, 1.0f, pulse));
            }
            
            yield return null;
        }
    }
}