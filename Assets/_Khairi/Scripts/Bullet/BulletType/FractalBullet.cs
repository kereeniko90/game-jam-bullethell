using UnityEngine;

public class FractalBullet : Bullet
{
    [Header("Fractal Settings")]
    [SerializeField] private GameObject miniatureBulletPrefab;
    [SerializeField] private int fractureCount = 5; // How many smaller bullets to create
    [SerializeField] private float fractureRadius = 2f; // How far from center the fractures spread
    [SerializeField] private float proximityTriggerRadius = 4f; // How close enemies need to be to trigger fracture
    [SerializeField] private float damageDivider = 2f; // Each fracture does original damage divided by this
    [SerializeField] private int maxFractureGenerations = 2; // How many times bullets can subdivide (0=none, 1=once, etc)
    [SerializeField] private LayerMask enemyDetectionLayers;
    
    private int currentGeneration = 0;
    private bool hasFractured = false;
    
    public void SetGeneration(int generation)
    {
        currentGeneration = generation;
        
        // Smaller with each generation
        transform.localScale = Vector3.one * Mathf.Pow(0.7f, generation);
        
        // Adjust color to indicate generation
        if (spriteRenderer != null)
        {
            float hue = 0.3f - (0.3f * generation / maxFractureGenerations);
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        // If generation wasn't set externally, assume it's the first generation
        if (currentGeneration == 0)
        {
            SetGeneration(0);
        }
    }
    
    protected override void OnBulletUpdate()
    {
        // Don't fracture if we've already done so or reached max generations
        if (hasFractured || currentGeneration >= maxFractureGenerations)
        {
            return;
        }
        
        // Check for nearby enemies to trigger fracture
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(
            transform.position, 
            proximityTriggerRadius, 
            enemyDetectionLayers
        );
        
        if (nearbyEnemies.Length > 0)
        {
            Fracture();
        }
    }
    
    private void Fracture()
    {
        hasFractured = true;
        
        // Create fracture bullets
        for (int i = 0; i < fractureCount; i++)
        {
            // Calculate spread angle
            float angle = 360f * i / fractureCount;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            
            // Create fracture bullet with slight randomness
            Vector3 position = transform.position + new Vector3(
                direction.x * Random.Range(0.1f, 0.3f),
                direction.y * Random.Range(0.1f, 0.3f),
                0f
            );
            
            GameObject fractureBullet = Instantiate(
                miniatureBulletPrefab != null ? miniatureBulletPrefab : gameObject,
                position,
                Quaternion.identity
            );
            
            // Configure the fracture bullet
            FractalBullet fractalComponent = fractureBullet.GetComponent<FractalBullet>();
            if (fractalComponent != null)
            {
                // Set next generation
                fractalComponent.SetGeneration(currentGeneration + 1);
                
                // Calculate damage
                int fractureDamage = Mathf.Max(1, Mathf.RoundToInt(damage / damageDivider));
                
                // Initialize with direction and speed
                fractalComponent.Initialize(1f, fractureDamage, direction);
            }
            else
            {
                // If it's not a fractal bullet, just initialize as regular bullet
                Bullet bulletComponent = fractureBullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    int fractureDamage = Mathf.Max(1, Mathf.RoundToInt(damage / damageDivider));
                    bulletComponent.Initialize(1f, fractureDamage, direction);
                }
            }
        }
        
        // Destroy original bullet
        Destroy(gameObject);
    }
    
    protected override void OnBulletHit(Collider2D other)
    {
        // On direct hit, we fracture instead of normal hit behavior, 
        // but only if we haven't reached max generations
        if (!hasFractured && currentGeneration < maxFractureGenerations)
        {
            Fracture();
        }
    }
}