using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class UpgradeManager : MonoBehaviour
{
    // Singleton pattern
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Settings")]
    [SerializeField] private List<BulletUpgrade> allPossibleUpgrades = new List<BulletUpgrade>();
    [SerializeField] private int upgradeChoicesCount = 3;

    [Header("Glitch Settings")]
    [SerializeField] private float baseGlitchChance = 0.0f;
    [SerializeField] private float glitchIncreasePerWave = 0.05f;
    [SerializeField] private float maxGlitchChance = 0.75f;

    [Header("References")]
    [SerializeField] private UpgradeUIController uiController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BulletManager bulletManager;

    private float currentGlitchChance;
    private int currentWave = 0;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentGlitchChance = baseGlitchChance;
    }

    private void Start()
    {
        MusicManager.Instance.PlayRandomTrack();
        TimeUI.Instance.onIntervalReached.AddListener(ShowUpgradeOptions);
        TimeUI.Instance.onWaveChanged.AddListener(OnWaveChanged);
    }

    public void OnWaveChanged(int newWave)
    {
        // Update current wave
        currentWave = newWave;

        // Increase glitch chance
        currentGlitchChance = Mathf.Min(maxGlitchChance, baseGlitchChance + (currentWave * glitchIncreasePerWave));

        Debug.Log($"Wave changed to {currentWave}, glitch chance now {currentGlitchChance}");
    }

    public void ShowUpgradeOptions()
    {
        // Get random upgrades
        List<BulletUpgrade> upgrades = GetRandomUpgrades(upgradeChoicesCount);

        // Apply glitch effects based on current wave
        List<GlitchedUpgradeData> glitchedUpgrades = ApplyGlitchesToUpgrades(upgrades);

        //Show the UI
        if (uiController != null)
        {
            uiController.ShowUpgradeUI(glitchedUpgrades);
        }
    }

    public void ApplyUpgrade(BulletUpgrade upgrade)
    {
        if (upgrade == null || playerController == null || bulletManager == null)
            return;

        // Get the bullet prefab from the manager
        GameObject bulletPrefab = bulletManager.GetBulletPrefab(upgrade.bulletType);

        // Apply the upgrade to the player
        if (bulletPrefab != null)
        {
            playerController.SetBulletPrefab(bulletPrefab);
            playerController.SwitchBulletType(upgrade.bulletType);

            ResetSpeedToDefault(bulletPrefab, upgrade);

            // Apply modifiers
            ApplyModifiers(upgrade);
        }

        // Hide the UI
        // if (uiController != null)
        // {
        //     uiController.HideUpgradeUI();
        // }
    }

    private void ResetSpeedToDefault(GameObject bulletPrefab, BulletUpgrade upgrade)
    {
        // Get the base Bullet component
        Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
        if (bulletComponent == null) return;

        // Check if the current upgrade specifically modifies speed
        bool hasSpeedModifier = false;

        foreach (BaseBulletModifier modifier in upgrade.baseModifiers)
        {
            if (modifier.type == BaseBulletModifier.ModifierType.Speed)
            {
                hasSpeedModifier = true;
                break;
            }
        }

        // If there's no speed modifier, reset to default speed
        if (!hasSpeedModifier)
        {
            // Instead of using a multiplier, we'll set to the original speed value
            // You'll need to track the original speed value somewhere
            bulletComponent.ResetSpeed();
        }
    }

    private void ApplyModifiers(BulletUpgrade upgrade)
    {
        // Apply base bullet modifiers
        foreach (BaseBulletModifier modifier in upgrade.baseModifiers)
        {
            ApplyBaseBulletModifier(modifier);
        }

        // Apply specific bullet modifiers
        foreach (SpecificBulletModifier modifier in upgrade.specificModifiers)
        {
            ApplySpecificBulletModifier(modifier, upgrade.bulletType);
        }

        // Apply player modifiers
        foreach (PlayerModifier modifier in upgrade.playerModifiers)
        {
            ApplyPlayerModifier(modifier);
        }
    }

    private List<BulletUpgrade> GetRandomUpgrades(int count)
    {
        // Create a copy of available upgrades
        List<BulletUpgrade> availableUpgrades = new List<BulletUpgrade>(allPossibleUpgrades);
        List<BulletUpgrade> selectedUpgrades = new List<BulletUpgrade>();

        // Randomly select upgrades
        for (int i = 0; i < count; i++)
        {
            if (availableUpgrades.Count == 0)
                break;

            int randomIndex = Random.Range(0, availableUpgrades.Count);
            selectedUpgrades.Add(availableUpgrades[randomIndex]);
            availableUpgrades.RemoveAt(randomIndex);
        }

        return selectedUpgrades;
    }

    private List<GlitchedUpgradeData> ApplyGlitchesToUpgrades(List<BulletUpgrade> upgrades)
    {
        List<GlitchedUpgradeData> glitchedUpgrades = new List<GlitchedUpgradeData>();

        foreach (BulletUpgrade upgrade in upgrades)
        {
            GlitchedUpgradeData glitchedData = new GlitchedUpgradeData();
            glitchedData.originalUpgrade = upgrade;

            // Apply glitches based on current chance
            glitchedData.nameGlitched = upgrade.canGlitchName && Random.value < currentGlitchChance;
            glitchedData.iconGlitched = upgrade.canGlitchIcon && Random.value < currentGlitchChance;

            // Apply glitches to base modifiers
            glitchedData.glitchedBaseModifiers = new List<bool>();
            foreach (BaseBulletModifier modifier in upgrade.baseModifiers)
            {
                bool isGlitched = upgrade.canGlitchModifiers && modifier.canGlitch && Random.value < currentGlitchChance;
                glitchedData.glitchedBaseModifiers.Add(isGlitched);
            }

            // Apply glitches to specific modifiers
            glitchedData.glitchedSpecificModifiers = new List<bool>();
            foreach (SpecificBulletModifier modifier in upgrade.specificModifiers)
            {
                bool isGlitched = upgrade.canGlitchModifiers && modifier.canGlitch && Random.value < currentGlitchChance;
                glitchedData.glitchedSpecificModifiers.Add(isGlitched);
            }

            // Apply glitches to player modifiers
            glitchedData.glitchedPlayerModifiers = new List<bool>();
            foreach (PlayerModifier modifier in upgrade.playerModifiers)
            {
                bool isGlitched = upgrade.canGlitchModifiers && modifier.canGlitch && Random.value < currentGlitchChance;
                glitchedData.glitchedPlayerModifiers.Add(isGlitched);
            }

            glitchedUpgrades.Add(glitchedData);
        }

        return glitchedUpgrades;
    }


    private void ApplyBaseBulletModifier(BaseBulletModifier modifier)
    {
        // Get the current bullet prefab from the player
        GameObject bulletPrefab = playerController.GetBulletPrefab();
        if (bulletPrefab == null) return;

        // Get the base Bullet component
        Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
        if (bulletComponent == null) return;

        // Get the current wave number
        int waveNumber = currentWave; // Replace with your wave tracking variable

        // Apply the appropriate modification based on the type
        switch (modifier.type)
        {
            case BaseBulletModifier.ModifierType.Damage:
                // Full exponential scaling for damage
                float scaledDamage = (waveNumber <= 1) ? modifier.value : Mathf.Pow(modifier.value, waveNumber);
                bulletComponent.ModifyDamage(scaledDamage);
                break;

            case BaseBulletModifier.ModifierType.Speed:
                // More conservative scaling for speed - use square root scaling
                // This ensures speed doesn't increase as dramatically
                float speedMultiplier = modifier.value;
                if (waveNumber > 1)
                {
                    // If modifier increases speed (value > 1), apply conservative scaling
                    if (speedMultiplier > 1)
                    {
                        // Formula: 1 + (original_increase * sqrt(waveNumber) / 2)
                        // Example: If modifier is 1.5 and wave is 4, result is approximately 1.5 → 2.0 instead of 5.0625
                        speedMultiplier = 1 + ((speedMultiplier - 1) * Mathf.Sqrt(waveNumber) / 2);
                    }
                    // If modifier decreases speed (value < 1), keep it as is
                    // This prevents bullets from becoming too slow
                }
                bulletComponent.ModifySpeed(speedMultiplier);
                break;

            case BaseBulletModifier.ModifierType.Size:
                // Standard scaling for size
                float scaledSize = (waveNumber <= 1) ? modifier.value : Mathf.Pow(modifier.value, waveNumber);
                // Apply a cap to prevent bullets from becoming too large
                float maxSizeMultiplier = 3f; // Maximum size multiplier
                scaledSize = Mathf.Min(scaledSize, maxSizeMultiplier);
                bulletPrefab.transform.localScale *= scaledSize;
                break;

            case BaseBulletModifier.ModifierType.Lifetime:
                // No scaling for lifetime - just use the base modifier value
                bulletComponent.ModifyLifetime(modifier.value);
                break;

            case BaseBulletModifier.ModifierType.UnstableEffectInterval:
                // No scaling for unstable effect - just use the base modifier value
                bulletComponent.ModifyUnstableEffectInterval(modifier.value);
                break;
        }
    }

    private void ApplySpecificBulletModifier(SpecificBulletModifier modifier, BulletManager.BulletType bulletType)
    {
        // Get the bullet prefab for the specific type
        GameObject bulletPrefab = bulletManager.GetBulletPrefab(bulletType);
        if (bulletPrefab == null) return;

        // Try to get the specific component type
        Component bulletComponent = null;

        switch (modifier.targetScript)
        {
            case SpecificBulletModifier.BulletScript.BouncingBullet:
                bulletComponent = bulletPrefab.GetComponent<BouncingBullet>();
                break;

            case SpecificBulletModifier.BulletScript.ChainReactionBullet:
                bulletComponent = bulletPrefab.GetComponent<ChainReactionBullet>();
                break;

            case SpecificBulletModifier.BulletScript.FractalBullet:
                bulletComponent = bulletPrefab.GetComponent<FractalBullet>();
                break;

            case SpecificBulletModifier.BulletScript.GravityWellBullet:
                bulletComponent = bulletPrefab.GetComponent<GravityWellBullet>();
                break;

            case SpecificBulletModifier.BulletScript.HomingBullet:
                bulletComponent = bulletPrefab.GetComponent<HomingBullet>();
                break;

            case SpecificBulletModifier.BulletScript.PhasingBullet:
                bulletComponent = bulletPrefab.GetComponent<PhasingBullet>();
                break;

            case SpecificBulletModifier.BulletScript.ProbabilityWaveBullet:
                bulletComponent = bulletPrefab.GetComponent<ProbabilityWaveBullet>();
                break;

            case SpecificBulletModifier.BulletScript.QuantumEntanglementBullet:
                bulletComponent = bulletPrefab.GetComponent<QuantumEntanglementBullet>();
                break;

            case SpecificBulletModifier.BulletScript.RealityShifterBullet:
                bulletComponent = bulletPrefab.GetComponent<RealityShifterBullet>();
                break;

            case SpecificBulletModifier.BulletScript.SchrodingerBullet:
                bulletComponent = bulletPrefab.GetComponent<SchrodingerBullet>();
                break;

            case SpecificBulletModifier.BulletScript.SplittingBullet:
                bulletComponent = bulletPrefab.GetComponent<SplittingBullet>();
                break;

            case SpecificBulletModifier.BulletScript.TimeFluxBullet:
                bulletComponent = bulletPrefab.GetComponent<TimeFluxBullet>();
                break;
        }

        // If we found the component, modify the property
        if (bulletComponent != null)
        {
            System.Reflection.FieldInfo field = bulletComponent.GetType().GetField(
                modifier.propertyName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
            );

            if (field != null)
            {
                // Get the current value
                object currentValue = field.GetValue(bulletComponent);

                // Modify value based on type
                if (currentValue is float)
                {
                    float floatValue = (float)currentValue;
                    if (modifier.isMultiplier)
                    {
                        floatValue *= modifier.value;
                    }
                    else
                    {
                        floatValue = modifier.value;
                    }
                    field.SetValue(bulletComponent, floatValue);
                }
                else if (currentValue is int)
                {
                    int intValue = (int)currentValue;
                    if (modifier.isMultiplier)
                    {
                        intValue = Mathf.RoundToInt(intValue * modifier.value);
                    }
                    else
                    {
                        intValue = Mathf.RoundToInt(modifier.value);
                    }
                    field.SetValue(bulletComponent, intValue);
                }
                else if (currentValue is bool)
                {
                    // For booleans, we just set the value directly
                    bool boolValue = modifier.value > 0;
                    field.SetValue(bulletComponent, boolValue);
                }
            }
        }
    }

    private void ApplyPlayerModifier(PlayerModifier modifier)
    {
        if (playerController == null) return;

        switch (modifier.type)
        {
            case PlayerModifier.ModifierType.MoveSpeed:
                playerController.ModifyMoveSpeed(modifier.value);
                break;

            case PlayerModifier.ModifierType.DashSpeed:
                playerController.ModifyDashSpeed(modifier.value);
                break;

            case PlayerModifier.ModifierType.DashDuration:
                playerController.ModifyDashDuration(modifier.value);
                break;

            case PlayerModifier.ModifierType.DashCooldown:
                playerController.ModifyDashCooldown(modifier.value);
                break;

            case PlayerModifier.ModifierType.FireRate:
                playerController.ModifyFireRate(modifier.value);
                break;

            case PlayerModifier.ModifierType.Health:
                // Assuming you have a PlayerHealth component or similar
                PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.SetMaxHealth((int)Math.Round(modifier.value));
                }
                break;
        }
    }

    // Method to be called by other systems when a special enemy is defeated
    public void OnSpecialEnemyDefeated()
    {
        // Show upgrade options
        ShowUpgradeOptions();
    }

    // Method to be called when a wave is completed
    // public void OnWaveCompleted(int waveNumber)
    // {
    //     // Update current wave
    //     currentWave = waveNumber;

    //     // Increase glitch chance
    //     currentGlitchChance = Mathf.Min(maxGlitchChance, baseGlitchChance + (currentWave * glitchIncreasePerWave));
    // }
}

[System.Serializable]
public class GlitchedUpgradeData
{
    public BulletUpgrade originalUpgrade;
    public bool nameGlitched;
    public bool iconGlitched;
    public List<bool> glitchedBaseModifiers;
    public List<bool> glitchedSpecificModifiers;
    public List<bool> glitchedPlayerModifiers;
}