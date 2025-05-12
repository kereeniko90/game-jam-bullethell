using UnityEngine;

public class EnemyHealth : HealthSystem
{
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

  // Optional: Add enemy-specific logic if needed
}
