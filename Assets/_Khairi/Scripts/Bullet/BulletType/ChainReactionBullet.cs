using System.Collections;
using UnityEngine;

public class ChainReactionBullet : Bullet
{
    [Header("Chain Reaction Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float primaryExplosionRadius = 2f;
    [SerializeField] private int primaryExplosionDamage = 2;
    [SerializeField] private float secondaryChance = 0.3f;
    [SerializeField] private float secondaryExplosionRadius = 4f;
    [SerializeField] private int secondaryExplosionDamage = 4;
    [SerializeField] private float tertiaryChance = 0.1f;
    [SerializeField] private float tertiaryExplosionRadius = 6f;
    [SerializeField] private int tertiaryExplosionDamage = 8;
    
    protected override void OnBulletHit(Collider2D other)
    {
        // Primary explosion
        Explode(transform.position, primaryExplosionRadius, primaryExplosionDamage, 1);
    }
    
    private void Explode(Vector3 position, float radius, int explosionDamage, int chainLevel)
    {
        // Spawn explosion VFX
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            
            // Scale explosion based on radius
            explosion.transform.localScale = Vector3.one * (radius / 2f);
            
            // Destroy after animation
            Destroy(explosion, 1f);
        }
        
        // Find targets in explosion radius
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, radius, targetLayers);
        
        // Damage all targets
        foreach (Collider2D target in targets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }
        
        // Check for chain reaction
        if (chainLevel == 1 && Random.value < secondaryChance)
        {
            // Delay for effect
            StartCoroutine(DelayedExplosion(position, secondaryExplosionRadius, secondaryExplosionDamage, 2));
        }
        else if (chainLevel == 2 && Random.value < tertiaryChance)
        {
            // Delay for effect
            StartCoroutine(DelayedExplosion(position, tertiaryExplosionRadius, tertiaryExplosionDamage, 3));
        }
    }
    
    private IEnumerator DelayedExplosion(Vector3 position, float radius, int damage, int chainLevel)
    {
        // Unstable factor - random delay
        float delay = Random.Range(0.1f, 0.3f);
        yield return new WaitForSeconds(delay);
        
        Explode(position, radius, damage, chainLevel);
    }
}