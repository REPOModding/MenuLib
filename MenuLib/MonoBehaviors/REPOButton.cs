using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuLib.MonoBehaviors;

public sealed class REPOButton : REPOElement
{
    public MenuButton menuButton;

    public TextMeshProUGUI labelTMP;

    [Obsolete("Update the button clicked event using the 'onClick' field rather than through the button")]
    public Button button;
    
    public Action onClick;

    public Vector2? overrideButtonSize;
    
    private string previousText;
    private float previousFontSize, previousFontSizeMin, previousFontSizeMax;
    
    private Vector2? previousOverrideButtonSize;
    
    public Vector2 GetLabelSize() => labelTMP.GetPreferredValues(); 
    
    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        rectTransform = transform as RectTransform;
        button = GetComponent<Button>();
        menuButton = GetComponent<MenuButton>();
        labelTMP = GetComponentInChildren<TextMeshProUGUI>();
        
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() => onClick?.Invoke());
        
        Destroy(GetComponent<MenuButtonPopUp>());
    }

    private void Update()
    {
        if (labelTMP.text == previousText && overrideButtonSize == previousOverrideButtonSize && Math.Abs(labelTMP.fontSize - previousFontSize) < float.Epsilon && Math.Abs(labelTMP.fontSizeMin - previousFontSizeMin) < float.Epsilon && Math.Abs(labelTMP.fontSizeMax - previousFontSizeMax) < float.Epsilon)
            return;

        rectTransform.sizeDelta = overrideButtonSize ?? GetLabelSize();
        
        previousText = labelTMP.text;
        previousOverrideButtonSize = overrideButtonSize;
        previousFontSize = labelTMP.fontSize;
        previousFontSizeMin = labelTMP.fontSizeMin;
        previousFontSizeMax = labelTMP.fontSizeMax;
    }
    
    private void OnTransformParentChanged() => REPOReflection.menuButton_ParentPage.SetValue(menuButton, GetComponentInParent<MenuPage>());
}