using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MechanicTweenSystem
{
	public float timeBtwnChars = 0.03f;

	public IEnumerator ScaleUpOrDown(Transform transform, float duration, bool direction, Callback<bool> onActionComplete = null)
	{
		if (transform == null)
		{
			Debug.LogWarning("Missing Transform for Tween.");
			yield break;
		}

		Vector3 initialScale = !direction ? Vector3.one : new Vector3(0.8f, 0.8f, 0.8f);
		Vector3 targetScale = direction ? Vector3.one : new Vector3(0.8f, 0.8f, 0.8f);

		transform.localScale = initialScale;

		float time = 0;

		while (time < duration)
		{
			time += Time.deltaTime;
			float progress = time / duration;
			//float progress = duration - (duration - time) * (duration - time); // EaseOutQuad
			transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
			yield return null;
		}

		transform.localScale = targetScale;

		onActionComplete?.Invoke(direction);
	}

	public IEnumerator TextPrintingVisible(TextMeshProUGUI textMeshPro, Callback onActionComplete = null)
	{
		if (textMeshPro == null)
		{
			Debug.LogWarning("Missing TextMeshProUGUI for Tween.");
			yield break;
		}

		textMeshPro.ForceMeshUpdate();
		int totalVisibleCharacters = textMeshPro.textInfo.characterCount;
		int counter = 0;
		int visibleCount = 0;

		while (counter <= totalVisibleCharacters)
		{
			visibleCount = counter % (totalVisibleCharacters + 1);
			textMeshPro.maxVisibleCharacters = visibleCount;
			counter++;

			yield return new WaitForSeconds(timeBtwnChars);
		}

		onActionComplete?.Invoke();
	}


	public IEnumerator ScaleUpOrDownCustomSize(Transform transform, float duration, bool direction, Vector3 startSize, Vector3 endSize)
	{
		if (transform == null)
		{
			Debug.LogWarning("Missing Transform for Tween.");
			yield break;
		}

		Vector3 initialScale = !direction ? endSize : startSize;
		Vector3 targetScale = direction ? endSize : startSize;

		transform.localScale = initialScale;

		float time = 0;

		while (time < duration)
		{
			time += Time.deltaTime;
			float progress = time / duration;
			//float progress = duration - (duration - time) * (duration - time); // EaseOutQuad
			transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
			yield return null;
		}

		transform.localScale = targetScale;
	}


	public IEnumerator FadeCanvas(CanvasGroup canvasgroup, bool direction, float timeToFade, Callback<bool> onActionComplete = null)
	{
		if (canvasgroup == null)
		{
			Debug.LogWarning("Missing List<CanvasGroup> for Tween, or no objects in list");
			yield break;
		}

		float time = 0;

		float startAlpha = direction ? 0 : 1;
		float endAlpha = direction ? 1 : 0;

		canvasgroup.alpha = startAlpha;


		while (time < timeToFade)
		{
			time += Time.deltaTime;
			float progress = time / timeToFade;
			canvasgroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
			yield return null;
		}

		time = 0;
		canvasgroup.alpha = endAlpha;


		onActionComplete?.Invoke(direction);
	}

	public IEnumerator FadeCanvasObjects(List<CanvasGroup> canvasgroups, bool direction, float timeToFade, Callback<bool> onActionComplete = null)
	{
		if (canvasgroups == null || canvasgroups.Count == 0)
		{
			Debug.LogWarning("Missing List<CanvasGroup> for Tween, or no objects in list");
			yield break;
		}

		float time = 0;

		float startAlpha = direction ? 0 : 1;
		float endAlpha = direction ? 1 : 0;

		canvasgroups.ForEach(x => x.alpha = startAlpha);

		for (int i = 0; i < canvasgroups.Count; i++)
		{
			while (time < timeToFade)
			{
				time += Time.deltaTime;
				float progress = time / timeToFade;
				canvasgroups[i].alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
				yield return null;
			}

			time = 0;
			canvasgroups[i].alpha = endAlpha;
		}

		onActionComplete?.Invoke(direction);
	}

	public IEnumerator ScaleObjects(List<GameObject> gameobjects, bool direction, float timeToScale, Callback<bool> onActionComplete = null)
	{
		if (gameobjects == null || gameobjects.Count == 0)
		{
			Debug.LogWarning("Missing List<GameObject> for Tween, or no objects in list");
			yield break;
		}

		float time = 0;

		Vector3 initialScale = !direction ? Vector3.one : Vector3.zero;
		Vector3 targetScale = direction ? Vector3.one : Vector3.zero;

		gameobjects.ForEach(x => x.transform.localScale = initialScale);

		for (int i = 0; i < gameobjects.Count; i++)
		{
			while (time < timeToScale)
			{
				time += Time.deltaTime;
				float progress = time / timeToScale;
				gameobjects[i].transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
				yield return null;
			}

			time = 0;
			gameobjects[i].transform.localScale = targetScale;
		}

		onActionComplete?.Invoke(direction);
	}

	public IEnumerator CallToAction(Transform transform, float duration,float speed, float lengthOfRotation, Callback onActionComplete = null)
	{
		if (transform == null)
		{
			Debug.LogWarning("Missing Transform for Tween.");
			yield break;
		}

		Quaternion initialRotation = transform.rotation;

		float time = 0;

		while (time < duration)
		{
			time += Time.deltaTime;
			transform.rotation = Quaternion.Euler(initialRotation.x, initialRotation.y, Mathf.PingPong(Time.time * speed, lengthOfRotation * 2) - lengthOfRotation);
			yield return null;
		}

		transform.rotation = initialRotation;

		onActionComplete?.Invoke();
	}
}