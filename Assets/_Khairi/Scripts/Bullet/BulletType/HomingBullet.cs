using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingBullet : Bullet
{
    [Header("Homing Settings")]
    [SerializeField] private float trackingSpeed = 200f; // Angular speed
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float loseTargetTime = 1f; // Time before finding a new target when current is lost
    [SerializeField] private LayerMask enemyLayerMask; // Specific layer mask for enemies
    [SerializeField] private string enemyTag = "Enemy"; // Tag to identify enemies
    [SerializeField] private bool useLayerMask = true; // Whether to use layer mask or tag
    
    private Transform currentTarget;
    private float timeSinceTargetLost = 0f;
    
    protected override void Start()
    {
        base.Start();
        
        // If not explicitly set, default to using Enemy layer
        if (enemyLayerMask.value == 0 && useLayerMask)
        {
            enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
            Debug.Log("HomingBullet: No enemy layer mask set, defaulting to 'Enemy' layer");
        }
    }
    
    protected override void OnBulletUpdate()
    {
        // Find or update target
        if (currentTarget == null || timeSinceTargetLost >= loseTargetTime)
        {
            FindNearestEnemyTarget();
            timeSinceTargetLost = 0f;
        }
        else if (currentTarget != null)
        {
            // Check if target still exists and is active
            if (!currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                timeSinceTargetLost = loseTargetTime; // Force immediate retargeting
            }
            else
            {
                // Track toward target
                TrackTarget();
            }
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
    
    private void FindNearestEnemyTarget()
    {
        Collider2D[] potentialTargets;
        
        if (useLayerMask)
        {
            // Find all potential targets within radius using layer mask
            potentialTargets = Physics2D.OverlapCircleAll(
                transform.position, 
                detectionRadius, 
                enemyLayerMask
            );
        }
        else
        {
            // Find all colliders in radius
            potentialTargets = Physics2D.OverlapCircleAll(
                transform.position,
                detectionRadius
            );
        }
        
        // Find closest valid enemy
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D targetCollider in potentialTargets)
        {
            // If using tag system, check for the enemy tag
            if (!useLayerMask && !targetCollider.CompareTag(enemyTag))
            {
                continue; // Skip if not an enemy
            }
            
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
    
    // Visualize the detection radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}