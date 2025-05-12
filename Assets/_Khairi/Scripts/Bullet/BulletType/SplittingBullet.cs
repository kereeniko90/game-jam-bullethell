using UnityEngine;

public class SplittingBullet : Bullet
{
    [Header("Split Settings")]
    [SerializeField] private GameObject childBulletPrefab;
    [SerializeField] private int splitCount = 3;
    [SerializeField] private float spreadAngle = 120f;
    [SerializeField] private float childSpeedMultiplier = 0.8f;
    
    private bool hasTriggeredSplit = false;
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit a valid target and haven't split yet
        if (((1 << other.gameObject.layer) & targetLayers) != 0 && !hasTriggeredSplit)
        {
            // Mark that we've triggered the split to prevent multiple splits
            hasTriggeredSplit = true;
            
            // Deal damage if target has health component
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // Spawn hit effect if available
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Split the bullet
            if (childBulletPrefab != null)
            {
                SplitBullet();
            }
            
            // Play sound effect
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Sound.EnemyHit);
            }
            
            // Destroy this bullet after splitting
            Destroy(gameObject);
        }
        else if (((1 << other.gameObject.layer) & targetLayers) == 0)
        {
            // For non-target collisions, use the base behavior
            base.OnTriggerEnter2D(other);
        }
    }
    
    private void SplitBullet()
    {
        // Only split if we have a valid prefab and count
        if (childBulletPrefab == null || splitCount <= 0)
        {
            Debug.LogWarning("SplittingBullet: Missing child bullet prefab or invalid split count");
            return;
        }
        
        try
        {
            // Calculate angle between bullets
            float angleStep = (splitCount > 1) ? spreadAngle / (splitCount - 1) : 0f;
            float startAngle = -spreadAngle / 2f;
            
            for (int i = 0; i < splitCount; i++)
            {
                // Calculate rotation for this child bullet
                float angle = startAngle + (angleStep * i);
                
                // Create rotation quaternion
                Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
                
                // Safely create child bullet slightly offset from current position
                Vector3 spawnPos = transform.position;
                GameObject childBullet = Instantiate(childBulletPrefab, spawnPos, rotation);
                
                // Initialize child bullet with reduced damage and speed
                Bullet bulletComponent = childBullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    int splitDamage = Mathf.Max(1, damage / 2);
                    // Make sure we use the proper direction based on the new rotation
                    Vector2 direction = (rotation * Vector2.up).normalized;
                    bulletComponent.Initialize(childSpeedMultiplier, splitDamage, direction);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in SplittingBullet.SplitBullet: " + e.Message);
        }
    }
    
    // We don't use this method as we handle everything in OnTriggerEnter2D
    protected override void OnBulletHit(Collider2D other)
    {
        // Intentionally empty - all logic is in OnTriggerEnter2D
    }
}