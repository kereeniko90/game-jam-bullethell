using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TimeUI : MonoBehaviour
{   
    public static TimeUI Instance { get; private set;}
    [SerializeField] private TextMeshProUGUI timeIndicator;
    [SerializeField] private float eventInterval = 120f;
    
    private float currentTimer;
    private int lastWave;
    private float lastEventTime = 0f;    
    public UnityEvent onIntervalReached = new UnityEvent();
    public UnityEvent<int> onWaveChanged = new UnityEvent<int>();

    private void Awake() {
        Instance = this;
    }

    void Update()
    {
        // Update the timer display
        currentTimer = GameManager.Instance.survivalTime;
        int currentWave = GameManager.Instance.currentWave;
        timeIndicator.text = TimeFormatter.SecondsToTimeString(currentTimer);

        if (currentWave != lastWave)
        {
            // Wave changed, trigger the event
            onWaveChanged.Invoke(currentWave);
            lastWave = currentWave;
        }
        
        // Check if it's time to invoke the event
        if (currentTimer - lastEventTime >= eventInterval)
        {
            lastEventTime = Mathf.Floor(currentTimer / eventInterval) * eventInterval;
            
            // Invoke the event
            onIntervalReached.Invoke();
            
            Debug.Log($"Event triggered at {TimeFormatter.SecondsToTimeString(currentTimer)}");
        }
    }
}