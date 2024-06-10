using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private RectTransform rectTransform;
	private Vector3 originalScale;
	private float scalePercentage = 10f; // Percentage increase of the scale on hover
	private float scaleDuration = 0.1f;
	private bool isHovered = false;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		originalScale = rectTransform.localScale;
	}

	private IEnumerator ScaleTo(Vector3 targetScale)
	{
		float elapsedTime = 0f;
		Vector3 startScale = rectTransform.localScale;

		while (elapsedTime < scaleDuration)
		{
			rectTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		rectTransform.localScale = targetScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!isHovered)
		{
			StopAllCoroutines();
			Vector3 hoverScale = originalScale * (1 + scalePercentage / 100f);

			//If button is not interactable then dont scale
			if (!(this.GetComponent<Button>()?.interactable ?? true))
				return;

			StartCoroutine(ScaleTo(hoverScale));
			isHovered = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isHovered)
		{
			StopAllCoroutines();
			StartCoroutine(ScaleTo(originalScale));
			isHovered = false;
		}
	}
}
