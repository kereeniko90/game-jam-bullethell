using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Optional base implementation that handles common functionality
public abstract class DamageableEntity : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected bool startInvulnerable = false;
    [SerializeField] protected float startInvulnerabilityDuration = 1f;
    
    [Header("Damage Type Resistances")]
    [SerializeField] protected float normalResistance = 1f;
    [SerializeField] protected float fireResistance = 1f;
    [SerializeField] protected float iceResistance = 1f;
    [SerializeField] protected float electricResistance = 1f;
    [SerializeField] protected float explosiveResistance = 1f;
    [SerializeField] protected float piercingResistance = 1f;
    [SerializeField] protected float unstableResistance = 1f;
    
    protected int currentHealth;
    protected bool isInvulnerable = false;
    protected Dictionary<StatusEffectType, Coroutine> activeStatusEffects;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        activeStatusEffects = new Dictionary<StatusEffectType, Coroutine>();
        
        if (startInvulnerable)
        {
            SetTemporaryInvulnerability(startInvulnerabilityDuration);
        }
    }
    
    // Basic implementations of interface methods
    public virtual void TakeDamage(int damage)
    {
        TakeDamage(damage, DamageType.Normal);
    }
    
    public virtual void TakeDamage(int damage, DamageType damageType)
    {
        if (isInvulnerable) return;
        
        // Apply resistance based on damage type
        float resistance = GetResistanceForDamageType(damageType);
        int actualDamage = Mathf.RoundToInt(damage * resistance);
        
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }
    
    public virtual void TakeDamageWithKnockback(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        TakeDamage(damage);
        
        // Apply knockback if entity has a Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }
    }
    
    public virtual void ApplyStatusEffect(StatusEffectType effectType, float duration)
    {
        // If already has this status effect, stop the current one
        if (activeStatusEffects.ContainsKey(effectType))
        {
            StopCoroutine(activeStatusEffects[effectType]);
            activeStatusEffects.Remove(effectType);
        }
        
        // Start new status effect
        Coroutine statusCoroutine = StartCoroutine(HandleStatusEffect(effectType, duration));
        activeStatusEffects.Add(effectType, statusCoroutine);
    }
    
    public virtual void ApplyDamageOverTime(int damagePerTick, float tickRate, float duration)
    {
        StartCoroutine(ApplyDOT(damagePerTick, tickRate, duration));
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
    
    public virtual void SetTemporaryInvulnerability(float duration)
    {
        StartCoroutine(TemporaryInvulnerabilityRoutine(duration));
    }
    
    public virtual void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    // Helper methods
    protected float GetResistanceForDamageType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                return fireResistance;
            case DamageType.Ice:
                return iceResistance;
            case DamageType.Electric:
                return electricResistance;
            case DamageType.Explosive:
                return explosiveResistance;
            case DamageType.Piercing:
                return piercingResistance;
            case DamageType.Unstable:
                return unstableResistance;
            case DamageType.Normal:
            default:
                return normalResistance;
        }
    }
    
    // Abstract method that derived classes must implement
    protected abstract void OnDeath();
    
    // Coroutines for handling effects
    protected virtual IEnumerator TemporaryInvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
    
    protected virtual IEnumerator ApplyDOT(int damagePerTick, float tickRate, float duration)
    {
        float endTime = Time.time + duration;
        
        while (Time.time < endTime && IsAlive())
        {
            // Apply damage directly to avoid invulnerability checks
            currentHealth = Mathf.Max(0, currentHealth - damagePerTick);
            
            if (currentHealth <= 0)
            {
                OnDeath();
                yield break;
            }
            
            yield return new WaitForSeconds(tickRate);
        }
    }
    
    protected virtual IEnumerator HandleStatusEffect(StatusEffectType effectType, float duration)
    {
        // Apply effect start logic
        OnStatusEffectStart(effectType);
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Apply effect end logic
        OnStatusEffectEnd(effectType);
        
        // Remove from active effects
        activeStatusEffects.Remove(effectType);
    }
    
    protected virtual void OnStatusEffectStart(StatusEffectType effectType)
    {
        // Override in derived classes
    }
    
    protected virtual void OnStatusEffectEnd(StatusEffectType effectType)
    {
        // Override in derived classes
    }
}