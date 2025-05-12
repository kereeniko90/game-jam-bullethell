using UnityEngine;

public class EnemyHealth : HealthSystem
{

  /// <summary>
  /// Added by khairi
  
  [Header("Item Drop Settings")]
  [SerializeField] private GameObject healItemPrefab;
  [SerializeField] private float healDropChance = 0.3f; // 30% chance by default
  /// </summary>
  public int MaxHealth
  {
    get => maxHealth;
    set
    {
      maxHealth = value;
      currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
  }

  public int CurrentHealth
  {
    get => currentHealth;
    set => currentHealth = Mathf.Clamp(value, 0, maxHealth);
  }

  //added by khairi
  protected override void Die()
  {
    isDead = true;
    // Spawn death effect if provided
    if (deathEffect != null)
    {
      Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

    if (healItemPrefab != null && Random.value <= healDropChance)
    {
      Instantiate(healItemPrefab, transform.position, Quaternion.identity);
    }

    // Trigger death event
    OnDeath?.Invoke();


    // Base implementation just destroys the game object
    Destroy(gameObject);
  }

  // Optional: Add enemy-specific logic if needed
}
