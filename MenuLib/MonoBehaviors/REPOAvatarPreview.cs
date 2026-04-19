using UnityEngine;
using UnityEngine.UI;

namespace MenuLib.MonoBehaviors;

public sealed class REPOAvatarPreview : REPOElement
{
    public bool enableBackgroundImage
    {
        get => backgroundImage.enabled;
        set => backgroundImage.enabled = value;
    }

    public Color backgroundImageColor
    {
        get => backgroundImage.color;
        set => backgroundImage.color = value;
    }

    public Vector2 previewSize
    {
        get => rectTransform.sizeDelta;
        set
        {
            const float ASPECT_RATIO = 0.53333336f;
            
            if (value.x > value.y)
                value = value with { y = value.x / ASPECT_RATIO };
            else
                value = value with { x = value.y * ASPECT_RATIO };
            
            renderTextureRectTransform.sizeDelta = rectTransform.sizeDelta = value;
            renderTextureRectTransform.localPosition = Vector3.zero;
        }
    }
    
    public PlayerAvatarVisuals playerAvatarVisuals { get; private set; }

    public Transform rigTransform => playerAvatarVisuals.meshParent.transform;
    
    private PlayerAvatarMenu playerAvatarMenu;
    private Image backgroundImage;
    private RectTransform renderTextureRectTransform;
    
    private void Awake()
    {
        LocalizationChangedEvent[] localizationChangedEvents = GetComponentsInChildren<LocalizationChangedEvent>(true);
        foreach (var localizationChangedEvent in localizationChangedEvents)
        {
            localizationChangedEvent.localizedAsset = null;
        }

        rectTransform = gameObject.AddComponent<RectTransform>();
        rectTransform.pivot = Vector2.right;
        rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;

        renderTextureRectTransform = (RectTransform) rectTransform.GetChild(1);
        renderTextureRectTransform.localPosition = Vector3.zero;
        
        playerAvatarMenu = GetComponentInChildren<PlayerAvatarMenuHover>().playerAvatarMenu;
        playerAvatarVisuals = playerAvatarMenu.GetComponentInChildren<PlayerAvatarVisuals>();
        
        backgroundImage = gameObject.AddComponent<Image>();
        backgroundImage.enabled = false;
    }

    private void OnDestroy()
    {
        if (!playerAvatarMenu)
            return;
        
        if (playerAvatarMenu.cameraAndStuff)
            Destroy(playerAvatarMenu.cameraAndStuff.gameObject);
        
        Destroy(playerAvatarMenu.gameObject);
    }
}
