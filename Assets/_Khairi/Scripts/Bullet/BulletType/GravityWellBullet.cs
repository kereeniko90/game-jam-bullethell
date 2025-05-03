using UnityEngine;

public class GravityWellBullet : Bullet
{
    [Header("Gravity Well Settings")]
    [SerializeField] private float gravityRadius = 4f;
    [SerializeField] private float gravityForce = 10f;
    [SerializeField] private float antiGravityChance = 0.2f; // Chance to repel instead of attract
    [SerializeField] private float wellDuration = 3f; // How long the gravity well lasts after impact
    [SerializeField] private LayerMask affectedLayers; // What layers are affected by gravity
    [SerializeField] private GameObject gravityFieldVFX; // Visual effect for the gravity field
    
    private bool isActive = false;
    private bool isAntiGravity = false;
    private float remainingDuration = 0f;
    private GameObject fieldEffect = null;
    
    protected override void OnBulletHit(Collider2D other)
    {
        // Activate gravity well on hit
        isActive = true;
        remainingDuration = wellDuration;
        
        // Determine if this is anti-gravity
        isAntiGravity = Random.value < antiGravityChance;
        
        // Create visual effect
        if (gravityFieldVFX != null)
        {
            fieldEffect = Instantiate(gravityFieldVFX, transform.position, Quaternion.identity);
            
            // Scale to match gravity radius
            fieldEffect.transform.localScale = Vector3.one * (gravityRadius / 2.5f);
            
            // Adjust color based on gravity type
            SpriteRenderer fieldRenderer = fieldEffect.GetComponent<SpriteRenderer>();
            if (fieldRenderer != null)
            {
                fieldRenderer.color = isAntiGravity ? Color.cyan : Color.magenta;
            }
        }
        
        // Disable movement but keep object alive
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Disable regular collider
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }
        
        // Hide sprite but keep object for gravity effect
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    protected override void OnBulletUpdate()
    {
        // Update gravity well if active
        if (isActive)
        {
            // Update remaining duration
            remainingDuration -= Time.deltaTime;
            if (remainingDuration <= 0f)
            {
                EndGravityWell();
                return;
            }
            
            // Find objects within gravity radius
            Collider2D[] affectedObjects = Physics2D.OverlapCircleAll(transform.position, gravityRadius, affectedLayers);
            
            foreach (Collider2D obj in affectedObjects)
            {
                Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
                if (objRb != null && obj.gameObject != gameObject)
                {
                    // Calculate direction and distance
                    Vector2 direction = transform.position - obj.transform.position;
                    float distance = direction.magnitude;
                    
                    // Skip if exactly at center to avoid division by zero
                    if (distance < 0.1f) continue;
                    
                    // Calculate force (stronger at closer range)
                    float forceMagnitude = gravityForce * (1f - distance / gravityRadius);
                    forceMagnitude = Mathf.Max(0, forceMagnitude); // Ensure positive
                    
                    // Apply force - either attract or repel
                    Vector2 force = direction.normalized * forceMagnitude;
                    if (isAntiGravity) force = -force;
                    
                    objRb.AddForce(force);
                }
            }
            
            // Pulse effect for the field
            if (fieldEffect != null)
            {
                float pulseScale = 1f + 0.1f * Mathf.Sin(Time.time * 5f);
                fieldEffect.transform.localScale = Vector3.one * (gravityRadius / 2.5f) * pulseScale;
                
                // Fade out near end of duration
                if (remainingDuration < 1f)
                {
                    SpriteRenderer fieldRenderer = fieldEffect.GetComponent<SpriteRenderer>();
                    if (fieldRenderer != null)
                    {
                        Color color = fieldRenderer.color;
                        color.a = remainingDuration;
                        fieldRenderer.color = color;
                    }
                }
            }
        }
    }
    
    private void EndGravityWell()
    {
        isActive = false;
        
        // Clean up field effect
        if (fieldEffect != null)
        {
            Destroy(fieldEffect);
        }
        
        // Destroy this object
        Destroy(gameObject);
    }
}