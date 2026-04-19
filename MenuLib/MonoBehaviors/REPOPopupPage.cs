using System;
using MenuLib.Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuLib.MonoBehaviors;

public sealed class REPOPopupPage : MonoBehaviour
{
    public enum PresetSide
    {
        Left,
        Right
    }

    public delegate RectTransform ScrollViewBuilderDelegate(Transform scrollView);

    public delegate bool ShouldCloseMenuDelegate();

    public RectTransform rectTransform;
    public RectTransform maskRectTransform;
    public RectTransform scrollBarRectTransform;
    public MenuPage menuPage;
    public TextMeshProUGUI headerTMP;
    public MenuScrollBox menuScrollBox;
    public REPOScrollView scrollView;

    public bool pageDimmerVisibility
    {
        get => pageDimmerGameObject.gameObject.activeSelf;
        set => pageDimmerGameObject.gameObject.SetActive(value);
    }

    public bool isCachedPage { get; internal set; }
    
    public float pageDimmerOpacity
    {
        get => pageDimmerRawImage.color.a;
        set => pageDimmerRawImage.color = pageDimmerRawImage.color with { a = value };
    }
    
    public Padding maskPadding
    {
        get => _maskPadding;
        set
        {
            var sizeDelta = defaultMaskSizeDelta;
            var position = defaultMaskPosition;

            sizeDelta.x -= value.left + value.right;
            sizeDelta.y -= value.top + value.bottom;
            
            if (value.left != 0)
                position.x += value.left;
            
            if (value.bottom != 0)
                position.y += value.bottom;
            
            maskRectTransform.sizeDelta = sizeDelta;
            maskRectTransform.localPosition = position;
            
            _maskPadding = value;
            UpdateScrollBarPosition();
        }
    }

    [Obsolete("Use onEscapePressed instead.")]
    public bool closeMenuOnEscape = true;

    public ShouldCloseMenuDelegate onEscapePressed;

    internal bool pageWasActivatedOnce;
    
    private GameObject pageDimmerGameObject;
    private RawImage pageDimmerRawImage;

    private RectTransform scrollBarFillRectTransform, scrollBarOutlineRectTransform;

    private Vector2 defaultMaskSizeDelta, defaultMaskPosition;
    private Padding _maskPadding;
    
    public void OpenPage(bool openOnTop)
    {
        MenuAPI.OpenMenuPage(menuPage, openOnTop);
        pageWasActivatedOnce = true;
        
        scrollView.UpdateElements();
    }

    public void ClosePage(bool closePagesAddedOnTop) => MenuAPI.CloseMenuPage(menuPage, closePagesAddedOnTop);
    
    public void AddElement(MenuAPI.BuilderDelegate builderDelegate) => builderDelegate?.Invoke(transform);
    
    public void AddElement(RectTransform elementRectTransform, Vector2 localPosition = default)
    {
        elementRectTransform.SetParent(transform);
        elementRectTransform.localPosition = localPosition;
    }
    
    public void AddElementToScrollView(ScrollViewBuilderDelegate scrollViewBuilderDelegate, float topPadding = 0, float bottomPadding = 0)
    {
        if (scrollViewBuilderDelegate?.Invoke(menuScrollBox.scroller)?.gameObject.AddComponent<REPOScrollViewElement>() is not { } scrollViewElement) return;

        scrollViewElement.onSettingChanged = scrollView.UpdateElements;
        scrollViewElement.topPadding = topPadding;
        scrollViewElement.bottomPadding = bottomPadding;
    }
    
    public void AddElementToScrollView(RectTransform elementRectTransform, Vector2 localPosition = default, float topPadding = 0, float bottomPadding = 0)
    {
        if (elementRectTransform.gameObject.AddComponent<REPOScrollViewElement>() is not { } scrollViewElement)
            return;
        
        scrollViewElement.onSettingChanged = scrollView.UpdateElements;
        
        elementRectTransform.SetParent(menuScrollBox.scroller);
        elementRectTransform.localPosition = localPosition;
        
        scrollViewElement.topPadding = topPadding;
        scrollViewElement.bottomPadding = bottomPadding;
    }
    
    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        menuPage = GetComponent<MenuPage>();
        headerTMP = GetComponentInChildren<TextMeshProUGUI>();
        menuScrollBox = GetComponentInChildren<MenuScrollBox>();
        
        rectTransform = (RectTransform) new GameObject("Page Content", typeof(RectTransform)).transform;
        rectTransform.SetParent(transform);
        
        transform.Find("Panel").SetParent(rectTransform);
        headerTMP.transform.parent.SetParent(rectTransform);
        menuScrollBox.transform.SetParent(rectTransform);
        
        pageDimmerGameObject = Instantiate(REPOTemplates.pageDimmerTemplate, transform).gameObject;
        pageDimmerGameObject.transform.SetAsFirstSibling();
        
        pageDimmerRawImage = pageDimmerGameObject.GetComponentInChildren<RawImage>();

        menuPage.menuPageIndex = (MenuPageIndex) (-1);

        var scroller = menuScrollBox.scroller;
        
        for (var i = 2; i < scroller.childCount; i++)
            Destroy(scroller.GetChild(i).gameObject);

        scrollView = scroller.gameObject.AddComponent<REPOScrollView>();
        scrollView.popupPage = this;
        
        maskRectTransform = (RectTransform) scroller.parent;

        defaultMaskSizeDelta = maskRectTransform.sizeDelta;
        defaultMaskPosition = maskRectTransform.localPosition;

        menuScrollBox.scroller.sizeDelta = maskRectTransform.sizeDelta;
        
        scrollBarRectTransform = (RectTransform) menuScrollBox.scrollBar.transform;
        scrollBarFillRectTransform = (RectTransform) scrollBarRectTransform.Find("Scroll Bar Bg (2)");
        scrollBarOutlineRectTransform = (RectTransform) scrollBarRectTransform.Find("Scroll Bar Bg (1)");

        maskPadding = new Padding(0, 0, 0, 25);
        
        Destroy(GetComponent<MenuPageSettingsPage>());
        
        MenuAPI.customMenuPages.Add(menuPage, this);
    }
    
    private void Start()
    {
        REPOReflection.menuScrollBox_ScrollerEndPosition.SetValue(menuScrollBox, 0);
        menuScrollBox.scroller.localPosition = menuScrollBox.scroller.localPosition with { y = 0 };
        
        REPOReflection.menuPage_ScrollBoxes.SetValue(menuPage, 2);
        
        if (!pageWasActivatedOnce)
            menuPage.PageStateSet(MenuPage.PageState.Closing);
        
        scrollView.SetScrollPosition(0);
    }

    private void Update()
    {
        if (!closeMenuOnEscape)
            return;
        
        var pageState = (MenuPage.PageState) REPOReflection.menuPage_CurrentPageState.GetValue(menuPage);
        
        if (!SemiFunc.InputDown(InputKey.Back) || pageState == MenuPage.PageState.Closing)
            return;
        
        if (onEscapePressed == null || onEscapePressed.Invoke())
            ClosePage(false);
    }

    private void OnDestroy() => MenuAPI.customMenuPages.Remove(menuPage);

    private void UpdateScrollBarPosition()
    {
        if (!scrollBarRectTransform)
            return;
        
        scrollBarRectTransform.localPosition = scrollBarRectTransform.localPosition with { y = maskRectTransform.localPosition.y};

        var scrollBarSize = scrollBarRectTransform.sizeDelta;
        scrollBarSize.y = maskRectTransform.sizeDelta.y;
        menuScrollBox.scrollBarBackground.sizeDelta = scrollBarFillRectTransform.sizeDelta = scrollBarRectTransform.sizeDelta = scrollBarSize;

        scrollBarOutlineRectTransform.sizeDelta = scrollBarSize + new Vector2(4f, 4f);
    }
}