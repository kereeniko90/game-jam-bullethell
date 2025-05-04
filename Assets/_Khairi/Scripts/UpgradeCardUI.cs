using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform positiveModifiersContainer;
    [SerializeField] private Transform negativeModifiersContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI buffText;
    [SerializeField] private TextMeshProUGUI debuffText;

    [Header("Prefabs")]
    [SerializeField] private GameObject positiveModifierItemPrefab;
    [SerializeField] private GameObject negativeModifierItemPrefab;

    [Header("Effects")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 5f;

    // Original scale for hover effect
    private Vector3 originalScale;

    // Animation state
    private bool isHovering = false;

    private void Awake()
    {
        originalScale = transform.localScale;

        // Set up button click event
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnButtonClick);
        }
    }

    // Implement hover interface methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    private void Update()
    {
        // Handle hover animation using unscaledDeltaTime to work even when game is paused
        if (isHovering)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * hoverScaleMultiplier, Time.unscaledDeltaTime * hoverTransitionSpeed);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.unscaledDeltaTime * hoverTransitionSpeed);
        }
    }

    public void SetTitle(string title, Color color)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = color;
        }
    }

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }

    public void SetDescription(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
    }

    public void SetPositiveModifiers(List<string> modifiers, Color textColor)
    {
        if (positiveModifiersContainer != null)
        {
            SetupPositiveModifiers(positiveModifiersContainer, modifiers, textColor);
            //RefreshContentSizeFitters();
        }
    }

    public void SetNegativeModifiers(List<string> modifiers, Color textColor)
    {
        if (negativeModifiersContainer != null)
        {
            SetupNegativeModifiers(negativeModifiersContainer, modifiers, textColor);
            //RefreshContentSizeFitters();
        }
    }

    private void SetupPositiveModifiers(Transform container, List<string> modifiers, Color textColor)
    {
        // Clear existing modifiers
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (modifiers.Count == 0) {
            buffText.gameObject.SetActive(false);
        }

        // Create new modifier items
        foreach (string modifier in modifiers)
        {
            GameObject modifierObj = Instantiate(positiveModifierItemPrefab, container);
            TextMeshProUGUI[] modifierText = modifierObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (modifierText[1] != null)
            {
                modifierText[1].text = modifier;
                modifierText[1].color = textColor;
            }
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(container as RectTransform);
        }


    }

    private void SetupNegativeModifiers(Transform container, List<string> modifiers, Color textColor)
    {
        // Clear existing modifiers
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (modifiers.Count == 0) {
            debuffText.gameObject.SetActive(false);
        }

        // Create new modifier items
        foreach (string modifier in modifiers)
        {
            GameObject modifierObj = Instantiate(negativeModifierItemPrefab, container);
            TextMeshProUGUI[] modifierText = modifierObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (modifierText[1] != null)
            {
                modifierText[1].text = modifier;
                modifierText[1].color = textColor;
            }
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(container as RectTransform);
        }



    }

    public void SetButtonListener(UnityAction action)
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(action);
        }
    }

    private void OnButtonClick()
    {
        // Animation or effects could be added here
        // The actual selection logic is handled by the UpgradeUIController
    }

    public void RefreshLayouts()
    {
        Canvas.ForceUpdateCanvases();

        // First rebuild any nested layouts
        if (positiveModifiersContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(positiveModifiersContainer as RectTransform);
        }

        if (negativeModifiersContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(negativeModifiersContainer as RectTransform);
        }

        // Then rebuild any parent containers that might depend on these sizes
        Transform parent = transform.parent;
        while (parent != null)
        {
            ContentSizeFitter fitter = parent.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
            }
            parent = parent.parent;
        }
    }

    public void RefreshContentSizeFitters()
    {
        StartCoroutine(RefreshContentSizeFittersCoroutine(transform));
    }

    private IEnumerator RefreshContentSizeFittersCoroutine(Transform container)
    {
        // Get all content size fitters in the card and its children
        ContentSizeFitter[] fitters = container.GetComponentsInChildren<ContentSizeFitter>(true);

        // Disable all fitters
        foreach (ContentSizeFitter fitter in fitters)
        {
            fitter.enabled = false;
        }

        // Wait a frame
        yield return null;

        // Re-enable all fitters
        foreach (ContentSizeFitter fitter in fitters)
        {
            fitter.enabled = true;
        }

        // Wait another frame
        yield return null;

        // Force layout rebuild
        Canvas.ForceUpdateCanvases();

        // Rebuild layouts from bottom to top
        foreach (RectTransform rect in container.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.GetComponent<LayoutGroup>() != null || rect.GetComponent<ContentSizeFitter>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        // Final rebuild of the container itself
        LayoutRebuilder.ForceRebuildLayoutImmediate(container as RectTransform);
    }




}