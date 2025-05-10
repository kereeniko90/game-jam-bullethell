using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class CanvasDigitalGlitch : MonoBehaviour
{
    [Header("Glitch Settings")]
    [SerializeField] private float glitchInterval = 2f; // Time between major glitches
    [SerializeField] private float glitchDuration = 0.5f; // How long each glitch lasts
    [SerializeField] private int minSquaresPerGlitch = 3;
    [SerializeField] private int maxSquaresPerGlitch = 10;
    
    [Header("Square Settings")]
    [SerializeField] private Color[] glitchColors = new Color[] { 
        new Color(0, 1, 1, 0.5f),  // Cyan
        new Color(1, 0, 1, 0.5f),  // Magenta
        new Color(1, 1, 0, 0.5f),  // Yellow
        new Color(1, 1, 1, 0.5f)   // White
    };
    [SerializeField] private Vector2 minSquareSize = new Vector2(20, 20);
    [SerializeField] private Vector2 maxSquareSize = new Vector2(100, 100);
    
    [Header("Canvas References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform canvasRect;
    
    // Pool of reusable square objects
    private List<RectTransform> squarePool = new List<RectTransform>();
    private List<RectTransform> activeSquares = new List<RectTransform>();
    
    private void Start()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponent<Canvas>();
            
        if (canvasRect == null && targetCanvas != null)
            canvasRect = targetCanvas.GetComponent<RectTransform>();
            
        // Initialize square pool
        CreateSquarePool(20); // Create 20 reusable squares
        
        // Begin glitch loop
        StartGlitchLoop();
    }
    
    private void CreateSquarePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject squareObj = new GameObject("GlitchSquare_" + i);
            squareObj.transform.SetParent(transform, false);
            
            RectTransform rectTransform = squareObj.AddComponent<RectTransform>();
            Image image = squareObj.AddComponent<Image>();
            
            // Set initial properties
            rectTransform.sizeDelta = minSquareSize;
            image.color = Color.clear; // Start invisible
            
            // Add to pool
            squarePool.Add(rectTransform);
            squareObj.SetActive(false);
        }
    }
    
    private void StartGlitchLoop()
    {
        // Create a sequence that loops indefinitely
        Sequence glitchLoop = DOTween.Sequence();
        
        // Wait for interval with some randomness
        glitchLoop.AppendInterval(Random.Range(glitchInterval * 0.7f, glitchInterval * 1.3f));
        
        // Add glitch
        glitchLoop.AppendCallback(() => {
            TriggerGlitchEffect();
        });
        
        // Loop forever
        glitchLoop.SetLoops(-1);
    }
    
    private void TriggerGlitchEffect()
    {
        // Create a sequence for this glitch instance
        Sequence glitchEffect = DOTween.Sequence();
        
        // Determine how many squares to show
        int squareCount = Random.Range(minSquaresPerGlitch, maxSquaresPerGlitch + 1);
        
        // Activate squares
        for (int i = 0; i < squareCount; i++)
        {
            RectTransform square = GetSquareFromPool();
            if (square != null)
            {
                // Configure the square
                ConfigureGlitchSquare(square);
                
                // Add to active squares
                activeSquares.Add(square);
            }
        }
        
        // Clear squares after the effect duration
        glitchEffect.AppendInterval(glitchDuration);
        glitchEffect.AppendCallback(() => {
            ClearActiveSquares();
        });
        
        // Add some short follow-up glitches for a more natural effect
        if (Random.value > 0.5f)
        {
            glitchEffect.AppendInterval(Random.Range(0.1f, 0.3f));
            glitchEffect.AppendCallback(() => {
                TriggerSmallGlitch();
            });
        }
    }
    
    private void TriggerSmallGlitch()
    {
        // A smaller, shorter version of the main glitch
        int squareCount = Random.Range(1, 4);
        
        for (int i = 0; i < squareCount; i++)
        {
            RectTransform square = GetSquareFromPool();
            if (square != null)
            {
                ConfigureGlitchSquare(square);
                activeSquares.Add(square);
            }
        }
        
        // Clear after a shorter duration
        DOTween.Sequence()
            .AppendInterval(glitchDuration * 0.3f)
            .AppendCallback(() => {
                ClearActiveSquares();
            });
    }
    
    private RectTransform GetSquareFromPool()
    {
        // Find an inactive square in the pool
        foreach (RectTransform square in squarePool)
        {
            if (!square.gameObject.activeInHierarchy)
            {
                square.gameObject.SetActive(true);
                return square;
            }
        }
        
        // If we couldn't find one, create a new one
        GameObject newSquareObj = new GameObject("GlitchSquare_Dynamic");
        newSquareObj.transform.SetParent(transform, false);
        
        RectTransform rectTransform = newSquareObj.AddComponent<RectTransform>();
        newSquareObj.AddComponent<Image>();
        
        squarePool.Add(rectTransform);
        return rectTransform;
    }
    
    private void ConfigureGlitchSquare(RectTransform square)
    {
        if (canvasRect == null) return;
        
        // Random position within canvas
        Vector2 randomPos = new Vector2(
            Random.Range(-canvasRect.rect.width/2, canvasRect.rect.width/2),
            Random.Range(-canvasRect.rect.height/2, canvasRect.rect.height/2)
        );
        
        // Random size
        Vector2 randomSize = new Vector2(
            Random.Range(minSquareSize.x, maxSquareSize.x),
            Random.Range(minSquareSize.y, maxSquareSize.y)
        );
        
        // Random color from the array
        Color randomColor = glitchColors[Random.Range(0, glitchColors.Length)];
        
        // Set properties
        square.anchoredPosition = randomPos;
        square.sizeDelta = randomSize;
        
        // Get the image component and set its color
        Image squareImage = square.GetComponent<Image>();
        if (squareImage != null)
        {
            squareImage.color = randomColor;
            
            // Sometimes add a flicker effect
            if (Random.value > 0.7f)
            {
                Sequence flicker = DOTween.Sequence();
                
                for (int i = 0; i < 3; i++)
                {
                    flicker.Append(squareImage.DOFade(Random.Range(0.3f, 0.8f), 0.05f));
                }
                
                // End with the original alpha
                flicker.Append(squareImage.DOFade(randomColor.a, 0.05f));
            }
        }
        
        // Sometimes add a jitter effect
        if (Random.value > 0.7f)
        {
            Vector2 originalPos = square.anchoredPosition;
            
            DOTween.Sequence()
                .Append(square.DOAnchorPos(
                    originalPos + new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)), 
                    0.05f))
                .Append(square.DOAnchorPos(originalPos, 0.05f))
                .SetLoops(2);
        }
    }
    
    private void ClearActiveSquares()
    {
        foreach (RectTransform square in activeSquares)
        {
            if (square != null && square.gameObject != null)
            {
                // Fade out with a quick animation instead of immediately hiding
                Image squareImage = square.GetComponent<Image>();
                if (squareImage != null)
                {
                    squareImage.DOFade(0f, 0.1f).OnComplete(() => {
                        square.gameObject.SetActive(false);
                    });
                }
                else
                {
                    square.gameObject.SetActive(false);
                }
            }
        }
        
        activeSquares.Clear();
    }
    
    // Call this method to trigger a glitch manually (e.g., on button press)
    public void TriggerManualGlitch()
    {
        TriggerGlitchEffect();
    }
    
    private void OnDestroy()
    {
        // Clean up DOTween animations
        DOTween.KillAll();
    }
}