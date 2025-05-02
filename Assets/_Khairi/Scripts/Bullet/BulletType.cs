using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 1. Unstable Phasing Bullet - Phases in and out of existence
public class PhasingBullet : Bullet
{
    [Header("Phasing Settings")]
    [SerializeField] private float phasingInterval = 0.3f;
    [SerializeField] private float phasingDuration = 0.15f;
    [SerializeField] private bool startPhased = false;
    
    private Collider2D bulletCollider;
    private bool isPhased;
    
    protected override void Awake()
    {
        base.Awake();
        bulletCollider = GetComponent<Collider2D>();
        isPhased = startPhased;
        
        // Initial state
        if (isPhased)
        {
            SetPhasedState(true);
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Start phasing regardless of unstable flag
        StartCoroutine(PhasingRoutine());
    }
    
    private IEnumerator PhasingRoutine()
    {
        while (true)
        {
            // Wait for interval
            yield return new WaitForSeconds(phasingInterval);
            
            // Phase out
            SetPhasedState(true);
            
            // Stay phased for duration
            yield return new WaitForSeconds(phasingDuration);
            
            // Phase back in
            SetPhasedState(false);
        }
    }
    
    private void SetPhasedState(bool phased)
    {
        isPhased = phased;
        
        // When phased, bullet becomes transparent and non-colliding
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = phased ? 0.3f : 1.0f;
            spriteRenderer.color = color;
        }
        
        if (bulletCollider != null)
        {
            bulletCollider.enabled = !phased;
        }
    }
    
    // Override unstable effect to do nothing (we have our own phasing)
    protected override IEnumerator UnstableEffect()
    {
        // Do nothing, phasing is handled separately
        yield break;
    }
}

// 2. Bouncing Bullet - Bounces off walls
public class BouncingBullet : Bullet
{
    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceDamping = 0.8f; // Energy lost on bounce
    [SerializeField] private LayerMask bounceSurfaces;
    
    private int bounceCount = 0;
    
    protected override void OnBulletUpdate()
    {
        // Ensure bullet rotation matches velocity direction
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Check if this is a bounceable surface
        if (((1 << other.gameObject.layer) & bounceSurfaces) != 0)
        {
            // We hit a wall - bounce!
            if (bounceCount < maxBounces)
            {
                Bounce(other);
                bounceCount++;
                return; // Don't destroy the bullet
            }
        }
        
        // Otherwise use regular hit logic
        base.OnTriggerEnter2D(other);
    }
    
    private void Bounce(Collider2D surface)
    {
        // Get the normal of the surface
        Vector2 normal = surface.bounds.center - transform.position;
        normal.Normalize();
        
        // Calculate reflection
        Vector2 velocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.Reflect(velocity, normal) * bounceDamping;
        
        // Add variation to make it more unstable
        if (isUnstable)
        {
            rb.linearVelocity += new Vector2(
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f)
            );
        }
    }
}

// 3. Splitting Bullet - Splits into multiple bullets when hitting enemies
public class SplittingBullet : Bullet
{
    [Header("Split Settings")]
    [SerializeField] private GameObject childBulletPrefab;
    [SerializeField] private int splitCount = 3;
    [SerializeField] private float spreadAngle = 120f;
    [SerializeField] private float childSpeedMultiplier = 0.8f;
    
    protected override void OnBulletHit(Collider2D other)
    {
        // Create multiple bullets in a spread pattern
        if (childBulletPrefab != null)
        {
            SplitBullet();
        }
    }
    
    private void SplitBullet()
    {
        // Calculate angle between bullets
        float angleStep = spreadAngle / (splitCount - 1);
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < splitCount; i++)
        {
            // Calculate rotation for this child bullet
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle) * transform.rotation;
            
            // Create child bullet
            GameObject childBullet = Instantiate(childBulletPrefab, transform.position, rotation);
            
            // Initialize child bullet with reduced damage and speed
            Bullet bulletComponent = childBullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(childSpeedMultiplier, Mathf.Max(1, damage / 2));
            }
        }
    }
}

// 4. Homing Bullet - Tracks toward the nearest enemy
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