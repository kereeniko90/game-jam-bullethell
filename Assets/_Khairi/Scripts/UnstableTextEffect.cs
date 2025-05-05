using UnityEngine;
using TMPro;
using DG.Tweening;

public class UnstableTextEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    
    [Header("Glitch Settings")]
    [SerializeField] private float glitchIntensity = 1f;
    [SerializeField] private float glitchInterval = 0.5f;
    [SerializeField] private float glitchDuration = 0.2f;
    
    [Header("Color Settings")]
    [SerializeField] private Color[] glitchColors = new Color[] { Color.cyan, Color.magenta };
    [SerializeField] private float colorChangeSpeed = 0.5f;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    
    private Sequence colorSequence;
    private Sequence glitchSequence;
    private string originalText;
    
    private void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
            
        originalText = textComponent.text;
        StartGlitchEffects();
    }
    
    public void StartGlitchEffects()
    {
        // Kill previous tweens if they exist
        if (colorSequence != null) colorSequence.Kill();
        if (glitchSequence != null) glitchSequence.Kill();
        
        // Start color cycle effect
        if (glitchColors != null && glitchColors.Length > 1)
        {
            StartColorCycle();
        }
        
        // Start glitch effect
        StartGlitchSequence();
    }
    
    private void StartColorCycle()
    {
        colorSequence = DOTween.Sequence();
        
        // Add each color transition
        for (int i = 0; i < glitchColors.Length; i++)
        {
            Color targetColor = glitchColors[i];
            colorSequence.Append(textComponent.DOColor(targetColor, colorChangeSpeed));
        }
        
        // Make it loop forever
        colorSequence.SetLoops(-1, LoopType.Restart);
    }
    
    private void StartGlitchSequence()
    {
        glitchSequence = DOTween.Sequence();
        
        // Add delay before glitch
        glitchSequence.AppendInterval(glitchInterval);
        
        // Add the glitch effect
        glitchSequence.Append(textComponent.rectTransform
            .DOShakePosition(glitchDuration, shakeStrength * glitchIntensity, shakeVibrato, shakeRandomness)
            .SetEase(Ease.OutElastic));
        
        // Add scale glitch
        glitchSequence.Join(textComponent.rectTransform
            .DOScale(Vector3.one * Random.Range(0.9f, 1.1f), glitchDuration * 0.5f)
            .SetEase(Ease.OutElastic)
            .SetLoops(2, LoopType.Yoyo));
            
        // Add text scrambling
        glitchSequence.Join(
            DOTween.To(() => 0f, x => {
                if (Random.value < 0.3f * glitchIntensity)
                {
                    ScrambleTextGlitch();
                }
                else
                {
                    textComponent.text = originalText;
                }
            }, 1f, glitchDuration)
        );
        
        // Make it loop forever by calling itself again when done
        glitchSequence.OnComplete(() => {
            textComponent.text = originalText; // Reset text
            StartGlitchSequence();
        });
    }
    
    private void ScrambleTextGlitch()
    {
        string scrambledText = "";
        string glitchChars = "Æ€ØŁÐÞŦŊĦŻĐŁĘŔŘØж";
        
        for (int i = 0; i < originalText.Length; i++)
        {
            // Randomly replace characters
            if (Random.value < 0.3f * glitchIntensity)
            {
                scrambledText += glitchChars[Random.Range(0, glitchChars.Length)];
            }
            else
            {
                scrambledText += originalText[i];
            }
        }
        
        textComponent.text = scrambledText;
    }
    
    private void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        if (colorSequence != null) colorSequence.Kill();
        if (glitchSequence != null) glitchSequence.Kill();
    }
    
    // Public method to change glitch intensity during gameplay
    public void SetGlitchIntensity(float intensity)
    {
        glitchIntensity = intensity;
        
        // Restart effects with new intensity
        StartGlitchEffects();
    }
    
    // Public method to update the base text
    public void SetText(string newText)
    {
        originalText = newText;
        textComponent.text = newText;
    }
    
    // You can also add a method to temporarily increase glitch intensity
    // useful for when players take damage or something unstable happens in-game
    public void GlitchBurst(float burstIntensity = 3f, float burstDuration = 1f)
    {
        float originalIntensity = glitchIntensity;
        
        // Increase the glitch intensity temporarily
        SetGlitchIntensity(burstIntensity);
        
        // Reset after duration
        DOTween.Sequence()
            .AppendInterval(burstDuration)
            .AppendCallback(() => {
                SetGlitchIntensity(originalIntensity);
            });
    }
}