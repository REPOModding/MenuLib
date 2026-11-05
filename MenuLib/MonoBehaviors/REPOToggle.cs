using System;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace MenuLib.MonoBehaviors;

public sealed class REPOToggle : REPOElement
{
    public TextMeshProUGUI labelTMP;
    public TextMeshProUGUI leftButtonTMP;
    public TextMeshProUGUI rightButtonTMP;

    public Action<bool> onToggle;
    
    public bool state { get; private set; }

    private RectTransform optionBox, optionBoxBehind, leftOptionRectTransform, rightOptionRectTransform;
    private Vector3 targetPosition, targetScale;
    
    public void SetState(bool newState, bool invokeCallback)
    {
        targetPosition = newState ? leftOptionRectTransform.localPosition : rightOptionRectTransform.localPosition;
        targetScale = new Vector3(73f, 21f, 1f);
        
        if (invokeCallback && state != newState)
            onToggle?.Invoke(newState);
        
        state = newState;
    }

    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        rectTransform = transform as RectTransform;
        labelTMP = GetComponentInChildren<TextMeshProUGUI>();
        optionBox = (RectTransform) transform.Find("Option Box");
        optionBoxBehind = (RectTransform)transform.Find("Option Box Behind");
        leftOptionRectTransform = (RectTransform)transform.Find("RawImage (1) Left");
        rightOptionRectTransform = (RectTransform)transform.Find("RawImage (1) Right");

        rectTransform.sizeDelta -= new Vector2(0, 2);
        labelTMP.rectTransform.sizeDelta -= new Vector2(0, 2);        
        
        var horizontalShift = Vector3.left * 5.3f;
        var verticalShift = Vector3.up;
        
        transform.Find("SliderBG").localPosition += horizontalShift + verticalShift;

        labelTMP.rectTransform.localPosition += horizontalShift + verticalShift;

        transform.Find("RawImage").localPosition += horizontalShift + verticalShift;
        leftOptionRectTransform.localPosition += horizontalShift + verticalShift;
        rightOptionRectTransform.localPosition += horizontalShift + verticalShift;
       
        var buttons = GetComponentsInChildren<Button>();
        
        var leftButton = buttons[0];
        leftButton.transform.localPosition += horizontalShift;
        leftButton.onClick = new Button.ButtonClickedEvent();
        leftButton.onClick.AddListener(() => SetState(true, true));
        
        leftButtonTMP = leftButton.GetComponentInChildren<TextMeshProUGUI>();
        
        var rightButton = buttons[1];
        rightButton.transform.localPosition += horizontalShift;
        rightButton.onClick = new Button.ButtonClickedEvent();
        rightButton.onClick.AddListener(() => SetState(false, true));
        rightButtonTMP = rightButton.GetComponentInChildren<TextMeshProUGUI>();

        Destroy(GetComponent<MenuTwoOptions>());
    }
    
    private void OnTransformParentChanged()
    {
        foreach (var menuButton in GetComponentsInChildren<MenuButton>())
        {
            REPOReflection.menuButton_ParentPage.SetValue(menuButton, GetComponentInParent<MenuPage>());
            REPOReflection.menuButton_MenuScrollBox.SetValue(menuButton, GetComponentInParent<MenuScrollBox>());
        }
    }
    
    private void Update()
    {
        if (!optionBox || !optionBoxBehind)
            return;
        
        optionBox.localPosition = Vector3.Lerp(optionBox.localPosition, targetPosition, 20f * Time.deltaTime);
        optionBox.localScale = Vector3.Lerp(optionBox.localScale, targetScale / 10f, 20f * Time.deltaTime);
        optionBoxBehind.localPosition = Vector3.Lerp(optionBoxBehind.localPosition, targetPosition, 20f * Time.deltaTime);
        optionBoxBehind.localScale = Vector3.Lerp(optionBoxBehind.localScale, new Vector3(targetScale.x + 2f, targetScale.y + 2f, 1f) / 10f, 20f * Time.deltaTime);
    }
}