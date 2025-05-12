using UnityEngine;
using DG.Tweening;

public class HealItem : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 2;
    [SerializeField] private float lifetime = 10f; // How long the item stays in the scene
    [SerializeField] private GameObject pickupEffect; // Optional visual effect
    
    [Header("Animation Settings")]
    [SerializeField] private float floatDistance = 0.3f;
    [SerializeField] private float floatDuration = 1.0f;
    [SerializeField] private Ease floatEase = Ease.InOutSine;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private Vector3 startPosition;
    private Tween floatTween;
    private SpriteRenderer spriteRenderer;
    private bool isBeingDestroyed = false;
    [SerializeField] private GameObject parentObject;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If no sprite renderer found, try to find it in children
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Start()
    {
        // Store initial position
        startPosition = transform.position;
        
        // Create floating animation with smooth ping-pong effect
        CreateSmoothFloatingAnimation();
        
        // Spawn effect to indicate this is a new drop
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).OnComplete(() => {
            transform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine);
        });
        
        // Start the lifetime countdown with the fade-out animation
        StartDisappearSequence(lifetime);
    }
    
    private void CreateSmoothFloatingAnimation()
    {
        // Create a smooth ping-pong floating effect
        floatTween = transform.DOMoveY(startPosition.y + floatDistance, floatDuration)
            .SetEase(floatEase)
            .SetLoops(-1, LoopType.Yoyo); // Yoyo makes it go back and forth smoothly
    }
    
    private void StartDisappearSequence(float delay)
    {
        // Create a sequence that will fade out the item when its lifetime expires
        Sequence disappearSequence = DOTween.Sequence();
        
        // Wait for the lifetime duration
        disappearSequence.AppendInterval(delay - fadeOutDuration);
        
        // Start flashing when nearing the end of lifetime
        disappearSequence.Append(
            DOTween.To(() => 1f, x => {
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = Mathf.PingPong(Time.time * 5, 1);
                    spriteRenderer.color = color;
                }
            }, 0f, fadeOutDuration * 0.5f)
        );
        
        // Then fade out and shrink
        disappearSequence.Append(
            DOTween.To(() => 1f, x => {
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = x;
                    spriteRenderer.color = color;
                }
            }, 0f, fadeOutDuration * 0.5f)
        );
        
        disappearSequence.Join(
            transform.DOScale(0.1f, fadeOutDuration * 0.5f)
                .SetEase(Ease.InQuad)
        );
        
        // Finally destroy the object
        disappearSequence.OnComplete(() => {
            if (!isBeingDestroyed)
            {
                isBeingDestroyed = true;
                Destroy(parentObject);
            }
        });
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // Find the player's health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            // Heal the player if they have a health component
            if (playerHealth != null && !isBeingDestroyed)
            {
                isBeingDestroyed = true;
                playerHealth.Heal(healAmount);
                
                // Spawn pickup effect if provided
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Kill running animation
                if (floatTween != null)
                {
                    floatTween.Kill();
                }
                DOTween.Kill(transform);
                
                // Add pickup animation
                transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                    // Destroy the heal item
                    Destroy(parentObject);
                });
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up DOTween animations when object is destroyed
        DOTween.Kill(transform);
    }
}