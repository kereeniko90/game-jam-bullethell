using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Bullet Upgrade", menuName = "Unstable/Bullet Upgrade")]
public class BulletUpgrade : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeName;
    public Sprite icon;
    public BulletManager.BulletType bulletType;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Base Bullet Modifiers")]
    public List<BaseBulletModifier> baseModifiers = new List<BaseBulletModifier>();
    
    [Header("Bullet-Specific Modifiers")]
    public List<SpecificBulletModifier> specificModifiers = new List<SpecificBulletModifier>();
    
    [Header("Player Modifiers")]
    public List<PlayerModifier> playerModifiers = new List<PlayerModifier>();
    
    [Header("Glitch Settings")]
    public bool canGlitchName = true;
    public bool canGlitchIcon = true;
    public bool canGlitchModifiers = true;
}

[System.Serializable]
public class BaseBulletModifier
{
    public enum ModifierType
    {
        Damage,
        Speed,
        Size,
        Lifetime,
        UnstableEffectInterval
    }
    
    public ModifierType type;
    public float value; // Multiplier or direct value depending on type
    public bool isPositive = true;
    [TextArea(2, 3)]
    public string description;
    public Sprite icon;
    public bool canGlitch = true;
}

[System.Serializable]
public class SpecificBulletModifier
{
    // This is used to specify which bullet script the modifier applies to
    public enum BulletScript
    {
        BouncingBullet,
        ChainReactionBullet,
        FractalBullet,
        GravityWellBullet,
        HomingBullet,
        PhasingBullet,
        ProbabilityWaveBullet,
        QuantumEntanglementBullet,
        RealityShifterBullet,
        SchrodingerBullet,
        SplittingBullet,
        TimeFluxBullet,
        UnstableBullet
    }
    
    public BulletScript targetScript;
    public string propertyName; // The name of the property to modify
    public float value; // Value to set or multiply by
    public bool isMultiplier = true; // Whether to multiply or set directly
    public bool isPositive = true;
    [TextArea(2, 3)]
    public string description;
    public Sprite icon;
    public bool canGlitch = true;
}

[System.Serializable]
public class PlayerModifier
{
    public enum ModifierType
    {
        MoveSpeed,
        DashSpeed,
        DashDuration,
        DashCooldown,
        FireRate,
        Health
    }
    
    public ModifierType type;
    public float value;
    public bool isPositive = true;
    [TextArea(2, 3)]
    public string description;
    public Sprite icon;
    public bool canGlitch = true;
}