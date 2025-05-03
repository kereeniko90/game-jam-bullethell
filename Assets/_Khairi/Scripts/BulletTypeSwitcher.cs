using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// this is a function to switch bullet by button,
/// can be used later on as an upgrade
/// </summary>
public class BulletTypeSwitcher : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TextMeshProUGUI currentBulletTypeText;
    
    [Header("Settings")]
    [SerializeField] private KeyCode nextTypeKey = KeyCode.E;
    [SerializeField] private KeyCode previousTypeKey = KeyCode.Q;
    
    private BulletManager.BulletType currentBulletType = BulletManager.BulletType.Normal;
    private int totalBulletTypes;
    
    private void Start()
    {
        // Get total number of bullet types from enum
        totalBulletTypes = System.Enum.GetValues(typeof(BulletManager.BulletType)).Length;
        
        // Set up button listeners if assigned
        if (nextButton != null)
            nextButton.onClick.AddListener(NextBulletType);
        
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousBulletType);
        
        // Update initial UI
        UpdateBulletPrefab();
        UpdateUI();
    }
    
    private void Update()
    {
        // Check for keyboard input
        if (Input.GetKeyDown(nextTypeKey))
        {
            NextBulletType();
        }
        else if (Input.GetKeyDown(previousTypeKey))
        {
            PreviousBulletType();
        }
    }
    
    public void NextBulletType()
    {
        // Move to next bullet type
        int currentIndex = (int)currentBulletType;
        currentIndex = (currentIndex + 1) % totalBulletTypes;
        currentBulletType = (BulletManager.BulletType)currentIndex;
        
        UpdateBulletPrefab();
        UpdateUI();
    }
    
    public void PreviousBulletType()
    {
        // Move to previous bullet type
        int currentIndex = (int)currentBulletType;
        currentIndex = (currentIndex - 1 + totalBulletTypes) % totalBulletTypes;
        currentBulletType = (BulletManager.BulletType)currentIndex;
        
        UpdateBulletPrefab();
        UpdateUI();
    }
    
    private void UpdateBulletPrefab()
    {
        if (bulletManager == null || playerController == null)
        {
            Debug.LogError("BulletTypeSwitcher: Missing references to BulletManager or PlayerController");
            return;
        }
        
        // Get the bullet prefab for the current type
        GameObject bulletPrefab = bulletManager.GetBulletPrefab(currentBulletType);
        
        // Update the player's bullet prefab
        if (bulletPrefab != null)
        {
            playerController.SetBulletPrefab(bulletPrefab);
            playerController.SwitchBulletType(currentBulletType);
        }
        else
        {
            Debug.LogWarning("BulletTypeSwitcher: Failed to get bullet prefab for type " + currentBulletType);
        }
    }
    
    private void UpdateUI()
    {
        if (currentBulletTypeText != null)
        {
            currentBulletTypeText.text = "Bullet Type: " + currentBulletType.ToString();
        }
    }
}