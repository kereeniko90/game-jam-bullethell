using UnityEngine;

public class ProbabilityWaveBullet : Bullet
{
    [Header("Wave Settings")]
    [SerializeField] private float baseAmplitude = 1f; // Base wave size
    [SerializeField] private float baseFrequency = 2f; // Base wave frequency
    [SerializeField] private float maxAmplitudeVariation = 0.5f; // How much amplitude can change
    [SerializeField] private float maxFrequencyVariation = 1f; // How much frequency can change
    [SerializeField] private float changeInterval = 0.5f; // How often wave parameters change
    [SerializeField] private float waveDamageMultiplierAtPeak = 1.5f; // Damage boost at wave peak
    
    private float currentAmplitude;
    private float currentFrequency;
    private float timeSinceLastChange = 0f;
    private Vector2 movementDirection;
    private Vector2 waveDirection; // Perpendicular to movement
    private float traveledDistance = 0f;
    private Vector2 lastPosition;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize wave parameters
        currentAmplitude = baseAmplitude;
        currentFrequency = baseFrequency;
        
        // Set initial direction
        movementDirection = transform.up;
        waveDirection = new Vector2(-movementDirection.y, movementDirection.x);
        
        lastPosition = transform.position;
    }
    
    protected override void OnBulletUpdate()
    {
        // Update traveled distance
        Vector2 currentPosition = transform.position;
        traveledDistance += Vector2.Distance(lastPosition, currentPosition);
        lastPosition = currentPosition;
        
        // Time to change wave parameters?
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeInterval)
        {
            timeSinceLastChange = 0f;
            UpdateWaveParameters();
        }
        
        // Calculate wave offset
        float waveOffset = currentAmplitude * Mathf.Sin(traveledDistance * currentFrequency);
        
        // Set velocity with wave motion
        rb.linearVelocity = (movementDirection * speed) + (waveDirection * waveOffset * 5f); // Multiplier for more noticeable effect
        
        // Rotate bullet to face direction of travel
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        // Adjust damage based on wave position if at peak
        float waveSin = Mathf.Sin(traveledDistance * currentFrequency);
        float waveDamageMultiplier = 1f + (waveDamageMultiplierAtPeak - 1f) * Mathf.Abs(waveSin);
        
        // Visual feedback - brighter at higher damage points
        if (spriteRenderer != null)
        {
            float intensity = 0.5f + (waveDamageMultiplier - 1f) * 0.5f;
            spriteRenderer.color = new Color(intensity, intensity, intensity);
        }
    }
    
    private void UpdateWaveParameters()
    {
        // Randomly adjust amplitude and frequency
        currentAmplitude = baseAmplitude + Random.Range(-maxAmplitudeVariation, maxAmplitudeVariation);
        currentFrequency = baseFrequency + Random.Range(-maxFrequencyVariation, maxFrequencyVariation);
        
        // Keep amplitude positive
        currentAmplitude = Mathf.Max(0.1f, currentAmplitude);
        currentFrequency = Mathf.Max(0.5f, currentFrequency);
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit a valid target
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            // Calculate wave position for damage multiplier
            float waveSin = Mathf.Sin(traveledDistance * currentFrequency);
            float waveDamageMultiplier = 1f + (waveDamageMultiplierAtPeak - 1f) * Mathf.Abs(waveSin);
            int adjustedDamage = Mathf.RoundToInt(damage * waveDamageMultiplier);
            
            // Deal damage with multiplier
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(adjustedDamage);
            }
            
            // Spawn hit effect if available
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy the bullet
            OnBulletHit(other);
            Destroy(gameObject);
        }
    }
}