using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuLib.MonoBehaviors;

public sealed class REPOSlider : REPOElement
{
    public enum BarBehavior
    {
        UpdateWithValue,
        StaticAtMinimum,
        StaticAtMaximum
    }

    public TextMeshProUGUI labelTMP;
    public TextMeshProUGUI descriptionTMP;

    public REPOTextScroller repoTextScroller;
    
    public Action<float> onValueChanged;

    public BarBehavior barBehavior;

    public float value;

    public float min
    {
        get => stringOptions?.Length > 0 ? 0 : _min;
        set => _min = value;
    }
    public float max
    {
        get => stringOptions?.Length > 0 ? stringOptions.Length - 1 : _max;
        set => _max = value;
    }

    public string prefix, postfix;
    
    public string[] stringOptions
    {
        get => _stringOptions;
        set
        {
            _stringOptions = value;
            UpdateBarText();
        }
    }

    public int precision
    {
        get => stringOptions?.Length > 0 ? 0 : _precision;
        set
        {
            precisionDecimal = value == 0 ? 1 : Mathf.Pow(10, -value);
            _precision = value;
        }
    }

    public float precisionDecimal
    {
        get => stringOptions?.Length > 0 ? 1 : _precisionDecimal;
        set => _precisionDecimal = value;
    }
    
    private RectTransform barRectTransform, barSizeRectTransform, barPointerRectTransform, barMaskRectTransform;
    private RectTransform sliderBackgroundRectTransform, backgroundFillRectTransform, backgroundOutlineRectTransform;
    
    private TextMeshProUGUI valueTMP, maskedValueTMP;
    
    private MenuPage menuPage;
    private MenuSelectableElement menuSelectableElement;

    private float normalizedValue => (value - min) / (max - min);
    private float _min, _max = 1;
    private float previousValue, _precisionDecimal = .01f;
    private int _precision = 2;
    private string[] _stringOptions = [];
    private string currentDescription;
    private bool isHovering;

    private bool hasValueChanged => Math.Abs(value - previousValue) > float.Epsilon;
    
    public void SetValue(float newValue, bool invokeCallback)
    {
        newValue = Mathf.Clamp(newValue, min, max);
        
        if (invokeCallback && Math.Abs(value - newValue) > float.Epsilon)
            onValueChanged.Invoke(newValue);

        previousValue = value = newValue;
        
        UpdateBarVisual();
        UpdateBarText();
    }

    public void Decrement()
    {
        var newValue = value - precisionDecimal;

        if (Math.Abs(value - min) < float.Epsilon)
            newValue = max;
        else if (newValue < min)
            newValue = min;
        
        SetValue(newValue, true);
    }

    public void Increment()
    {
        var newValue = value + precisionDecimal;

        if (Math.Abs(max - value) < float.Epsilon)
            newValue = min;
        else if (newValue > max)
            newValue = max;
        
        SetValue(newValue, true);
    }
    
    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        rectTransform = (RectTransform) transform;
        menuPage = GetComponentInParent<MenuPage>();
        menuSelectableElement = GetComponent<MenuSelectableElement>();
        labelTMP = GetComponentInChildren<TextMeshProUGUI>();
        descriptionTMP = transform.Find("Big Setting Text").GetComponent<TextMeshProUGUI>();
        valueTMP = transform.Find("Bar Text").GetComponent<TextMeshProUGUI>();
        valueTMP.fontStyle = FontStyles.Normal;
        
        barMaskRectTransform = (RectTransform) transform.Find("MaskedText"); 
        maskedValueTMP = barMaskRectTransform.GetComponentInChildren<TextMeshProUGUI>();
        maskedValueTMP.fontStyle = FontStyles.Normal;
        barPointerRectTransform = (RectTransform) transform.Find("Bar Pointer").transform;
        
        var movementShift = new Vector3(5.3f, 0);
        
        labelTMP.rectTransform.localPosition -= movementShift;

        descriptionTMP.alignment = TextAlignmentOptions.Center;
        descriptionTMP.enableWordWrapping = descriptionTMP.enableAutoSizing = false;
        descriptionTMP.overflowMode = TextOverflowModes.Masking;
        descriptionTMP.fontSize -= 5;

        repoTextScroller = descriptionTMP.gameObject.AddComponent<REPOTextScroller>();
        repoTextScroller.textMeshPro = descriptionTMP;

        descriptionTMP.rectTransform.sizeDelta -= new Vector2(0, 4);
        descriptionTMP.rectTransform.localPosition -= movementShift;

        sliderBackgroundRectTransform = (RectTransform) transform.Find("SliderBG");
        backgroundFillRectTransform = (RectTransform) sliderBackgroundRectTransform.Find("RawImage (2)");
        backgroundOutlineRectTransform = (RectTransform) sliderBackgroundRectTransform.Find("RawImage (3)");
        
        sliderBackgroundRectTransform.localPosition -= movementShift;

        valueTMP.rectTransform.localPosition -= movementShift;
        maskedValueTMP.rectTransform.parent.localPosition -= movementShift;
        
        var bar = transform.Find("Bar");
        bar.localPosition -= movementShift;
        
        barRectTransform = (RectTransform) bar.Find("RawImage");
        barRectTransform.pivot = Vector2.zero;
        barRectTransform.localPosition = new Vector2(0f, -5f);

        barSizeRectTransform = (RectTransform) transform.Find("BarSize");
        barSizeRectTransform.localPosition -= movementShift;
        
        var labelSizeDelta = labelTMP.rectTransform.sizeDelta;
        labelSizeDelta.y -= 10;
        labelTMP.rectTransform.sizeDelta = labelSizeDelta;
        
        var buttons = GetComponentsInChildren<Button>();

        var decrementButton = buttons[0];
        decrementButton.transform.localPosition -= movementShift;
        decrementButton.onClick = new Button.ButtonClickedEvent();
        decrementButton.onClick.AddListener(Decrement);
        
        var incrementButton = buttons[1];
        incrementButton.transform.localPosition -= movementShift;
        incrementButton.onClick = new Button.ButtonClickedEvent();
        incrementButton.onClick.AddListener(Increment);
        
        Destroy(sliderBackgroundRectTransform.Find("RawImage (4)").gameObject);
        Destroy(sliderBackgroundRectTransform.Find("RawImage (5)").gameObject);
        Destroy(bar.Find("Extra Bar").gameObject);
        Destroy(GetComponent<MenuSliderMicrophone>());
        Destroy(GetComponent<MenuSlider>());
    }

    private void Update()
    {
        HandleDescription();
        
        var isHoveringUI = SemiFunc.UIMouseHover(menuPage, barSizeRectTransform, REPOReflection.menuSelectableElement_MenuID.GetValue(menuSelectableElement) as string, 5f, 5f);

        if (isHoveringUI)
        {
            if (!isHovering)
                MenuManager.instance.MenuEffectHover(SemiFunc.MenuGetPitchFromYPos(rectTransform));
            
            isHovering = true;
            
            SemiFunc.MenuSelectionBoxTargetSet(menuPage, barSizeRectTransform, new Vector2(-3f, 0f), new Vector2(20f, 10f));
            
            if (!barPointerRectTransform.gameObject.activeSelf)
                barPointerRectTransform.gameObject.SetActive(true);
            
            HandleHovering();
        }
        else
        {
            isHovering = false;

            if (barPointerRectTransform.gameObject.activeSelf)
            {
                barPointerRectTransform.localPosition = barPointerRectTransform.localPosition with { x = -1000f };
                barPointerRectTransform.gameObject.SetActive(false);
            }
        }

        if (!hasValueChanged)
            return;
        
        value = Mathf.Clamp(value, min, max);
        
        UpdateBarVisual();
        UpdateBarText();
        
        onValueChanged.Invoke(previousValue = value);
    }
    
    private void HandleDescription()
    {
        if (descriptionTMP.text == currentDescription)
            return;

        var hasDescription = !string.IsNullOrEmpty(descriptionTMP.text);
        
        backgroundFillRectTransform.sizeDelta = new Vector2(109.8f, hasDescription ? 33 : 15f);
        backgroundOutlineRectTransform.sizeDelta = new Vector2(108, hasDescription ? 30.6f : 15f);

        if (repoScrollViewElement)
        {
            repoScrollViewElement.bottomPadding = hasDescription ? 25f : 1f;
            repoScrollViewElement.onSettingChanged?.Invoke();
        }
        
        currentDescription = descriptionTMP.text;
    }
    
    private void HandleHovering()
    {
        var pointInRect = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(barSizeRectTransform).x;
        
        var multiplier = max - min;
        var steps = precisionDecimal / multiplier;
        var normalized = Mathf.Round(Mathf.Clamp01(pointInRect / barSizeRectTransform.sizeDelta.x) / steps) * steps;
        
        var newXPos = Mathf.Clamp(barSizeRectTransform.localPosition.x + normalized * barSizeRectTransform.sizeDelta.x, barSizeRectTransform.localPosition.x,
            barSizeRectTransform.localPosition.x + barSizeRectTransform.sizeDelta.x) - 2f;

        barPointerRectTransform.localPosition = barPointerRectTransform.localPosition with { x = newXPos };

        if (!Input.GetMouseButton(0))
            return;
        
        value = min + normalized * multiplier;
        
        if (hasValueChanged)
            MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, menuPage);
    }
    
    private void UpdateBarVisual()
    {
        var newNormalizedBarValue = barBehavior switch
        {
            BarBehavior.UpdateWithValue => normalizedValue,
            BarBehavior.StaticAtMinimum => 0,
            BarBehavior.StaticAtMaximum => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        barRectTransform.sizeDelta = barMaskRectTransform.sizeDelta = new Vector2(newNormalizedBarValue * 100, 10);
    }

    private void UpdateBarText()
    {
        var valueToDisplay = value;

        prefix ??= string.Empty;
        postfix ??= string.Empty;

        var barText = prefix;
        
        if (stringOptions?.Length > 0)
            barText += stringOptions.ElementAtOrDefault(Convert.ToInt32(valueToDisplay)) ?? stringOptions.First();
        else 
            barText += valueToDisplay.ToString($"F{precision}", CultureInfo.CurrentCulture);

        maskedValueTMP.text = valueTMP.text = barText + postfix;
    }
}