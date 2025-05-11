using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Base Bullet Settings")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected LayerMask targetLayers;
    [SerializeField] protected GameObject hitEffect;
    

    [Header("Unstable Properties")]
    [SerializeField] protected bool isUnstable = false;
    [SerializeField] protected float unstableEffectInterval = 0.5f; // How often the unstable effect triggers

    // References
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    // State tracking
    protected bool isInitialized = false;
    protected float timer = 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Start lifetime countdown
        Destroy(gameObject, lifetime);
    }

    protected virtual void Start()
    {
        // Default behavior: move forward based on rotation
        if (!isInitialized)
        {
            Initialize();
        }
    }

    public virtual void Initialize(float speedMultiplier = 1f, int damageOverride = -1, Vector2? direction = null)
    {
        // Apply movement in the specified direction or forward based on bullet's rotation
        Vector2 moveDirection = direction.HasValue ? direction.Value.normalized : transform.up;

        // Apply velocity based on the direction
        rb.linearVelocity = moveDirection * speed * speedMultiplier;

        // Set the rotation to match the movement direction
        // This calculates the angle in degrees from the x-axis
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        // Adjust this offset based on which way your sprite is initially facing
        // If your sprite faces right by default, use -90
        // If your sprite faces up by default, use 0
        // If your sprite faces left by default, use 90
        // If your sprite faces down by default, use 180
        float rotationOffset = 0f;

        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // Override damage if specified
        if (damageOverride > 0)
        {
            damage = damageOverride;
        }

        isInitialized = true;

        // Start unstable effect if enabled
        if (isUnstable)
        {
            StartCoroutine(UnstableEffect());
        }
    }

    protected virtual void Update()
    {
        // Update timer for potential time-based effects
        timer += Time.deltaTime;

        // Implement derived class behavior in OnBulletUpdate
        OnBulletUpdate();
    }

    // Override this in derived classes for custom behavior
    protected virtual void OnBulletUpdate()
    {
        // Base implementation does nothing
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit a valid target
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
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
            SoundManager.Instance.PlaySound(SoundManager.Sound.EnemyHit);
            // Destroy the bullet
            OnBulletHit(other);
            Destroy(gameObject);
        }
    }


    // Override in derived classes for custom hit behavior
    protected virtual void OnBulletHit(Collider2D other)
    {
        // Base implementation does nothing
    }

    // Unstable effect behavior - override in derived classes for different unstable effects
    protected virtual IEnumerator UnstableEffect()
    {
        while (true)
        {
            // Default unstable effect: flicker visibility
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(unstableEffectInterval);
        }
    }

    public virtual void ModifyDamage(float multiplier)
    {
        damage = Mathf.RoundToInt(damage * multiplier);
    }

    public virtual void ModifySpeed(float multiplier)
    {
        speed *= multiplier;

        // If the bullet is already moving, update its velocity
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    public virtual void ModifyLifetime(float multiplier)
    {
        lifetime *= multiplier;
    }

    public virtual void ModifyUnstableEffectInterval(float multiplier)
    {
        unstableEffectInterval *= multiplier;
    }
}

