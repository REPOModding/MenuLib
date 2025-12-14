using TMPro;
using UnityEngine;

namespace MenuLib.MonoBehaviors;

public sealed class REPOLabel : REPOElement
{
	public TextMeshProUGUI labelTMP;
	public float maxFontSize = 30f;
	public float minFontSize = 20f;
	public bool autoScaleToFit = true;

	private void Awake()
	{
		rectTransform = (RectTransform)transform;
		labelTMP = GetComponentInChildren<TextMeshProUGUI>();

		labelTMP.rectTransform.pivot = rectTransform.pivot = Vector2.zero;
		labelTMP.rectTransform.sizeDelta = rectTransform.sizeDelta = new Vector2(200f, 30f);

		labelTMP.enableAutoSizing = false;
		labelTMP.fontSize = maxFontSize;

		labelTMP.enableWordWrapping = false;
		labelTMP.alignment = TextAlignmentOptions.Left;
		labelTMP.margin = Vector4.zero;
	}

	private void Start()
	{
		labelTMP.rectTransform.localPosition = Vector2.zero;

		if (autoScaleToFit)
		{
			ScaleFontToFit();
		}
	}

	public void ScaleFontToFit()
	{
		labelTMP.fontSize = maxFontSize;
		labelTMP.ForceMeshUpdate();

		var textBounds = labelTMP.textBounds;
		var rectWidth = labelTMP.rectTransform.rect.width;

		if (textBounds.size.x > rectWidth)
		{
			float scaleFactor = rectWidth / textBounds.size.x;
			float newSize = Mathf.Max(minFontSize, maxFontSize * scaleFactor * 0.95f);
			labelTMP.fontSize = newSize;
			labelTMP.ForceMeshUpdate();
		}
	}

	public void SetText(string text, bool scaleToFit = true)
	{
		labelTMP.text = text;

		if (scaleToFit)
		{
			ScaleFontToFit();
		}
	}
}