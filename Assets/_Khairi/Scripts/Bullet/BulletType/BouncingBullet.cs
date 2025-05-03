using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BouncingBullet : Bullet
{
    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceDamping = 0.8f; // Energy lost on bounce
    [SerializeField] private LayerMask bounceSurfaces;
    
    private int bounceCount = 0;
    
    protected override void OnBulletUpdate()
    {
        // Ensure bullet rotation matches velocity direction
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Check if this is a bounceable surface
        if (((1 << other.gameObject.layer) & bounceSurfaces) != 0)
        {
            // We hit a wall - bounce!
            if (bounceCount < maxBounces)
            {
                Bounce(other);
                bounceCount++;
                return; // Don't destroy the bullet
            }
        }
        
        // Otherwise use regular hit logic
        base.OnTriggerEnter2D(other);
    }
    
    private void Bounce(Collider2D surface)
    {
        // Get the normal of the surface
        Vector2 normal = surface.bounds.center - transform.position;
        normal.Normalize();
        
        // Calculate reflection
        Vector2 velocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.Reflect(velocity, normal) * bounceDamping;
        
        // Add variation to make it more unstable
        if (isUnstable)
        {
            rb.linearVelocity += new Vector2(
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f)
            );
        }
    }
}
