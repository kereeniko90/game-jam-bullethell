using System.Collections.Generic;
using UnityEngine; 

public class BulletManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static BulletManager Instance { get; private set; }
    
    [System.Serializable]
    public enum BulletType
    {
        Normal,
        Phasing,
        Bouncing,
        Splitting,
        Homing,        
        QuantumEntanglement,
        TimeFlux,
        RealityShifter,
        ProbabilityWave,
        ChainReaction,
        GravityWell,
        Fractal,
        Schrodinger
    }
    
    [System.Serializable]
    public class BulletPrefabMapping
    {
        public BulletType type;
        public GameObject prefab;
    }
    
    [SerializeField] private List<BulletPrefabMapping> bulletPrefabs = new List<BulletPrefabMapping>();
    
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
    }
    
    public GameObject GetBulletPrefab(BulletType type)
    {
        foreach (var mapping in bulletPrefabs)
        {
            if (mapping.type == type)
            {
                return mapping.prefab;
            }
        }
        
        Debug.LogWarning("Bullet type not found: " + type);
        return null;
    }
}
