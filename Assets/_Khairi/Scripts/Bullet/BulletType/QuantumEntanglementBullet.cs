using UnityEngine;

public class QuantumEntanglementBullet : Bullet
{
    [Header("Entanglement Settings")]
    [SerializeField] private float partnerOffset = 2f; // Distance between entangled bullets
    [SerializeField] private float entanglementBreakChance = 0.05f; // Chance per second to break entanglement
    [SerializeField] private float teleportDamageMultiplier = 1.5f; // Extra damage on teleport hit
    
    private GameObject partnerBullet;
    private bool isPartner = false; // Is this the partner or the original bullet
    private bool entanglementBroken = false;
    
    public override void Initialize(float speedMultiplier = 1f, int damageOverride = -1, Vector2? direction = null)
    {
        base.Initialize(speedMultiplier, damageOverride, direction);
        
        // Create partner bullet if this is the original
        if (!isPartner)
        {
            // Calculate perpendicular vector for partner placement
            Vector2 perpendicular = new Vector2(-transform.up.y, transform.up.x) * partnerOffset;
            Vector3 partnerPosition = transform.position + new Vector3(perpendicular.x, perpendicular.y, 0);
            
            // Create partner with identical properties but offset position
            partnerBullet = Instantiate(gameObject, partnerPosition, transform.rotation);
            
            // Configure partner to prevent infinite recursion
            QuantumEntanglementBullet partnerComponent = partnerBullet.GetComponent<QuantumEntanglementBullet>();
            if (partnerComponent != null)
            {
                partnerComponent.isPartner = true;
                partnerComponent.entanglementBroken = false;
                
                // Reference back to original
                partnerComponent.partnerBullet = gameObject;
            }
        }
    }
    
    protected override void OnBulletUpdate()
    {
        // Check for entanglement break
        if (!entanglementBroken && Random.value < entanglementBreakChance * Time.deltaTime)
        {
            BreakEntanglement();
        }
        
        // Mirror movement of partner if entanglement is intact
        if (!entanglementBroken && !isPartner && partnerBullet != null)
        {
            // Calculate the mirrored movement 
            // This is simplified; in a real implementation you might want to use more complex mirroring
            Vector2 perpendicular = new Vector2(-transform.up.y, transform.up.x) * partnerOffset;
            partnerBullet.transform.position = transform.position + new Vector3(perpendicular.x, perpendicular.y, 0);
            partnerBullet.transform.rotation = transform.rotation;
        }
    }
    
    protected override void OnBulletHit(Collider2D other)
    {
        // If we hit something and entanglement is intact, teleport partner to hit location
        if (!entanglementBroken && partnerBullet != null)
        {
            // Teleport partner to hit location
            partnerBullet.transform.position = transform.position;
            
            // Apply additional damage to the hit target
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(Mathf.RoundToInt(damage * teleportDamageMultiplier));
            }
            
            // Destroy partner after teleport hit
            Destroy(partnerBullet);
        }
    }
    
    private void BreakEntanglement()
    {
        entanglementBroken = true;
        
        // If we have a partner, break their entanglement too
        if (partnerBullet != null)
        {
            QuantumEntanglementBullet partnerComponent = partnerBullet.GetComponent<QuantumEntanglementBullet>();
            if (partnerComponent != null)
            {
                partnerComponent.entanglementBroken = true;
            }
            
            // Make bullets go in random directions when entanglement breaks
            if (rb != null)
            {
                // Assign a random velocity direction
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.linearVelocity = randomDirection * speed;
                
                // Update bullet rotation to match new direction
                float angle = Mathf.Atan2(randomDirection.y, randomDirection.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
