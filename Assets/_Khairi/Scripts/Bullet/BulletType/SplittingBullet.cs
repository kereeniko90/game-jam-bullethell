using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SplittingBullet : Bullet
{
    [Header("Split Settings")]
    [SerializeField] private GameObject childBulletPrefab;
    [SerializeField] private int splitCount = 3;
    [SerializeField] private float spreadAngle = 120f;
    [SerializeField] private float childSpeedMultiplier = 0.8f;
    
    protected override void OnBulletHit(Collider2D other)
    {
        // Create multiple bullets in a spread pattern
        if (childBulletPrefab != null)
        {
            SplitBullet();
        }
    }
    
    private void SplitBullet()
    {
        // Calculate angle between bullets
        float angleStep = spreadAngle / (splitCount - 1);
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < splitCount; i++)
        {
            // Calculate rotation for this child bullet
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle) * transform.rotation;
            
            // Create child bullet
            GameObject childBullet = Instantiate(childBulletPrefab, transform.position, rotation);
            
            // Initialize child bullet with reduced damage and speed
            Bullet bulletComponent = childBullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(childSpeedMultiplier, Mathf.Max(1, damage / 2));
            }
        }
    }
}
