using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class TimeUI : MonoBehaviour
{
    public static TimeUI Instance { get; private set; }
    
    [Header("Current Stats Display")]
    [SerializeField] private TextMeshProUGUI timeIndicator;
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private float eventInterval = 120f;
    
    [Header("Combo Display")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject comboContainer;
    [SerializeField] private float comboDuration = 5f; // Time before combo resets if no hits
    
    [Header("Best Stats Display")]
    [SerializeField] private TextMeshProUGUI bestComboText;
    [SerializeField] private TextMeshProUGUI bestSurvivalTimeText;
    [SerializeField] private TextMeshProUGUI bestWaveText;
    
    [Header("Combo Animation Settings")]
    [SerializeField] private float punchScale = 1.5f;
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private int punchVibrato = 2;
    [SerializeField] private float punchElasticity = 0.5f;
    [SerializeField] private Color increaseColor = Color.green;
    [SerializeField] private Color resetColor = Color.red;
    [SerializeField] private Color newRecordColor = Color.yellow;
    [SerializeField] private Color waveChangeColor = new Color(0.5f, 0.8f, 1f); // Light blue for wave changes
    
    private float currentTimer;
    private int currentCombo = 0;
    private int lastWave;
    private float lastEventTime = 0f;
    private float lastComboTime = 0f;
    
    // Best stats tracking
    private int bestCombo = 0;
    private float bestSurvivalTime = 0f;
    private int bestWave = 0;
    
    // PlayerPrefs keys
    private const string BEST_COMBO_KEY = "BestCombo";
    private const string BEST_SURVIVAL_TIME_KEY = "BestSurvivalTime";
    private const string BEST_WAVE_KEY = "BestWave";
    
    public UnityEvent onIntervalReached = new UnityEvent();
    public UnityEvent<int> onWaveChanged = new UnityEvent<int>();
    public UnityEvent<int, int> onNewBestCombo = new UnityEvent<int, int>(); // (oldBest, newBest)
    public UnityEvent<float, float> onNewBestTime = new UnityEvent<float, float>(); // (oldBest, newBest)
    public UnityEvent<int, int> onNewBestWave = new UnityEvent<int, int>(); // (oldBest, newBest)
    
    private void Awake()
    {
        Instance = this;
        
        // Load best stats from PlayerPrefs
        LoadBestStats();
        
        // Hide combo text initially if no active combo
        if (comboContainer != null && currentCombo == 0)
        {
            comboContainer.SetActive(false);
        }
        
        // Update best stats display
        UpdateBestStatsDisplay();
    }
    
    void Start()
    {
        // Initialize current wave display
        if (currentWaveText != null && GameManager.Instance != null)
        {
            currentWaveText.text = $"Wave: {GameManager.Instance.currentWave}";
        }
    }
    
    void Update()
    {
        // Update the timer display
        currentTimer = GameManager.Instance.survivalTime;
        int currentWave = GameManager.Instance.currentWave;
        timeIndicator.text = TimeFormatter.SecondsToTimeString(currentTimer);
        
        // Check for new best survival time (only update when full second changes)
        if (Mathf.Floor(currentTimer) > Mathf.Floor(bestSurvivalTime))
        {
            float oldBest = bestSurvivalTime;
            bestSurvivalTime = currentTimer;
            onNewBestTime.Invoke(oldBest, bestSurvivalTime);
            SaveBestStats();
            UpdateBestStatsDisplay();
            
            // Animate best time text if available
            if (bestSurvivalTimeText != null)
            {
                AnimateNewRecord(bestSurvivalTimeText);
            }
        }
        
        if (currentWave != lastWave)
        {
            // Update wave display
            if (currentWaveText != null)
            {
                currentWaveText.text = $"Wave: {currentWave}";
                AnimateWaveChange(currentWaveText);
            }
            
            // Wave changed, trigger the event
            onWaveChanged.Invoke(currentWave);
            lastWave = currentWave;
            
            // Check for new best wave
            if (currentWave > bestWave)
            {
                int oldBest = bestWave;
                bestWave = currentWave;
                onNewBestWave.Invoke(oldBest, bestWave);
                SaveBestStats();
                UpdateBestStatsDisplay();
                
                // Animate best wave text if available
                if (bestWaveText != null)
                {
                    AnimateNewRecord(bestWaveText);
                }
            }
        }
        
        // Check if it's time to invoke the event
        if (currentTimer - lastEventTime >= eventInterval)
        {
            lastEventTime = Mathf.Floor(currentTimer / eventInterval) * eventInterval;
            
            // Invoke the event
            onIntervalReached.Invoke();
            
            Debug.Log($"Event triggered at {TimeFormatter.SecondsToTimeString(currentTimer)}");
        }
        
        // Check if combo should expire
        if (currentCombo > 0 && Time.time - lastComboTime > comboDuration)
        {
            ResetCombo();
        }
    }
    
    public void UpdateCombo()
    {
        // Increment combo
        currentCombo++;
        lastComboTime = Time.time;
        
        // Check for new best combo
        if (currentCombo > bestCombo)
        {
            int oldBest = bestCombo;
            bestCombo = currentCombo;
            onNewBestCombo.Invoke(oldBest, bestCombo);
            SaveBestStats();
            UpdateBestStatsDisplay();
            
            // Animate best combo text if available
            if (bestComboText != null)
            {
                AnimateNewRecord(bestComboText);
            }
        }
        
        // Show combo container if it was hidden
        if (comboContainer != null && !comboContainer.activeSelf)
        {
            comboContainer.SetActive(true);
        }
        
        // Update text
        if (comboText != null)
        {
            comboText.text = $"Combo: {currentCombo}x";
            
            // Stop any ongoing animations
            DOTween.Kill(comboText.transform);
            
            // Reset scale and color before starting new animation
            comboText.transform.localScale = Vector3.one;
            
            // Punch animation for increasing combo
            comboText.transform.DOPunchScale(
                Vector3.one * punchScale, 
                punchDuration, 
                punchVibrato, 
                punchElasticity
            );
            
            // Color animation
            comboText.color = increaseColor;
            comboText.DOColor(Color.white, 0.5f);
        }
    }
    
    public void ResetCombo()
    {
        // Don't animate if already at zero
        if (currentCombo == 0) return;
        
        // Store previous combo value for display
        int previousCombo = currentCombo;
        
        // Reset combo counter
        currentCombo = 0;
        
        if (comboText != null)
        {
            // Show the combo that was lost
            comboText.text = $"Combo Lost: {previousCombo}x";
            
            // Stop any ongoing animations
            DOTween.Kill(comboText.transform);
            
            // Reset scale before starting new animation
            comboText.transform.localScale = Vector3.one;
            
            // Shake animation for losing combo
            comboText.transform.DOShakeScale(0.5f, 0.5f, 5);
            
            // Color animation
            comboText.color = resetColor;
            
            // Hide the combo container after a delay
            DOTween.Sequence()
                .Append(comboText.DOColor(new Color(resetColor.r, resetColor.g, resetColor.b, 0), 1f))
                .OnComplete(() => {
                    if (comboContainer != null)
                    {
                        comboContainer.SetActive(false);
                        // Reset alpha for next time
                        comboText.color = new Color(comboText.color.r, comboText.color.g, comboText.color.b, 1f);
                    }
                });
        }
    }
    
    private void LoadBestStats()
    {
        bestCombo = PlayerPrefs.GetInt(BEST_COMBO_KEY, 0);
        bestSurvivalTime = PlayerPrefs.GetFloat(BEST_SURVIVAL_TIME_KEY, 0f);
        bestWave = PlayerPrefs.GetInt(BEST_WAVE_KEY, 0);
    }
    
    private void SaveBestStats()
    {
        PlayerPrefs.SetInt(BEST_COMBO_KEY, bestCombo);
        PlayerPrefs.SetFloat(BEST_SURVIVAL_TIME_KEY, bestSurvivalTime);
        PlayerPrefs.SetInt(BEST_WAVE_KEY, bestWave);
        PlayerPrefs.Save();
    }
    
    private void UpdateBestStatsDisplay()
    {
        if (bestComboText != null)
        {
            bestComboText.text = $"Best Combo: {bestCombo}x";
        }
        
        if (bestSurvivalTimeText != null)
        {
            bestSurvivalTimeText.text = $"Best Time: {TimeFormatter.SecondsToTimeString(bestSurvivalTime)}";
        }
        
        if (bestWaveText != null)
        {
            bestWaveText.text = $"Best Wave: {bestWave}";
        }
    }
    
    private void AnimateNewRecord(TextMeshProUGUI textElement)
    {
        // Kill any ongoing animations for this element
        DOTween.Kill(textElement.transform);
        DOTween.Kill(textElement);
        
        // Reset scale to prevent accumulating scale changes
        textElement.transform.localScale = Vector3.one;
        
        // Save original color
        Color originalColor = textElement.color;
        
        // Create sequence for record animation
        Sequence recordSequence = DOTween.Sequence();
        
        // Flash color and scale
        recordSequence.Append(textElement.DOColor(newRecordColor, 0.3f));
        recordSequence.Join(textElement.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 3, 0.5f));
        recordSequence.Append(textElement.DOColor(originalColor, 0.3f));
        
        // Ensure scale is reset at the end
        recordSequence.OnComplete(() => {
            textElement.transform.localScale = Vector3.one;
        });
    }
    
    private void AnimateWaveChange(TextMeshProUGUI textElement)
    {
        // Kill any ongoing animations for this element
        DOTween.Kill(textElement.transform);
        DOTween.Kill(textElement);
        
        // Reset scale to prevent accumulating scale changes
        textElement.transform.localScale = Vector3.one;
        
        // Save original color
        Color originalColor = textElement.color;
        
        // Create sequence for wave change animation
        Sequence waveSequence = DOTween.Sequence();
        
        // Flash color and scale
        waveSequence.Append(textElement.DOColor(waveChangeColor, 0.2f));
        waveSequence.Join(textElement.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
        waveSequence.Append(textElement.DOColor(originalColor, 0.3f));
        waveSequence.Join(textElement.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        
        // Ensure scale is reset at the end
        waveSequence.OnComplete(() => {
            textElement.transform.localScale = Vector3.one;
        });
    }
    
    public int GetCurrentCombo()
    {
        return currentCombo;
    }
    
    public int GetBestCombo()
    {
        return bestCombo;
    }
    
    public float GetBestSurvivalTime()
    {
        return bestSurvivalTime;
    }
    
    public int GetBestWave()
    {
        return bestWave;
    }
    
    // Call this when player wants to reset their best stats
    public void ResetBestStats()
    {
        bestCombo = 0;
        bestSurvivalTime = 0f;
        bestWave = 0;
        SaveBestStats();
        UpdateBestStatsDisplay();
    }
}