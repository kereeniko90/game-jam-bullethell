using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BulletType
{
    public string name;
    public GameObject bulletPrefab;
    public Sprite bulletSprite;
    public Color bulletColor = Color.white;
    [Range(0f, 1f)]
    public float weight = 1f; // Chance of this bullet being selected when random
}

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }
    
    [Header("Bullet Types")]
    [SerializeField] private List<BulletType> bulletTypes = new List<BulletType>();
    [SerializeField] private BulletType defaultBulletType;
    
    [Header("Unstable Settings")]
    [SerializeField] private bool enableUnstableMode = false;
    [SerializeField] private float unstableBulletChance = 0.3f;
    
    // Current player bullet type
    private BulletType currentPlayerBulletType;
    
    // Pool of active bullets (useful for tracking or modifying all bullets)
    private List<Bullet> activeBullets = new List<Bullet>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set initial bullet type
        if (defaultBulletType != null)
        {
            currentPlayerBulletType = defaultBulletType;
        }
        else if (bulletTypes.Count > 0)
        {
            currentPlayerBulletType = bulletTypes[0];
        }
    }
    
    // Create a bullet at the specified position and rotation
    public Bullet CreateBullet(Vector3 position, Quaternion rotation, bool isPlayerBullet = true, string bulletTypeName = "")
    {
        BulletType typeToUse;
        
        // Determine which bullet type to use
        if (!string.IsNullOrEmpty(bulletTypeName))
        {
            // Try to find the specified type
            typeToUse = bulletTypes.Find(type => type.name == bulletTypeName);
            if (typeToUse == null)
            {
                Debug.LogWarning($"Bullet type '{bulletTypeName}' not found. Using default.");
                typeToUse = currentPlayerBulletType;
            }
        }
        else if (isPlayerBullet)
        {
            // Use current player bullet type
            typeToUse = currentPlayerBulletType;
            
            // Apply unstable mode chance for player bullets
            if (enableUnstableMode && Random.value < unstableBulletChance)
            {
                // Get a random bullet type instead
                typeToUse = GetRandomBulletType();
            }
        }
        else
        {
            // For enemy bullets, use default or random
            typeToUse = defaultBulletType ?? GetRandomBulletType();
        }
        
        // Create the bullet
        if (typeToUse?.bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(typeToUse.bulletPrefab, position, rotation);
            
            // Get the Bullet component
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                // Apply visual customization if needed
                SpriteRenderer spriteRenderer = bulletObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    if (typeToUse.bulletSprite != null)
                    {
                        spriteRenderer.sprite = typeToUse.bulletSprite;
                    }
                    spriteRenderer.color = typeToUse.bulletColor;
                }
                
                // Initialize the bullet
                bullet.Initialize();
                
                // Add to active bullets list
                activeBullets.Add(bullet);
                
                // Handle cleanup when bullet is destroyed
                StartCoroutine(TrackBulletDestruction(bullet));
                
                return bullet;
            }
            else
            {
                Debug.LogError("Bullet prefab does not have a Bullet component!");
                Destroy(bulletObj);
                return null;
            }
        }
        
        Debug.LogError("Failed to create bullet: No valid bullet prefab found.");
        return null;
    }
    
    // Track when bullets are destroyed to remove them from our list
    private System.Collections.IEnumerator TrackBulletDestruction(Bullet bullet)
    {
        // Wait until the bullet is destroyed
        yield return new WaitUntil(() => bullet == null);
        
        // Remove from active bullets list
        activeBullets.Remove(bullet);
    }
    
    // Get a random bullet type based on weights
    private BulletType GetRandomBulletType()
    {
        if (bulletTypes.Count == 0) return null;
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (BulletType type in bulletTypes)
        {
            totalWeight += type.weight;
        }
        
        // No valid weights
        if (totalWeight <= 0f) return bulletTypes[0];
        
        // Select based on weight
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (BulletType type in bulletTypes)
        {
            currentWeight += type.weight;
            if (randomValue <= currentWeight)
            {
                return type;
            }
        }
        
        // Fallback (shouldn't happen)
        return bulletTypes[0];
    }
    
    // Set current player bullet type
    public void SetPlayerBulletType(string typeName)
    {
        BulletType newType = bulletTypes.Find(type => type.name == typeName);
        if (newType != null)
        {
            currentPlayerBulletType = newType;
            
            // Notify player or UI of bullet type change
            // You might want to add an event system here
        }
        else
        {
            Debug.LogWarning($"Bullet type '{typeName}' not found.");
        }
    }
    
    // Toggle unstable mode
    public void SetUnstableMode(bool enabled)
    {
        enableUnstableMode = enabled;
    }
    
    // Apply an effect to all active bullets (useful for power-ups)
    public void ModifyAllBullets(System.Action<Bullet> modifierFunction)
    {
        foreach (Bullet bullet in new List<Bullet>(activeBullets))
        {
            if (bullet != null)
            {
                modifierFunction(bullet);
            }
        }
    }
}