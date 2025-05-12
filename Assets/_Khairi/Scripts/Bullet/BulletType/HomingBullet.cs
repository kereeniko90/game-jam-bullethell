using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingBullet : Bullet
{
    [Header("Homing Settings")]
    [SerializeField] private float trackingSpeed = 200f; // Angular speed
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float loseTargetTime = 1f; // Time before finding a new target when current is lost
    
    private Transform currentTarget;
    private float timeSinceTargetLost = 0f;
    
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
        // Find all colliders in radius
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius
        );
        
        // Find closest target with a health component (IDamageable)
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D targetCollider in potentialTargets)
        {
            // Skip own gameObject or its parents
            if (targetCollider.gameObject == gameObject || targetCollider.transform.IsChildOf(transform))
                continue;
                
            // Check if it has a health component (IDamageable)
            IDamageable damageableComponent = targetCollider.GetComponent<IDamageable>();
            
            // Skip if it doesn't have a health component
            if (damageableComponent == null)
                continue;
                
            // Skip if it's our own bullet or another player object (optional - add checks if needed)
            // For example: if (targetCollider.CompareTag("Player") || targetCollider.CompareTag("PlayerBullet")) continue;
            
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