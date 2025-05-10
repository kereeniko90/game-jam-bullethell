using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TimeUI : MonoBehaviour
{   
    public static TimeUI Instance { get; private set;}
    [SerializeField] private TextMeshProUGUI timeIndicator;
    [SerializeField] private float eventInterval = 120f;
    
    private float currentTimer;
    private float lastEventTime = 0f;    
    public UnityEvent onIntervalReached = new UnityEvent();

    private void Awake() {
        Instance = this;
    }

    void Update()
    {
        // Update the timer display
        currentTimer = GameManager.Instance.survivalTime;
        timeIndicator.text = TimeFormatter.SecondsToTimeString(currentTimer);
        
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