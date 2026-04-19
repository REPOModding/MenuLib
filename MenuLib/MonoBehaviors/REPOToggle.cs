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
    
    private RectTransform optionBox, optionBoxBehind;
    private Vector3 targetPosition, targetScale;
    
    public void SetState(bool newState, bool invokeCallback)
    {
        targetPosition = newState ? new Vector3(137.8f, 12.3f) : new Vector3(212.644f, 12.3f);
        targetScale = newState ? new Vector3(73f, 22f, 1f) : new Vector3(74f, 22f, 1f);
        
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
        optionBoxBehind = (RectTransform) transform.Find("Option Box Behind");

        labelTMP.rectTransform.sizeDelta -= new Vector2(0, 10);        
        
        var horizontalShift = Vector3.right * 100f;
        
        transform.Find("SliderBG").localPosition += horizontalShift;

        labelTMP.rectTransform.localPosition += horizontalShift;

        transform.Find("RawImage").localPosition += horizontalShift;
        transform.Find("RawImage (1)").localPosition += horizontalShift;
        transform.Find("RawImage (2)").localPosition += horizontalShift;
       
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
            REPOReflection.menuButton_ParentPage.SetValue(menuButton, GetComponentInParent<MenuPage>());
    }
    
    private void Update()
    {
        if (!optionBox || !optionBoxBehind)
            return;
        
        optionBox.localPosition = Vector3.Lerp(optionBox.localPosition, targetPosition, 20f * Time.deltaTime);
        optionBox.localScale = Vector3.Lerp(optionBox.localScale, targetScale / 10f, 20f * Time.deltaTime);
        optionBoxBehind.localPosition = Vector3.Lerp(optionBoxBehind.localPosition, targetPosition, 20f * Time.deltaTime);
        optionBoxBehind.localScale = Vector3.Lerp(optionBoxBehind.localScale, new Vector3(targetScale.x + 4f, targetScale.y + 2f, 1f) / 10f, 20f * Time.deltaTime);
    }
}