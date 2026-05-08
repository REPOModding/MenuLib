using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MenuLib.MonoBehaviors;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MenuLib;

public static class MenuAPI
{
    internal static BuilderDelegate mainMenuBuilderDelegate,
        settingsMenuBuilderDelegate,
        colorMenuBuilderDelegate,
        lobbyMenuBuilderDelegate,
        escapeMenuBuilderDelegate,
        regionSelectionMenuBuilderDelegate,
        serverListMenuBuilderDelegate;

    internal static readonly Dictionary<MenuPage, REPOPopupPage> customMenuPages = [];

    private static MenuButtonPopUp menuButtonPopup;

    public delegate void BuilderDelegate(Transform parent);

    public static void AddElementToMainMenu(BuilderDelegate builderDelegate) =>
        mainMenuBuilderDelegate += builderDelegate;
    
    public static void AddElementToSettingsMenu(BuilderDelegate builderDelegate) =>
        settingsMenuBuilderDelegate += builderDelegate;
    
    public static void AddElementToColorMenu(BuilderDelegate builderDelegate) =>
        colorMenuBuilderDelegate += builderDelegate;

    public static void AddElementToLobbyMenu(BuilderDelegate builderDelegate) =>
        lobbyMenuBuilderDelegate += builderDelegate;

    public static void AddElementToEscapeMenu(BuilderDelegate builderDelegate) =>
        escapeMenuBuilderDelegate += builderDelegate;
    
    public static void AddElementToRegionSelectionMenu(BuilderDelegate builderDelegate) =>
        regionSelectionMenuBuilderDelegate += builderDelegate;
    
    public static void AddElementToServerListMenu(BuilderDelegate builderDelegate) =>
        serverListMenuBuilderDelegate += builderDelegate;

    public static void CloseAllPagesAddedOnTop() => MenuManager.instance.PageCloseAllAddedOnTop();
    
    public static void OpenPopup(string header, Color headerColor, string content, Action onLeftClicked, Action onRightClicked = null)
    {
        if (!menuButtonPopup)
            menuButtonPopup = MenuManager.instance.gameObject.AddComponent<MenuButtonPopUp>();

        menuButtonPopup.option1Event = new UnityEvent();
        menuButtonPopup.option2Event = new UnityEvent();

        if (onLeftClicked != null)
            menuButtonPopup.option1Event.AddListener(new UnityAction(onLeftClicked));

        if (onRightClicked != null)
            menuButtonPopup.option2Event.AddListener(new UnityAction(onRightClicked));
        
        MenuManager.instance.PagePopUpTwoOptions(menuButtonPopup, header, null, headerColor, content, null, "Yes", null, "No", null, true);
    }

    public static REPOButton CreateREPOButton(string text, Action onClick, Transform parent, Vector2 localPosition = default)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.buttonTemplate, parent);
        newRectTransform.name = $"Button - {text}";

        newRectTransform.localPosition = localPosition;

        var repoButton = newRectTransform.gameObject.AddComponent<REPOButton>();

        repoButton.labelTMP.text = text;
        repoButton.onClick = onClick;

        return repoButton;
    }

    public static REPOToggle CreateREPOToggle(string text, Action<bool> onToggle, Transform parent, Vector2 localPosition = default, string leftButtonText = "ON", string rightButtonText = "OFF", bool defaultValue = false)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.toggleTemplate, parent);
        newRectTransform.name = $"Toggle - {text}";

        newRectTransform.localPosition = localPosition;

        var repoToggle = newRectTransform.gameObject.AddComponent<REPOToggle>();

        repoToggle.labelTMP.text = text;
        repoToggle.leftButtonTMP.text = leftButtonText;
        repoToggle.rightButtonTMP.text = rightButtonText;
        repoToggle.onToggle = onToggle;

        repoToggle.SetState(defaultValue, false);
        return repoToggle;
    }

    public static REPOInputField CreateREPOInputField(string labelText, Action<string> onValueChanged, Transform parent, Vector2 localPosition = default, bool onlyNotifyOnSubmit = false, string placeholder = "", string defaultValue = "")
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.toggleTemplate, parent);
        newRectTransform.name = $"Input Field - {labelText}";
        
        newRectTransform.localPosition = localPosition;

        var repoInputField = newRectTransform.gameObject.AddComponent<REPOInputField>();

        repoInputField.labelTMP.text = labelText;
        repoInputField.inputStringSystem.onValueChanged = onValueChanged;
        repoInputField.inputStringSystem.onlyNotifyOnSubmit = onlyNotifyOnSubmit;
        repoInputField.inputStringSystem.placeholder = placeholder;
        repoInputField.inputStringSystem.SetValue(defaultValue, false);
        
        return repoInputField;
    }
    
    public static REPOSlider CreateREPOSlider(string text, string description, Action<float> onValueChanged, Transform parent, Vector2 localPosition = default, float min = 0f, float max = 1f, int precision = 2, float defaultValue = 0f, string prefix = "", string postfix = "", REPOSlider.BarBehavior barBehavior = REPOSlider.BarBehavior.UpdateWithValue)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.sliderTemplate, parent);
        newRectTransform.name = $"Float Slider - {text}";

        newRectTransform.localPosition = localPosition;

        var repoSlider = newRectTransform.gameObject.AddComponent<REPOSlider>();

        repoSlider.labelTMP.text = text;
        repoSlider.descriptionTMP.text = description;
        repoSlider.onValueChanged = onValueChanged;
        repoSlider.min = min;
        repoSlider.max = max;
        repoSlider.precision = precision;
        repoSlider.prefix = prefix;
        repoSlider.postfix = postfix;
        repoSlider.barBehavior = barBehavior;

        repoSlider.SetValue(defaultValue, false);
        return repoSlider;
    }

    public static REPOSlider CreateREPOSlider(string text, string description, Action<int> onValueChanged, Transform parent, Vector2 localPosition = default, int min = 0, int max = 1, int defaultValue = 0, string prefix = "", string postfix = "", REPOSlider.BarBehavior barBehavior = REPOSlider.BarBehavior.UpdateWithValue)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.sliderTemplate, parent);
        newRectTransform.name = $"Int Slider - {text}";

        newRectTransform.localPosition = localPosition;

        var repoSlider = newRectTransform.gameObject.AddComponent<REPOSlider>();

        repoSlider.labelTMP.text = text;
        repoSlider.descriptionTMP.text = description;
        repoSlider.onValueChanged = f => onValueChanged.Invoke(Convert.ToInt32(f));
        repoSlider.min = min;
        repoSlider.max = max;
        repoSlider.precision = 0;
        repoSlider.prefix = prefix;
        repoSlider.postfix = postfix;
        repoSlider.barBehavior = barBehavior;

        repoSlider.SetValue(defaultValue, false);
        return repoSlider;
    }

    public static REPOSlider CreateREPOSlider(string text, string description, Action<string> onOptionChanged, Transform parent, string[] stringOptions, string defaultOption, Vector2 localPosition = default, string prefix = "", string postfix = "", REPOSlider.BarBehavior barBehavior = REPOSlider.BarBehavior.UpdateWithValue)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.sliderTemplate, parent);
        newRectTransform.name = $"Option Slider - {text}";

        newRectTransform.localPosition = localPosition;

        var repoSlider = newRectTransform.gameObject.AddComponent<REPOSlider>();

        repoSlider.labelTMP.text = text;
        repoSlider.descriptionTMP.text = description;
        repoSlider.onValueChanged = f =>
            onOptionChanged.Invoke(repoSlider.stringOptions.ElementAtOrDefault(Convert.ToInt32(f)) ??
                                   repoSlider.stringOptions.FirstOrDefault());
        repoSlider.stringOptions = stringOptions;
        repoSlider.prefix = prefix;
        repoSlider.postfix = postfix;
        repoSlider.barBehavior = barBehavior;

        var defaultIndex = Array.IndexOf(stringOptions, defaultOption);

        if (defaultIndex == -1)
            defaultIndex = 0;

        repoSlider.SetValue(defaultIndex, false);
        return repoSlider;
    }

    public static REPOSlider CreateREPOSlider(string text, string description, Action<int> onOptionChanged, Transform parent, string[] stringOptions, string defaultOption, Vector2 localPosition = default, string prefix = "", string postfix = "", REPOSlider.BarBehavior barBehavior = REPOSlider.BarBehavior.UpdateWithValue)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.sliderTemplate, parent);
        newRectTransform.name = $"Option Slider - {text}";

        newRectTransform.localPosition = localPosition;

        var repoSlider = newRectTransform.gameObject.AddComponent<REPOSlider>();

        repoSlider.labelTMP.text = text;
        repoSlider.descriptionTMP.text = description;
        repoSlider.onValueChanged = f => onOptionChanged.Invoke(Convert.ToInt32(f));
        repoSlider.stringOptions = stringOptions;
        repoSlider.prefix = prefix;
        repoSlider.postfix = postfix;
        repoSlider.barBehavior = barBehavior;

        var defaultIndex = Array.IndexOf(stringOptions, defaultOption);

        if (defaultIndex == -1)
            defaultIndex = 0;

        repoSlider.SetValue(defaultIndex, false);
        return repoSlider;
    }

    public static REPOLabel CreateREPOLabel(string text, Transform parent, Vector2 localPosition = default)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.labelTemplate, parent);
        newRectTransform.name = $"Label - {text}";

        newRectTransform.localPosition = localPosition;

        var repoLabel = newRectTransform.gameObject.AddComponent<REPOLabel>();

        repoLabel.labelTMP.text = text;

        return repoLabel;
    }

    public static REPOSpacer CreateREPOSpacer(Transform parent, Vector2 localPosition = default, Vector2 size = default)
    {
        var newRectTransform = (RectTransform)new GameObject("Spacer", typeof(RectTransform)).transform;

        newRectTransform.SetParent(parent);

        var repoSpacer = newRectTransform.gameObject.AddComponent<REPOSpacer>();

        newRectTransform.localPosition = localPosition;
        newRectTransform.sizeDelta = size;

        return repoSpacer;
    }

    [Obsolete("Switch to the overload with the 'shouldCachePage' argument!")]
    public static REPOPopupPage CreateREPOPopupPage(string headerText, REPOPopupPage.PresetSide presetSide, bool pageDimmerVisibility = false, float spacing = 0) => CreateREPOPopupPage(headerText, pageDimmerVisibility, spacing, presetSide == REPOPopupPage.PresetSide.Left ? null : new Vector2(40, 0));

    [Obsolete("Switch to the overload with the 'shouldCachePage' argument!")]
    public static REPOPopupPage CreateREPOPopupPage(string headerText, bool pageDimmerVisibility = false, float spacing = 0, Vector2? localPosition = null) => CreateREPOPopupPage(headerText, false, pageDimmerVisibility, spacing, localPosition);

    public static REPOPopupPage CreateREPOPopupPage(string headerText, REPOPopupPage.PresetSide presetSide, bool shouldCachePage, bool pageDimmerVisibility = false, float spacing = 0) => CreateREPOPopupPage(headerText, shouldCachePage, pageDimmerVisibility, spacing, presetSide == REPOPopupPage.PresetSide.Left ? null : new Vector2(40, 0));

    public static REPOPopupPage CreateREPOPopupPage(string headerText, bool shouldCachePage, bool pageDimmerVisibility = false, float spacing = 0, Vector2? localPosition = null)
    {
        var newRectTransform = Object.Instantiate(REPOTemplates.popupPageTemplate, MenuHolder.instance.transform);
        newRectTransform.name = $"Menu Page {headerText}";

        var repoPopupPage = newRectTransform.gameObject.AddComponent<REPOPopupPage>();

        repoPopupPage.rectTransform.localPosition = localPosition ?? new Vector2(-280, 0);
        repoPopupPage.headerTMP.text = headerText;
        repoPopupPage.isCachedPage = shouldCachePage;
        repoPopupPage.pageDimmerVisibility = pageDimmerVisibility;
        repoPopupPage.scrollView.spacing = spacing;
        
        return repoPopupPage;
    }

    public static REPOAvatarPreview CreateREPOAvatarPreview(Transform parent, Vector2 localPosition = default, bool enableBackgroundImage = false, Color? backgroundImageColor = null)
    {
        var newTransform = Object.Instantiate(REPOTemplates.avatarPreviewTemplate, parent);
        newTransform.name = "Player Avatar Preview";

        var repoAvatarPreview = newTransform.gameObject.AddComponent<REPOAvatarPreview>();

        repoAvatarPreview.rectTransform.localPosition = localPosition;
        repoAvatarPreview.enableBackgroundImage = enableBackgroundImage;
        repoAvatarPreview.backgroundImageColor = backgroundImageColor ?? Color.white;
        
        repoAvatarPreview.previewSize = new Vector2(184f, 345f);
        
        return repoAvatarPreview;
    }

    public static REPOObjectPreview CreateREPOObjectPreview(Transform parent, GameObject previewObject, Vector2 localPosition = default, bool enableBackgroundImage = false, Color? backgroundImageColor = null)
    {
        var newTransform = Object.Instantiate(REPOTemplates.avatarPreviewTemplate, parent);
        newTransform.name = "Object Preview";

        var repoObjectPreview = newTransform.gameObject.AddComponent<REPOObjectPreview>();

        repoObjectPreview.rectTransform.localPosition = localPosition;
        repoObjectPreview.enableBackgroundImage = enableBackgroundImage;
        repoObjectPreview.backgroundImageColor = backgroundImageColor ?? Color.white;
        repoObjectPreview.previewObject = previewObject;
        
        repoObjectPreview.previewSize = new Vector2(184f, 345f);
        
        return repoObjectPreview;
    }
    
    internal static void OpenMenuPage(MenuPage menuPage, bool pageOnTop)
    {
        var currentMenuPage = REPOReflection.menuManager_CurrentMenuPage.GetValue(MenuManager.instance) as MenuPage;
        
        var addedPagesOnTop = REPOReflection.menuManager_AddedPagesOnTop.GetValue(MenuManager.instance) as List<MenuPage>;
        
        if (pageOnTop && !currentMenuPage)
            pageOnTop = false;
        
        switch (pageOnTop)
        {
            case true when addedPagesOnTop == null || addedPagesOnTop.Contains(currentMenuPage):
                return;
            case false when currentMenuPage:
                REPOReflection.menuManager_PageInactiveAdd.Invoke(MenuManager.instance, [ currentMenuPage ]);
                currentMenuPage.PageStateSet(MenuPage.PageState.Inactive);
                break;
        }
        
        menuPage.transform.SetAsLastSibling();

        menuPage.enabled = true;
        menuPage.ResetPage();
        menuPage.PageStateSet(MenuPage.PageState.Opening);
        
        MenuManager.instance.PageAdd(menuPage);
        menuPage.StartCoroutine(REPOReflection.menuPage_LateStart.Invoke(menuPage, null) as IEnumerator);

        REPOReflection.menuPage_AddedPageOnTop.SetValue(menuPage, pageOnTop);
        
        if (!pageOnTop)
        {
            MenuManager.instance.PageSetCurrent(menuPage.menuPageIndex, menuPage);
        
            REPOReflection.menuPage_PageIsOnTopOfOtherPage.SetValue(menuPage, true);
            REPOReflection.menuPage_PageUnderThisPage.SetValue(menuPage, currentMenuPage);
            return;
        }
        
        REPOReflection.menuPage_ParentPage.SetValue(menuPage, currentMenuPage);
        addedPagesOnTop.Add(menuPage);
    }

    internal static void CloseMenuPage(MenuPage menuPage, bool closePagesAddedOnTop)
    {
        if (closePagesAddedOnTop)
            CloseAllPagesAddedOnTop();
        
        menuPage.PageStateSet(MenuPage.PageState.Closing);
        
        if (REPOReflection.menuPage_PageUnderThisPage.GetValue(menuPage) is MenuPage parentPage)
            MenuManager.instance.PageSetCurrent(parentPage.menuPageIndex, parentPage);
    }
}