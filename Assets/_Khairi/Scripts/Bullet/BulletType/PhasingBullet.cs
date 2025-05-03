using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhasingBullet : Bullet
{
    [Header("Phasing Settings")]
    [SerializeField] private float phasingInterval = 0.3f;
    [SerializeField] private float phasingDuration = 0.15f;
    [SerializeField] private bool startPhased = false;
    
    private Collider2D bulletCollider;
    private bool isPhased;
    
    protected override void Awake()
    {
        base.Awake();
        bulletCollider = GetComponent<Collider2D>();
        isPhased = startPhased;
        
        // Initial state
        if (isPhased)
        {
            SetPhasedState(true);
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Start phasing regardless of unstable flag
        StartCoroutine(PhasingRoutine());
    }
    
    private IEnumerator PhasingRoutine()
    {
        while (true)
        {
            // Wait for interval
            yield return new WaitForSeconds(phasingInterval);
            
            // Phase out
            SetPhasedState(true);
            
            // Stay phased for duration
            yield return new WaitForSeconds(phasingDuration);
            
            // Phase back in
            SetPhasedState(false);
        }
    }
    
    private void SetPhasedState(bool phased)
    {
        isPhased = phased;
        
        // When phased, bullet becomes transparent and non-colliding
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = phased ? 0.3f : 1.0f;
            spriteRenderer.color = color;
        }
        
        if (bulletCollider != null)
        {
            bulletCollider.enabled = !phased;
        }
    }
    
    // Override unstable effect to do nothing (we have our own phasing)
    protected override IEnumerator UnstableEffect()
    {
        // Do nothing, phasing is handled separately
        yield break;
    }
}