using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingBullet : Bullet
{
    [Header("Homing Settings")]
    [SerializeField] private float trackingSpeed = 200f; // Angular speed
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float loseTargetTime = 1f; // Time before finding a new target when current is lost
    [SerializeField] private LayerMask targetDetectionLayers;
    
    private Transform currentTarget;
    private float timeSinceTargetLost = 0f;
    
    protected override void OnBulletUpdate()
    {
        // Find or update target
        if (currentTarget == null || timeSinceTargetLost >= loseTargetTime)
        {
            FindNearestTarget();
            timeSinceTargetLost = 0f;
        }
        else if (currentTarget != null)
        {
            // Track toward target
            TrackTarget();
        }
        else
        {
            timeSinceTargetLost += Time.deltaTime;
        }
        
        // Add unstable behavior - occasionally change targets
        if (isUnstable && Random.value < Time.deltaTime / unstableEffectInterval)
        {
            currentTarget = null;
            timeSinceTargetLost = loseTargetTime; // Force immediate retargeting
        }
    }
    
    private void FindNearestTarget()
    {
        // Find all potential targets within radius
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(
            transform.position, 
            detectionRadius, 
            targetDetectionLayers
        );
        
        // Find closest
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D targetCollider in potentialTargets)
        {
            float distance = Vector2.Distance(transform.position, targetCollider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = targetCollider.transform;
            }
        }
        
        currentTarget = closestTarget;
    }
    
    private void TrackTarget()
    {
        // Calculate direction to target
        Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
        
        // Calculate current forward direction
        Vector2 currentDirection = transform.up;
        
        // Calculate rotation needed
        float rotationAmount = Vector3.Cross(currentDirection, directionToTarget).z;
        
        // Apply rotation to rigidbody
        rb.angularVelocity = -rotationAmount * trackingSpeed;
        
        // Update velocity direction based on new rotation
        rb.linearVelocity = transform.up * speed;
    }
}

// 5. Unstable Bullet - Completely random behavior
public class UnstableBullet : Bullet
{
    [Header("Unstable Settings")]
    [SerializeField] private float behaviorChangeInterval = 1f;
    [SerializeField] private float maxSpeedVariation = 2f;
    [SerializeField] private float maxSizeVariation = 0.5f;
    [SerializeField] private List<Color> colorCycle = new List<Color>();
    
    private float behaviorTimer = 0f;
    private int colorIndex = 0;
    
    protected override void Start()
    {
        base.Start();
        isUnstable = true; // Force unstable flag
    }
    
    protected override void OnBulletUpdate()
    {
        behaviorTimer += Time.deltaTime;
        
        // Change behavior periodically
        if (behaviorTimer >= behaviorChangeInterval)
        {
            ChangeBehavior();
            behaviorTimer = 0f;
        }
    }
    
    private void ChangeBehavior()
    {
        // 1. Random speed change
        float speedMultiplier = 1f + Random.Range(-maxSpeedVariation, maxSpeedVariation);
        rb.linearVelocity = transform.up * speed * speedMultiplier;
        
        // 2. Random size change
        float sizeChange = 1f + Random.Range(-maxSizeVariation, maxSizeVariation);
        transform.localScale = Vector3.one * sizeChange;
        
        // 3. Random slight direction change
        float angleChange = Random.Range(-30f, 30f);
        transform.Rotate(0, 0, angleChange);
        rb.linearVelocity = transform.up * rb.linearVelocity.magnitude;
        
        // 4. Random color change if we have colors defined
        if (colorCycle.Count > 0 && spriteRenderer != null)
        {
            colorIndex = (colorIndex + 1) % colorCycle.Count;
            spriteRenderer.color = colorCycle[colorIndex];
        }
    }
    
    // Override the unstable effect to do nothing (we handle it in OnBulletUpdate)
    protected override IEnumerator UnstableEffect()
    {
        yield break;
    }
}