using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MenuLib.MonoBehaviors;

public sealed class REPOInputField : REPOElement
{
    public TextMeshProUGUI labelTMP;
    
    public REPOInputStringSystem inputStringSystem;

    private RectTransform inputAreaRectTransform;
    private MenuPage menuPage;
    private string menuID;

    public Vector2 GetLabelSize() => labelTMP.GetPreferredValues();
    
    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        rectTransform = transform as RectTransform;
        menuPage = GetComponentInParent<MenuPage>();
        menuID = SemiFunc.MenuGetSelectableID(gameObject);
        labelTMP = GetComponentInChildren<TextMeshProUGUI>();
        
        rectTransform.sizeDelta -= new Vector2(0, 2);
        labelTMP.rectTransform.sizeDelta -= new Vector2(0, 2);
        
        var horizontalShift = Vector3.left * 5.3f;
        var verticalShift = Vector3.up;
        
        transform.Find("SliderBG").localPosition += horizontalShift + verticalShift;
        transform.Find("RawImage").localPosition += horizontalShift + verticalShift;

        labelTMP.rectTransform.localPosition += horizontalShift + verticalShift;

        var inputBox = (RectTransform) transform.Find("RawImage (1) Left");
        inputBox.localPosition = transform.Find("RawImage").localPosition;
        inputBox.sizeDelta = new Vector2(148f, 21f);

        TextMeshProUGUI inputTMP = null;
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            switch (child.name)
            {
                case "Option Box" or "Option Box Behind" or "RawImage (1) Right":
                {
                    Destroy(child.gameObject);
                    break;
                }
                case "Button BUTTON":
                {
                    if (!inputTMP)
                    {
                        inputTMP = child.GetComponent<MenuButton>().GetComponentInChildren<TextMeshProUGUI>();;
                        inputTMP.transform.SetParent(child.parent);
                    }
                    
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
        
        inputTMP!.rectTransform.pivot = Vector2.zero;
        inputTMP.rectTransform.anchorMin = Vector2.zero;
        inputTMP.rectTransform.anchorMax = Vector2.one;
        inputTMP.fontStyle = FontStyles.Normal;

        inputTMP.enableAutoSizing = false;
        inputTMP.alignment = TextAlignmentOptions.Left;
        inputTMP.margin = new Vector4(2, 2, 0, 0);
        inputTMP.fontSizeMax = inputTMP.fontSizeMin =  inputTMP.fontSize = 18f;
        
        inputAreaRectTransform = new GameObject("Input Area") { transform = { parent = transform } }.AddComponent<RectTransform>();
        inputAreaRectTransform.sizeDelta = new Vector2(146.5f, 20f);
        inputAreaRectTransform.pivot = Vector2.zero;
        inputAreaRectTransform.localPosition = new Vector3(108f, 1.5f, 0) + horizontalShift;

        inputAreaRectTransform.gameObject.AddComponent<RectMask2D>();

        inputTMP.transform.SetParent(inputAreaRectTransform);
        inputTMP.transform.localPosition = inputTMP.rectTransform.sizeDelta = Vector3.zero;
        
        Destroy(GetComponent<MenuSettingElement>());
        Destroy(GetComponent<AudioButtonPushToTalk>());
        
        inputStringSystem = gameObject.AddComponent<REPOInputStringSystem>();
        inputStringSystem.inputTMP = inputTMP;
        inputStringSystem.maskRectTransform = inputAreaRectTransform;
    }
    
    private void Update()
    {
        inputStringSystem.SetHovering(SemiFunc.UIMouseHover(menuPage, inputAreaRectTransform, menuID, 2f, 2f));
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
            inputStringSystem.isFocused = inputStringSystem.isHovering;
    }
}