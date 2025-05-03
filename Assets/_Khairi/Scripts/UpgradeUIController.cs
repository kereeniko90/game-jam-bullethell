// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections.Generic;

// public class UpgradeUIController : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private GameObject upgradePanel;
//     [SerializeField] private List<UpgradeChoiceUI> upgradeChoices = new List<UpgradeChoiceUI>();
    
//     [Header("Glitch Effect Settings")]
//     [SerializeField] private string glitchCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?/";
//     [SerializeField] private Color glitchColor = new Color(0.2f, 0.9f, 1f);
    
//     private List<GlitchedUpgradeData> currentUpgrades;
    
//     [System.Serializable]
//     public class UpgradeChoiceUI
//     {
//         public Button selectButton;
//         public Image bulletIcon;
//         public TextMeshProUGUI bulletTypeName;
//         public Transform positiveModifiersContainer;
//         public Transform negativeModifiersContainer;
//         public GameObject modifierPrefab;
//     }
    
//     private void Start()
//     {
//         // Hide the upgrade panel initially
//         if (upgradePanel != null)
//         {
//             upgradePanel.SetActive(false);
//         }
        
//         // Set up button listeners
//         for (int i = 0; i < upgradeChoices.Count; i++)
//         {
//             int index = i; // Capture for lambda
//             if (upgradeChoices[i].selectButton != null)
//             {
//                 upgradeChoices[i].selectButton.onClick.AddListener(() => OnUpgradeSelected(index));
//             }
//         }
//     }
    
//     public void ShowUpgradeUI(List<GlitchedUpgradeData> upgrades)
//     {
//         currentUpgrades = upgrades;
        
//         // Show the panel
//         if (upgradePanel != null)
//         {
//             upgradePanel.SetActive(true);
//         }
        
//         // Pause the game
//         Time.timeScale = 0f;
        
//         // Display upgrades
//         DisplayUpgrades();
//     }
    
//     public void HideUpgradeUI()
//     {
//         // Hide the panel
//         if (upgradePanel != null)
//         {
//             upgradePanel.SetActive(false);
//         }
        
//         // Resume the game
//         Time.timeScale = 1f;
        
//         currentUpgrades = null;
//     }
    
//     private void DisplayUpgrades()
//     {
//         for (int i = 0; i < upgradeChoices.Count; i++)
//         {
//             if (i < currentUpgrades.Count)
//             {
//                 UpgradeChoiceUI choiceUI = upgradeChoices[i];
//                 GlitchedUpgradeData upgradeData = currentUpgrades[i];
//                 BulletUpgrade upgrade = upgradeData.originalUpgrade;
                
//                 // Set the icon
//                 if (choiceUI.bulletIcon != null)
//                 {
//                     choiceUI.bulletIcon.sprite = upgrade.icon;
                    
//                     // Apply glitch effect if needed
//                     if (upgradeData.iconGlitched)
//                     {
//                         ApplyIconGlitch(choiceUI.bulletIcon);
//                     }
//                 }
                
//                 // Set the name
//                 if (choiceUI.bulletTypeName != null)
//                 {
//                     string displayName = upgradeData.nameGlitched 
//                         ? ApplyTextGlitch(upgrade.upgradeName) 
//                         : upgrade.upgradeName;
                    
//                     choiceUI.bulletTypeName.text = displayName;
                    
//                     if (upgradeData.nameGlitched)
//                     {
//                         choiceUI.bulletTypeName.color = glitchColor;
//                     }
//                     else
//                     {
//                         choiceUI.bulletTypeName.color = Color.white;
//                     }
//                 }
                
//                 // Clear existing modifiers
//                 ClearContainer(choiceUI.positiveModifiersContainer);
//                 ClearContainer(choiceUI.negativeModifiersContainer);
                
//                 // Add positive modifiers
//                 for (int j = 0; j < upgrade.positiveModifiers.Count; j++)
//                 {
//                     bool isGlitched = j < upgradeData.glitchedPositiveModifiers.Count 
//                         ? upgradeData.glitchedPositiveModifiers[j] 
//                         : false;
                    
//                     AddModifierToUI(
//                         upgrade.positiveModifiers[j], 
//                         choiceUI.positiveModifiersContainer, 
//                         choiceUI.modifierPrefab,
//                         isGlitched,
//                         true
//                     );
//                 }
                
//                 // Add negative modifiers
//                 for (int j = 0; j < upgrade.negativeModifiers.Count; j++)
//                 {
//                     bool isGlitched = j < upgradeData.glitchedNegativeModifiers.Count 
//                         ? upgradeData.glitchedNegativeModifiers[j] 
//                         : false;
                    
//                     AddModifierToUI(
//                         upgrade.negativeModifiers[j], 
//                         choiceUI.negativeModifiersContainer, 
//                         choiceUI.modifierPrefab,
//                         isGlitched,
//                         false
//                     );
//                 }
//             }
//         }
//     }
    
//     private void AddModifierToUI(ModifierData modifier, Transform container, GameObject prefab, bool isGlitched, bool isPositive)
//     {
//         if (container == null || prefab == null)
//             return;
        
//         GameObject modifierObj = Instantiate(prefab, container);
//         ModifierUI modifierUI = modifierObj.GetComponent<ModifierUI>();
        
//         if (modifierUI != null)
//         {
//             modifierUI.SetModifier(modifier, isGlitched, isPositive, glitchColor, glitchCharacters);
//         }
//     }
    
//     private void ClearContainer(Transform container)
//     {
//         if (container == null)
//             return;
        
//         foreach (Transform child in container)
//         {
//             Destroy(child.gameObject);
//         }
//     }
    
//     private void OnUpgradeSelected(int index)
//     {
//         if (currentUpgrades != null && index < currentUpgrades.Count)
//         {
//             BulletUpgrade selectedUpgrade = currentUpgrades[index].originalUpgrade;
            
//             // Apply the upgrade
//             UpgradeManager.Instance.ApplyUpgrade(selectedUpgrade);
//         }
//     }
    
//     private string ApplyTextGlitch(string originalText)
//     {
//         string glitchedText = originalText;
        
//         // Replace random characters with glitch characters
//         int glitchCount = Random.Range(1, Mathf.Max(2, originalText.Length / 2));
        
//         for (int i = 0; i < glitchCount; i++)
//         {
//             int charIndex = Random.Range(0, originalText.Length);
//             int glitchCharIndex = Random.Range(0, glitchCharacters.Length);
            
//             if (charIndex < glitchedText.Length && glitchCharIndex < glitchCharacters.Length)
//             {
//                 char glitchChar = glitchCharacters[glitchCharIndex];
                
//                 if (charIndex == 0)
//                 {
//                     glitchedText = glitchChar + glitchedText.Substring(1);
//                 }
//                 else if (charIndex == glitchedText.Length - 1)
//                 {
//                     glitchedText = glitchedText.Substring(0, charIndex) + glitchChar;
//                 }
//                 else
//                 {
//                     glitchedText = glitchedText.Substring(0, charIndex) + glitchChar + glitchedText.Substring(charIndex + 1);
//                 }
//             }
//         }
        
//         return glitchedText;
//     }
    
//     private void ApplyIconGlitch(Image icon)
//     {
//         if (icon == null)
//             return;
        
//         // Apply visual glitch effect
//         // You can use a color tint for a simple effect
//         icon.color = glitchColor;
        
//         // For more advanced effects, you could use a material with a glitch shader
//         // icon.material = glitchMaterial;
//     }
// }