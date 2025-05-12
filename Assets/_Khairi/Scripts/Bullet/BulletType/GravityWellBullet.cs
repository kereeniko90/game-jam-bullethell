using UnityEngine;
using DG.Tweening; // Added for better visual effects

public class GravityWellBullet : Bullet
{
    [Header("Gravity Well Settings")]
    [SerializeField] private float gravityRadius = 4f;
    [SerializeField] private float gravityForce = 20f; // Increased force
    [SerializeField] private float antiGravityChance = 0.2f; // Chance to repel instead of attract
    [SerializeField] private float wellDuration = 3f; // How long the gravity well lasts after impact
    [SerializeField] private LayerMask affectedLayers; // What layers are affected by gravity
    [SerializeField] private GameObject gravityFieldVFX; // Visual effect for the gravity field
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private bool isActive = false;
    private bool isAntiGravity = false;
    private float remainingDuration = 0f;
    private GameObject fieldEffect = null;
    
    protected override void Start()
    {
        base.Start();
        Debug.Log("GravityWellBullet initialized");
    }
    
    protected override void OnBulletHit(Collider2D other)
    {
        Debug.Log($"GravityWellBullet hit {other.gameObject.name}");
        
        // Don't activate twice
        if (isActive) return;
        
        // Activate gravity well on hit
        isActive = true;
        remainingDuration = wellDuration;
        
        // Determine if this is anti-gravity
        isAntiGravity = Random.value < antiGravityChance;
        Debug.Log($"GravityWell active: {isActive}, AntiGravity: {isAntiGravity}");
        
        // Create visual effect
        CreateFieldEffect();
        
        // Disable movement but keep object alive
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Disable regular collider to prevent multiple hits
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
    
    private void CreateFieldEffect()
    {
        // Create visual effect
        if (gravityFieldVFX != null)
        {
            fieldEffect = Instantiate(gravityFieldVFX, transform.position, Quaternion.identity);
            
            // Scale to match gravity radius with a nice pop-in effect
            fieldEffect.transform.localScale = Vector3.zero;
            fieldEffect.transform.DOScale(Vector3.one * (gravityRadius / 2f), 0.3f)
                .SetEase(Ease.OutBack);
            
            // Adjust color based on gravity type
            SpriteRenderer fieldRenderer = fieldEffect.GetComponent<SpriteRenderer>();
            if (fieldRenderer != null)
            {
                Color targetColor = isAntiGravity ? Color.cyan : Color.magenta;
                // Start with white and fade to target color
                fieldRenderer.color = Color.white;
                DOTween.To(() => fieldRenderer.color, x => fieldRenderer.color = x, 
                    targetColor, 0.5f);
            }
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
            
            // Debug log if we have objects to affect
            if (affectedObjects.Length > 0 && Time.frameCount % 60 == 0) // Log only once per second
            {
                Debug.Log($"GravityWell affecting {affectedObjects.Length} objects");
            }
            
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
                    
                    // Make sure enemy is awake and simulating physics
                    objRb.WakeUp();
                    
                    // Add a small effect on affected objects
                    SpriteRenderer objRenderer = obj.GetComponent<SpriteRenderer>();
                    if (objRenderer != null && Time.frameCount % 10 == 0)
                    {
                        // Flash the affected object
                        Color originalColor = objRenderer.color;
                        Color flashColor = isAntiGravity ? Color.cyan : Color.magenta;
                        flashColor.a = originalColor.a; // Preserve original alpha
                        
                        // Sequence: flash to gravity color then back to original
                        DOTween.Sequence()
                            .Append(DOTween.To(() => objRenderer.color, x => objRenderer.color = x, 
                                flashColor, 0.1f))
                            .Append(DOTween.To(() => objRenderer.color, x => objRenderer.color = x, 
                                originalColor, 0.2f));
                    }
                }
            }
            
            // Pulse effect for the field
            UpdateFieldEffect();
        }
    }
    
    private void UpdateFieldEffect()
    {
        if (fieldEffect != null)
        {
            // Pulse the size
            float pulseScale = 1f + 0.1f * Mathf.Sin(Time.time * 5f);
            fieldEffect.transform.localScale = Vector3.one * (gravityRadius / 2f) * pulseScale;
            
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
    
    private void EndGravityWell()
    {
        isActive = false;
        
        // Clean up field effect with a nice shrink animation
        if (fieldEffect != null)
        {
            SpriteRenderer fieldRenderer = fieldEffect.GetComponent<SpriteRenderer>();
            if (fieldRenderer != null)
            {
                DOTween.To(() => fieldRenderer.color, x => fieldRenderer.color = x, 
                    new Color(fieldRenderer.color.r, fieldRenderer.color.g, fieldRenderer.color.b, 0), 0.3f);
            }
            
            fieldEffect.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(fieldEffect));
        }
        
        // Destroy this object
        Destroy(gameObject);
    }
    
    private void OnDrawGizmos()
    {
        if (showDebugGizmos && isActive)
        {
            // Draw the gravity radius as a wire sphere
            Gizmos.color = isAntiGravity ? Color.cyan : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, gravityRadius);
        }
    }
    
    // Add debug visualization in scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityRadius);
    }
}