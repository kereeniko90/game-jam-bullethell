using UnityEngine;
using System.Collections;


public interface IDamageable
{
    // Core damage handling
    void TakeDamage(int damage);
    
    // Extended functionality for different damage types
    void TakeDamage(int damage, DamageType damageType);
    
    // Handle knockback with damage
    void TakeDamageWithKnockback(int damage, Vector2 knockbackDirection, float knockbackForce);
    
    // Status effects
    void ApplyStatusEffect(StatusEffectType effectType, float duration);
    
    // Damage over time
    void ApplyDamageOverTime(int damagePerTick, float tickRate, float duration);
    
    // Check if entity is alive
    bool IsAlive();
    
    // Get current health status
    float GetHealthPercentage();
    
    // Getter for current health
    int GetCurrentHealth();
    
    // Getter for max health
    int GetMaxHealth();
    
    // Check if entity is invincible/invulnerable
    bool IsInvulnerable();
    
    // Apply temporary invulnerability
    void SetTemporaryInvulnerability(float duration);
    
    // Heal/restore health
    void RestoreHealth(int amount);
}

// Create an enum for different damage types
public enum DamageType
{
    Normal,
    Fire,
    Ice,
    Electric,
    Explosive,
    Piercing,
    Unstable // Special damage type for your game's theme
}

// Create an enum for status effects
public enum StatusEffectType
{
    None,
    Burn,
    Freeze,
    Shock,
    Slow,
    Confusion, // For unstable theme - reverses controls
    Glitch,    // For unstable theme - random teleportation
    Phasing    // For unstable theme - intermittent collision
}