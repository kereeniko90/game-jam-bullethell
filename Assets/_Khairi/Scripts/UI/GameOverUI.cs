using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameOverUI : MonoBehaviour
{   
    public static GameOverUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleScreenButton;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private RectTransform gameOverContainer;
    
    [Header("Animation Settings")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float textAnimationDuration = 0.8f;
    [SerializeField] private float buttonsFadeInDuration = 0.5f;
    [SerializeField] private Ease textAnimationEase = Ease.OutBack;
    [SerializeField] private Ease buttonsFadeEase = Ease.OutQuad;
    [SerializeField] private Vector3 textStartScale = new Vector3(0.3f, 0.3f, 0.3f);
    
    [Header("Background Animation")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float backgroundFadeDuration = 0.7f;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    
    [Header("Glitch Effects")]
    [SerializeField] private bool useGlitchEffect = true;
    [SerializeField] private GameObject glitchEffectPrefab;
    
    private string mainGameSceneName = "MainGame";
    private string titleScreenSceneName = "TitleScreen";
    
    private void Awake()
    {
        // Setup singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Hide the game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Set up button listeners
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        if (titleScreenButton != null)
        {
            titleScreenButton.onClick.AddListener(OnTitleScreenButtonClicked);
        }
    }
    
    /// <summary>
    /// Shows the game over UI with the given survival time.
    /// </summary>
    /// <param name="survivalTime">The survival time to display</param>
    public void ShowGameOver(float survivalTime = 0f)
    {
        // Activate the panel
        gameOverPanel.SetActive(true);
        
        // Stop any existing animations
        DOTween.Kill(gameOverText.transform);
        DOTween.Kill(buttonsCanvasGroup);
        DOTween.Kill(backgroundImage);
        
        // Reset UI elements
        gameOverText.transform.localScale = textStartScale;
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.alpha = 0f;
        }
        
        // Set score text if we have survival time
        if (scoreText != null && survivalTime > 0f)
        {
            scoreText.text = $"Survival Time: {TimeFormatter.SecondsToTimeString(survivalTime)}";
        }
        
        // Add glitch effect to the game over screen if enabled
        if (useGlitchEffect && glitchEffectPrefab != null)
        {
            GameObject glitchEffect = Instantiate(glitchEffectPrefab, gameOverContainer);
            glitchEffect.transform.SetAsFirstSibling(); // Put it behind other elements
        }
        
        // Start animations
        StartCoroutine(AnimateGameOver());
    }
    
    private IEnumerator AnimateGameOver()
    {
        // Make sure game is paused
        Time.timeScale = 0f;
        
        // Animate background fade in first
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);
            backgroundImage.DOColor(backgroundColor, backgroundFadeDuration)
                .SetUpdate(true); // Use unscaledTime because game is paused
        }
        
        // Wait for initial delay
        yield return new WaitForSecondsRealtime(initialDelay);
        
        // Animate game over text scale up
        gameOverText.transform.DOScale(Vector3.one, textAnimationDuration)
            .SetEase(textAnimationEase)
            .SetUpdate(true);
        
        // Add slight shake after scale is done
        gameOverText.transform.DOShakePosition(0.5f, 10f, 20, 90f)
            .SetDelay(textAnimationDuration)
            .SetUpdate(true);
        
        // Add slight color pulse animation to game over text
        Sequence colorPulse = DOTween.Sequence()
            .Append(gameOverText.DOColor(Color.red, 0.3f))
            .Append(gameOverText.DOColor(Color.white, 0.3f))
            .SetLoops(2)
            .SetDelay(textAnimationDuration + 0.2f)
            .SetUpdate(true);
        
        // Wait for text animation to complete
        yield return new WaitForSecondsRealtime(textAnimationDuration + 0.3f);
        
        // Fade in buttons
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.DOFade(1f, buttonsFadeInDuration)
                .SetEase(buttonsFadeEase)
                .SetUpdate(true);
                
            // Scale up slightly
            buttonsCanvasGroup.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            buttonsCanvasGroup.transform.DOScale(1f, buttonsFadeInDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        
        // Animate score text fade in
        if (scoreText != null)
        {
            scoreText.alpha = 0f;
            scoreText.DOFade(1f, buttonsFadeInDuration)
                .SetEase(buttonsFadeEase)
                .SetDelay(0.2f)
                .SetUpdate(true);
        }
    }
    
    private void OnRetryButtonClicked()
    {
        // Play sound effect
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Sound.SteampunkButtonClick);
        }
        
        AnimateOut(() => {
            // Resume time scale before loading
            Time.timeScale = 1f;
            
            // Reload the current game scene
            SceneManager.LoadScene(mainGameSceneName);
        });
    }
    
    private void OnTitleScreenButtonClicked()
    {
        // Play sound effect
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Sound.SteampunkButtonClick);
        }
        
        AnimateOut(() => {
            // Resume time scale before loading
            Time.timeScale = 1f;
            
            // Play title screen music
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayTitleScreen();
            }
            
            // Load the title screen scene
            SceneManager.LoadScene(titleScreenSceneName);
        });
    }
    
    private void AnimateOut(TweenCallback onComplete)
    {
        Sequence sequence = DOTween.Sequence();
        
        // Fade out buttons first
        if (buttonsCanvasGroup != null)
        {
            sequence.Append(buttonsCanvasGroup.DOFade(0f, buttonsFadeInDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true));
                
            sequence.Join(buttonsCanvasGroup.transform.DOScale(0.8f, buttonsFadeInDuration * 0.5f)
                .SetEase(Ease.InBack)
                .SetUpdate(true));
        }
        
        // Fade out score text
        if (scoreText != null)
        {
            sequence.Join(scoreText.DOFade(0f, buttonsFadeInDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true));
        }
        
        // Scale down and fade out game over text
        sequence.Append(gameOverText.transform.DOScale(textStartScale, textAnimationDuration * 0.5f)
            .SetEase(Ease.InBack)
            .SetUpdate(true));
            
        sequence.Join(gameOverText.DOFade(0f, textAnimationDuration * 0.5f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true));
        
        // Fade out background last
        if (backgroundImage != null)
        {
            sequence.Join(backgroundImage.DOFade(0f, backgroundFadeDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true));
        }
        
        sequence.OnComplete(onComplete).SetUpdate(true);
    }
    
    /// <summary>
    /// Set the scene names used for buttons.
    /// </summary>
    public void SetSceneNames(string gameSceneName, string menuSceneName)
    {
        mainGameSceneName = gameSceneName;
        titleScreenSceneName = menuSceneName;
    }
}