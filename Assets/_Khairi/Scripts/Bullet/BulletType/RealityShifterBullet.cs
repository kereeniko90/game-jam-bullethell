using System.Collections;
using UnityEngine;

public class RealityShifterBullet : Bullet
{
    [Header("Reality Shifter Settings")]
    [SerializeField] private bool randomExit = true; // Whether exit point can be random
    [SerializeField] private float randomExitChance = 0.3f; // Chance for random exit if enabled
    [SerializeField] private float screenEdgeBuffer = 0.1f; // Distance from edge to trigger shift (in percent)
    
    private Camera mainCamera;
    
    protected override void Start()
    {
        base.Start();
        mainCamera = Camera.main;
    }
    
    protected override void OnBulletUpdate()
    {
        // Check if bullet is near screen edge
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        
        // If we're outside the viewport bounds, shift to opposite side
        if (viewportPosition.x < -0.1f || viewportPosition.x > 1.1f || 
            viewportPosition.y < -0.1f || viewportPosition.y > 1.1f)
        {
            ShiftReality(viewportPosition);
        }
    }
    
    private void ShiftReality(Vector3 viewportPosition)
    {
        Vector3 newPosition = transform.position;
        
        // Determine if we use random exit
        bool useRandomExit = randomExit && Random.value < randomExitChance;
        
        if (useRandomExit)
        {
            // Random exit point
            Vector2 randomViewport = new Vector2(Random.value, Random.value);
            newPosition = mainCamera.ViewportToWorldPoint(randomViewport);
            
            // Ensure we're not too close to where we entered
            if (Vector2.Distance(transform.position, newPosition) < 1f)
            {
                // Try opposite side instead
                randomViewport = new Vector2(1f - randomViewport.x, 1f - randomViewport.y);
                newPosition = mainCamera.ViewportToWorldPoint(randomViewport);
            }
        }
        else
        {
            // Normal opposite-side exit
            // Shift X if we crossed horizontal boundary
            if (viewportPosition.x < 0f || viewportPosition.x > 1f)
            {
                float newX = viewportPosition.x < 0f ? 1f - screenEdgeBuffer : screenEdgeBuffer;
                Vector3 newViewportPos = new Vector3(newX, viewportPosition.y, viewportPosition.z);
                newPosition = mainCamera.ViewportToWorldPoint(newViewportPos);
            }
            
            // Shift Y if we crossed vertical boundary
            if (viewportPosition.y < 0f || viewportPosition.y > 1f)
            {
                float newY = viewportPosition.y < 0f ? 1f - screenEdgeBuffer : screenEdgeBuffer;
                Vector3 newViewportPos = new Vector3(viewportPosition.x, newY, viewportPosition.z);
                newPosition = mainCamera.ViewportToWorldPoint(newViewportPos);
            }
        }
        
        // Update position, keeping the Z coordinate unchanged for 2D
        newPosition.z = transform.position.z;
        transform.position = newPosition;
        
        // Visual effect when shifting
        if (spriteRenderer != null)
        {
            StartCoroutine(ShiftFlashEffect());
        }
    }
    
    private IEnumerator ShiftFlashEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}