using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class UpgradeUIController : MonoBehaviour
{
    [SerializeField] Button temporaryCallUpgradeButton;
    [Header("UI References")]
    [SerializeField] private GameObject upgradeUIPanel;
    [SerializeField] private Transform upgradeCardsContainer;
    [SerializeField] private GameObject upgradeCardPrefab;

    [Header("Card References")]
    [SerializeField] private Color positiveModifierColor = Color.green;
    [SerializeField] private Color negativeModifierColor = Color.red;
    [SerializeField] private Color glitchedTextColor = Color.cyan;

    [Header("Audio")]
    [SerializeField] private AudioClip openUpgradeUISound;
    [SerializeField] private AudioClip selectUpgradeSound;

    [Header("Animation Settings")]
    [SerializeField] private float cardAppearDelay = 0.2f; // Delay between each card appearing
    [SerializeField] private float cardAnimationDuration = 0.5f; // Duration of each card's animation
    [SerializeField] private Ease cardAnimationEase = Ease.OutBack; // DOTween easing function
    [SerializeField] private Vector3 cardStartScale = new Vector3(0.5f, 0.5f, 0.5f); // Initial scale before animation
    [SerializeField] private float cardStartAlpha = 0f; // Initial alpha before animation

    // References
    private AudioSource audioSource;
    private UpgradeManager upgradeManager;

    // Currently displayed upgrades
    private List<GlitchedUpgradeData> currentUpgrades = new List<GlitchedUpgradeData>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeUIController: Could not find UpgradeManager in scene");
        }

        // Hide the upgrade UI initially
        if (upgradeUIPanel != null)
        {
            upgradeUIPanel.SetActive(false);
        }

        temporaryCallUpgradeButton.onClick.AddListener(TemporaryCallUI);

        if (SoundManager.Instance == null) {
            gameObject.AddComponent<SoundManager>();
        }

        if (MusicManager.Instance == null) {
            var obj = Resources.Load<GameObject>("MusicManager");
            Instantiate(obj,transform.position,Quaternion.identity, gameObject.transform);
        }
    }

    public void TemporaryCallUI()
    {
        UpgradeManager.Instance.ShowUpgradeOptions();
    }

    public void ShowUpgradeUI(List<GlitchedUpgradeData> upgrades)
    {
        if (upgradeUIPanel == null || upgradeCardsContainer == null || upgradeCardPrefab == null)
        {
            Debug.LogError("UpgradeUIController: Missing UI references");
            return;
        }

        // Store current upgrades
        currentUpgrades = upgrades;

        // Clear existing cards
        foreach (Transform child in upgradeCardsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create upgrade cards
        // for (int i = 0; i < upgrades.Count; i++)
        // {
        //     CreateUpgradeCard(upgrades[i], i);
        // }

        // Show the upgrade UI
        upgradeUIPanel.SetActive(true);

        // Play sound effect
        if (audioSource != null && openUpgradeUISound != null)
        {
            audioSource.PlayOneShot(openUpgradeUISound);
        }

        StartCoroutine(CreateCardsSequentially(upgrades));

        // Pause the game
        Time.timeScale = 0f;
    }

    private IEnumerator CreateCardsSequentially(List<GlitchedUpgradeData> upgrades)
    {
        // Create each card one by one with animation
        for (int i = 0; i < upgrades.Count; i++)
        {
            GameObject cardObj = CreateUpgradeCard(upgrades[i], i);
            
            // Setup initial state for animation
            SetupCardForAnimation(cardObj);
            
            // Animate the card
            AnimateCard(cardObj);
            
            // Wait before showing the next card
            yield return new WaitForSecondsRealtime(cardAppearDelay);
        }
    }

    private void SetupCardForAnimation(GameObject cardObj)
    {
        // Set initial scale
        cardObj.transform.localScale = cardStartScale;
        
        // Set initial alpha (optional)
        CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = cardObj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = cardStartAlpha;
    }

    private void AnimateCard(GameObject cardObj)
    {
        // Get or add CanvasGroup for fade animation
        CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = cardObj.AddComponent<CanvasGroup>();
        }
        
        // Scale animation
        cardObj.transform.DOScale(Vector3.one, cardAnimationDuration)
            .SetEase(cardAnimationEase)
            .SetUpdate(true); // Use unscaled time since game is paused
        
        // Fade in animation
        canvasGroup.DOFade(1f, cardAnimationDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // Use unscaled time since game is paused
    }


    public void HideUpgradeUI()
    {
        if (upgradeUIPanel != null)
        {
            // upgradeUIPanel.SetActive(false);
            AnimateCardsOut(() => {
                upgradeUIPanel.SetActive(false);
                // Resume the game
                Time.timeScale = 1f;
            });
        }

        // Resume the game
        // Time.timeScale = 1f;
    }

    private void AnimateCardsOut(TweenCallback onComplete)
    {
        Sequence sequence = DOTween.Sequence();
        
        // Get all cards
        int childCount = upgradeCardsContainer.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform card = upgradeCardsContainer.GetChild(i);
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            
            // Add fade out and scale down animation
            sequence.Insert(0, card.DOScale(cardStartScale, cardAnimationDuration / 2)
                .SetEase(Ease.InBack)
                .SetUpdate(true));
                
            if (canvasGroup != null)
            {
                sequence.Insert(0, canvasGroup.DOFade(0, cardAnimationDuration / 2)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true));
            }
        }
        
        sequence.OnComplete(onComplete).SetUpdate(true);
    }

    // private void CreateUpgradeCard(GlitchedUpgradeData upgradeData, int index)
    // {
    //     if (upgradeCardPrefab == null || upgradeCardsContainer == null)
    //     {
    //         Debug.LogError("UpgradeUIController: Missing card prefab or container reference");
    //         return;
    //     }

    //     // Instantiate the card
    //     GameObject cardObj = Instantiate(upgradeCardPrefab, upgradeCardsContainer);
    //     UpgradeCardUI cardUI = cardObj.GetComponent<UpgradeCardUI>();

    //     if (cardUI == null)
    //     {
    //         Debug.LogError("UpgradeUIController: Upgrade card prefab is missing UpgradeCardUI component");
    //         return;
    //     }

    //     BulletUpgrade upgrade = upgradeData.originalUpgrade;

    //     // Set card values
    //     string titleText = upgradeData.nameGlitched ? GenerateGlitchedText(upgrade.upgradeName) : upgrade.upgradeName;

    //     cardUI.SetTitle(titleText, upgradeData.nameGlitched ? glitchedTextColor : Color.white);
    //     cardUI.SetIcon(upgradeData.iconGlitched ? GenerateGlitchedIcon(upgrade.icon) : upgrade.icon);
    //     cardUI.SetDescription(upgrade.description);

    //     // Add positive modifiers
    //     List<string> positiveModifiers = new List<string>();
    //     List<string> negativeModifiers = new List<string>();

    //     // Process base modifiers
    //     for (int i = 0; i < upgrade.baseModifiers.Count; i++)
    //     {
    //         BaseBulletModifier modifier = upgrade.baseModifiers[i];
    //         bool isGlitched = upgradeData.glitchedBaseModifiers.Count > i && upgradeData.glitchedBaseModifiers[i];

    //         string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

    //         if (modifier.isPositive)
    //         {
    //             positiveModifiers.Add(modifierText);
    //         }
    //         else
    //         {
    //             negativeModifiers.Add(modifierText);
    //         }
    //     }

    //     // Process specific modifiers
    //     for (int i = 0; i < upgrade.specificModifiers.Count; i++)
    //     {
    //         SpecificBulletModifier modifier = upgrade.specificModifiers[i];
    //         bool isGlitched = upgradeData.glitchedSpecificModifiers.Count > i && upgradeData.glitchedSpecificModifiers[i];

    //         string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

    //         if (modifier.isPositive)
    //         {
    //             positiveModifiers.Add(modifierText);
    //         }
    //         else
    //         {
    //             negativeModifiers.Add(modifierText);
    //         }
    //     }

    //     // Process player modifiers
    //     for (int i = 0; i < upgrade.playerModifiers.Count; i++)
    //     {
    //         PlayerModifier modifier = upgrade.playerModifiers[i];
    //         bool isGlitched = upgradeData.glitchedPlayerModifiers.Count > i && upgradeData.glitchedPlayerModifiers[i];

    //         string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

    //         if (modifier.isPositive)
    //         {
    //             positiveModifiers.Add(modifierText);
    //         }
    //         else
    //         {
    //             negativeModifiers.Add(modifierText);
    //         }
    //     }

    //     // Set modifiers on the card
    //     cardUI.SetPositiveModifiers(positiveModifiers, positiveModifierColor);
    //     cardUI.SetNegativeModifiers(negativeModifiers, negativeModifierColor);
    //     StartCoroutine(FinalLayoutRefresh(cardUI));

    //     // Add button click handler
    //     cardUI.SetButtonListener(() => OnUpgradeSelected(index));
    // }
    private GameObject CreateUpgradeCard(GlitchedUpgradeData upgradeData, int index)
    {
        if (upgradeCardPrefab == null || upgradeCardsContainer == null)
        {
            Debug.LogError("UpgradeUIController: Missing card prefab or container reference");
            return null;
        }

        // Instantiate the card
        GameObject cardObj = Instantiate(upgradeCardPrefab, upgradeCardsContainer);
        UpgradeCardUI cardUI = cardObj.GetComponent<UpgradeCardUI>();

        if (cardUI == null)
        {
            Debug.LogError("UpgradeUIController: Upgrade card prefab is missing UpgradeCardUI component");
            return cardObj;
        }

        BulletUpgrade upgrade = upgradeData.originalUpgrade;

        // Set card values
        string titleText = upgradeData.nameGlitched ? GenerateGlitchedText(upgrade.upgradeName) : upgrade.upgradeName;

        cardUI.SetTitle(titleText, upgradeData.nameGlitched ? glitchedTextColor : Color.white);
        cardUI.SetIcon(upgradeData.iconGlitched ? GenerateGlitchedIcon(upgrade.icon) : upgrade.icon);
        cardUI.SetDescription(upgrade.description);

        // Add positive modifiers
        List<string> positiveModifiers = new List<string>();
        List<string> negativeModifiers = new List<string>();

        // Process base modifiers
        for (int i = 0; i < upgrade.baseModifiers.Count; i++)
        {
            BaseBulletModifier modifier = upgrade.baseModifiers[i];
            bool isGlitched = upgradeData.glitchedBaseModifiers.Count > i && upgradeData.glitchedBaseModifiers[i];

            string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

            if (modifier.isPositive)
            {
                positiveModifiers.Add(modifierText);
            }
            else
            {
                negativeModifiers.Add(modifierText);
            }
        }

        // Process specific modifiers
        for (int i = 0; i < upgrade.specificModifiers.Count; i++)
        {
            SpecificBulletModifier modifier = upgrade.specificModifiers[i];
            bool isGlitched = upgradeData.glitchedSpecificModifiers.Count > i && upgradeData.glitchedSpecificModifiers[i];

            string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

            if (modifier.isPositive)
            {
                positiveModifiers.Add(modifierText);
            }
            else
            {
                negativeModifiers.Add(modifierText);
            }
        }

        // Process player modifiers
        for (int i = 0; i < upgrade.playerModifiers.Count; i++)
        {
            PlayerModifier modifier = upgrade.playerModifiers[i];
            bool isGlitched = upgradeData.glitchedPlayerModifiers.Count > i && upgradeData.glitchedPlayerModifiers[i];

            string modifierText = isGlitched ? GenerateGlitchedText(modifier.description) : modifier.description;

            if (modifier.isPositive)
            {
                positiveModifiers.Add(modifierText);
            }
            else
            {
                negativeModifiers.Add(modifierText);
            }
        }

        // Set modifiers on the card
        cardUI.SetPositiveModifiers(positiveModifiers, positiveModifierColor);
        cardUI.SetNegativeModifiers(negativeModifiers, negativeModifierColor);
        StartCoroutine(FinalLayoutRefresh(cardUI));

        // Add button click handler
        cardUI.SetButtonListener(() => OnUpgradeSelected(index));
        
        return cardObj;
    }

    private IEnumerator FinalLayoutRefresh(UpgradeCardUI cardUI)
    {
        // Wait for end of frame to ensure all content is processed
        yield return new WaitForEndOfFrame();

        // Perform one more refresh
        cardUI.RefreshContentSizeFitters();

        // Wait another frame and do a final Canvas update
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    private void OnUpgradeSelected(int index)
    {
        if (index < 0 || index >= currentUpgrades.Count || upgradeManager == null)
        {
            return;
        }

        // Play sound effect
        if (audioSource != null && selectUpgradeSound != null)
        {
            audioSource.PlayOneShot(selectUpgradeSound);
        }

        // Apply the upgrade
        upgradeManager.ApplyUpgrade(currentUpgrades[index].originalUpgrade);

        // Hide the UI
        HideUpgradeUI();
    }

    // Helper method to generate "glitched" text
    private string GenerateGlitchedText(string originalText)
    {
        if (string.IsNullOrEmpty(originalText))
        {
            return "ĘŔŘØŘ";
        }

        // List of glitch characters
        char[] glitchChars = new char[] { 'Æ', 'Ø', 'Ł', 'Ð', 'Þ', 'Ŧ', 'Ŋ', 'Ħ', 'Ż', 'Đ', 'Ł', 'Ę', 'Ŕ', 'Ř', 'Ø' };

        // Create a copy of the original text
        char[] result = originalText.ToCharArray();

        // Replace 30-50% of characters with glitch characters
        int glitchCount = Random.Range(Mathf.CeilToInt(result.Length * 0.3f), Mathf.CeilToInt(result.Length * 0.5f));

        for (int i = 0; i < glitchCount; i++)
        {
            int randomIndex = Random.Range(0, result.Length);
            result[randomIndex] = glitchChars[Random.Range(0, glitchChars.Length)];
        }

        return new string(result);
    }

    // Helper method to generate a "glitched" icon
    private Sprite GenerateGlitchedIcon(Sprite originalIcon)
    {
        // In a real implementation, you might want to have some pre-prepared glitched icons
        // For now, just return the original, but you could modify this as needed

        // Alternatively, you could have a list of "glitched" icon sprites to choose from
        return originalIcon;
    }
}