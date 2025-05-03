using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchrodingerBullet : Bullet
{
    [Header("Schrödinger Settings")]
    [SerializeField] private List<GameObject> possibleBulletPrefabs = new List<GameObject>();
    [SerializeField] private float collapseRadius = 3f; // Radius at which bullet "collapses" to a specific type
    [SerializeField] private float probabilityShiftInterval = 0.5f; // How often probabilities change
    [SerializeField] private LayerMask collapseDetectionLayers; // Layers that trigger collapse
    
    [Header("Probability Distribution")]
    [SerializeField] private List<float> spawnWeights = new List<float>(); // Weight for each bullet type
    
    private bool hasCollapsed = false;
    private List<float> normalizedWeights = new List<float>();
    private float probabilityShiftTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        
        // Validate prefabs and weights
        if (possibleBulletPrefabs.Count == 0)
        {
            Debug.LogError("SchrodingerBullet has no possible bullet prefabs assigned!");
            return;
        }
        
        // Ensure weights match prefab count
        if (spawnWeights.Count != possibleBulletPrefabs.Count)
        {
            // Default to equal weights
            spawnWeights.Clear();
            for (int i = 0; i < possibleBulletPrefabs.Count; i++)
            {
                spawnWeights.Add(1f);
            }
        }
        
        // Calculate normalized weights
        CalculateNormalizedWeights();
        
        // Visual indicator of superposition
        if (spriteRenderer != null)
        {
            // Start with a shifting rainbow effect to represent superposition
            StartCoroutine(SuperpositionVisualEffect());
        }
    }
    
    private void CalculateNormalizedWeights()
    {
        normalizedWeights.Clear();
        
        float totalWeight = 0f;
        foreach (float weight in spawnWeights)
        {
            totalWeight += weight;
        }
        
        // Normalize weights to sum to 1
        foreach (float weight in spawnWeights)
        {
            normalizedWeights.Add(weight / totalWeight);
        }
    }
    
    protected override void OnBulletUpdate()
    {
        if (hasCollapsed)
        {
            return;
        }
        
        // Shift probabilities periodically
        probabilityShiftTimer += Time.deltaTime;
        if (probabilityShiftTimer >= probabilityShiftInterval)
        {
            probabilityShiftTimer = 0f;
            ShiftProbabilities();
        }
        
        // Check if we're near something that triggers collapse
        Collider2D[] collapseTargets = Physics2D.OverlapCircleAll(
            transform.position, 
            collapseRadius, 
            collapseDetectionLayers
        );
        
        if (collapseTargets.Length > 0)
        {
            CollapseSuperposition();
        }
    }
    
    private void ShiftProbabilities()
    {
        // Randomly adjust weights to simulate quantum uncertainty
        for (int i = 0; i < spawnWeights.Count; i++)
        {
            spawnWeights[i] += Random.Range(-0.2f, 0.2f);
            spawnWeights[i] = Mathf.Max(0.1f, spawnWeights[i]); // Keep weights positive
        }
        
        CalculateNormalizedWeights();
    }
    
    private void CollapseSuperposition()
    {
        hasCollapsed = true;
        
        // Choose a bullet type based on current probability distribution
        float randomValue = Random.value;
        float cumulativeProbability = 0f;
        int selectedIndex = 0;
        
        for (int i = 0; i < normalizedWeights.Count; i++)
        {
            cumulativeProbability += normalizedWeights[i];
            if (randomValue <= cumulativeProbability)
            {
                selectedIndex = i;
                break;
            }
        }
        
        // Spawn the selected bullet type
        GameObject selectedBulletPrefab = possibleBulletPrefabs[selectedIndex];
        GameObject newBullet = Instantiate(selectedBulletPrefab, transform.position, transform.rotation);
        
        // Initialize the new bullet
        Bullet bulletComponent = newBullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Transfer speed and damage
            bulletComponent.Initialize(rb.linearVelocity.magnitude / speed, damage);
        }
        
        // Create a visual "collapse" effect
        if (spriteRenderer != null)
        {
            StartCoroutine(CollapseVisualEffect(newBullet));
        }
        else
        {
            // If no visual effect, just destroy immediately
            Destroy(gameObject);
        }
    }
    
    private IEnumerator SuperpositionVisualEffect()
    {
        float hue = 0f;
        
        while (!hasCollapsed)
        {
            // Cycle through colors
            hue = (hue + Time.deltaTime * 0.5f) % 1f;
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
            
            // Add a pulsing effect to represent quantum uncertainty
            float scale = 1f + 0.2f * Mathf.Sin(Time.time * 8f);
            transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
    }
    
    private IEnumerator CollapseVisualEffect(GameObject newBullet)
    {
        // Flash white
        spriteRenderer.color = Color.white;
        
        // Scale down rapidly
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            transform.localScale = Vector3.one * (1f - t);
            
            // Fade out
            Color color = spriteRenderer.color;
            color.a = 1f - t;
            spriteRenderer.color = color;
            
            yield return null;
        }
        
        // Destroy this bullet once effect is complete
        Destroy(gameObject);
    }
    
    protected override void OnBulletHit(Collider2D other)
    {
        // If we hit something before collapsing, collapse immediately
        if (!hasCollapsed)
        {
            CollapseSuperposition();
        }
        else
        {
            // Otherwise use default behavior
            base.OnBulletHit(other);
        }
    }
}