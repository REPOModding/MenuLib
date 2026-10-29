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
    private string menuID;

    private float normalizedValue => (max - min) != 0f ? (value - min) / (max - min) : 0f;
    private float _min, _max = 1;
    private float previousValue, _precisionDecimal = .01f;
    private float defaultHeight;
    private int _precision = 2;
    private string[] _stringOptions = [];
    private string currentDescription;
    private bool isHovering;
    private bool isBigSlider;

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
        isBigSlider = rectTransform.pivot == Vector2.up;
        rectTransform.pivot = Vector2.zero;
        menuPage = GetComponentInParent<MenuPage>();
        menuID = SemiFunc.MenuGetSelectableID(gameObject);
        labelTMP = GetComponentInChildren<TextMeshProUGUI>();
        valueTMP = transform.Find("Bar Text").GetComponent<TextMeshProUGUI>();
        valueTMP.fontStyle = FontStyles.Normal;
        
        descriptionTMP = Instantiate(valueTMP, transform);
        descriptionTMP.name = "Big Setting Text";
        descriptionTMP.rectTransform.anchoredPosition = isBigSlider ? new Vector2(0, -20.5f) : new Vector2(5f, -19f);
        descriptionTMP.rectTransform.sizeDelta = new Vector2(251.4f, 31f);
        descriptionTMP.color = new Color(1f, .55f, 0f);
        descriptionTMP.fontStyle = FontStyles.UpperCase;
        descriptionTMP.margin = new Vector4(5, 0, 5, 0);
        descriptionTMP.fontSize = 21.6f;
        
        barMaskRectTransform = (RectTransform) transform.Find("MaskedText"); 
        maskedValueTMP = barMaskRectTransform.GetComponentInChildren<TextMeshProUGUI>();
        maskedValueTMP.fontStyle = FontStyles.Normal;
        barPointerRectTransform = (RectTransform) transform.Find("Bar Pointer").transform;
        
        var movementShift = new Vector3(5.3f, 0);
        var verticalShift = isBigSlider ? Vector3.zero : Vector3.up * 1.5f;
        
        if (!isBigSlider)
            rectTransform.sizeDelta += new Vector2(0, 3);
        
        defaultHeight = rectTransform.sizeDelta.y;
        
        labelTMP.rectTransform.localPosition -= movementShift + verticalShift;

        descriptionTMP.alignment = TextAlignmentOptions.Center;
        descriptionTMP.enableWordWrapping = descriptionTMP.enableAutoSizing = false;
        descriptionTMP.overflowMode = TextOverflowModes.Masking;
        descriptionTMP.fontSize -= 5;

        repoTextScroller = descriptionTMP.gameObject.AddComponent<REPOTextScroller>();
        repoTextScroller.textMeshPro = descriptionTMP;

        descriptionTMP.rectTransform.sizeDelta -= new Vector2(0, 4);
        descriptionTMP.rectTransform.localPosition -= movementShift + verticalShift;

        sliderBackgroundRectTransform = (RectTransform) transform.Find("SliderBG");
        backgroundFillRectTransform = (RectTransform) sliderBackgroundRectTransform.Find("RawImage (2)");
        backgroundOutlineRectTransform = (RectTransform) sliderBackgroundRectTransform.Find("RawImage (3)");
        
        if (!isBigSlider)
        {
            backgroundFillRectTransform.pivot = backgroundOutlineRectTransform.pivot = new Vector2(.5f, 1f);
            backgroundFillRectTransform.anchoredPosition = backgroundFillRectTransform.anchoredPosition with { y = 6.4f };
            backgroundOutlineRectTransform.anchoredPosition = backgroundOutlineRectTransform.anchoredPosition with { y = 7.5f };
        }
        
        sliderBackgroundRectTransform.localPosition -= movementShift + verticalShift;

        valueTMP.rectTransform.localPosition -= movementShift + verticalShift;
        maskedValueTMP.rectTransform.parent.localPosition -= movementShift + verticalShift;
        
        var bar = transform.Find("Bar");
        bar.localPosition -= movementShift + verticalShift;
        barPointerRectTransform.localPosition -= verticalShift;
        
        if (!isBigSlider)
        {
            barRectTransform = (RectTransform) bar.Find("RawImage");
            barRectTransform.pivot = Vector2.zero;
            barRectTransform.localPosition = new Vector2(0f, -5f);
        }
        else
            barRectTransform = (RectTransform) bar;

        barSizeRectTransform = (RectTransform) transform.Find("BarSize");
        barSizeRectTransform.localPosition -= movementShift;
        
        var buttons = GetComponentsInChildren<Button>();

        var decrementButton = buttons[0];
        decrementButton.transform.localPosition -= movementShift;
        decrementButton.onClick = new Button.ButtonClickedEvent();
        decrementButton.onClick.AddListener(Decrement);
        
        var incrementButton = buttons[1];
        incrementButton.transform.localPosition -= movementShift;
        incrementButton.onClick = new Button.ButtonClickedEvent();
        incrementButton.onClick.AddListener(Increment);
        
        if (isBigSlider)
            Destroy(GetComponent<LocalizeChangedUI>());
        
        Destroy(bar.Find("Extra Bar").gameObject);
        Destroy(GetComponent<MenuSlider>());
    }

    private void Update()
    {
        HandleDescription();
        
        var isHoveringUI = SemiFunc.UIMouseHover(menuPage, barSizeRectTransform, menuID, 5f, 5f);

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
        
        if (!isBigSlider)
        {
            backgroundFillRectTransform.sizeDelta = new Vector2(109.8f, hasDescription ? 33 : 15f);
            backgroundOutlineRectTransform.sizeDelta = new Vector2(108, hasDescription ? 30.6f : 15f);
        }

        var descriptionHeight = defaultHeight + (isBigSlider ? 18.8f : 23.8f);
        var heightDifference = (hasDescription ? descriptionHeight : defaultHeight) - rectTransform.sizeDelta.y;

        foreach (RectTransform child in transform)
            child.anchoredPosition += new Vector2(0, (1 - child.anchorMax.y) * heightDifference);

        rectTransform.sizeDelta += new Vector2(0, heightDifference);

        if (repoScrollViewElement)
            repoScrollViewElement.onSettingChanged?.Invoke();
        
        currentDescription = descriptionTMP.text;
    }
    
    private void HandleHovering()
    {
        var pointInRect = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(barSizeRectTransform).x;
        
        var multiplier = max - min;
        var steps = precisionDecimal / multiplier;
        var normalized = multiplier <= float.Epsilon 
            ? 0f 
            : Mathf.Round(Mathf.Clamp01(pointInRect / barSizeRectTransform.sizeDelta.x) / steps) * steps;
        
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
        
        if (isBigSlider)
        {
            barRectTransform.localScale = barRectTransform.localScale with { x = newNormalizedBarValue };
            barMaskRectTransform.sizeDelta = barMaskRectTransform.sizeDelta with { x = barSizeRectTransform.sizeDelta.x * newNormalizedBarValue };
        }
        else
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