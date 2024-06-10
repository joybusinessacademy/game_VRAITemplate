using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultipleChoiceQuestionItem : MonoBehaviour
{
	public Button questionButton;

	public TextMeshProUGUI questionText;
	public bool isCorrectItem;

	internal bool isSelectedQuestion = false;

	internal bool stayInSelectedColor = false;

	internal ColorBlock originalColors;
	public ColorBlock selectedColors;

	public GameObject correctIcon, incorrectIcon;

	private void Awake()
	{
		SetupButton();
	}

	private void OnDestroy()
	{
		questionButton?.onClick.RemoveListener(SelectedQuestion);
	}

	public void SetupButton()
	{
		questionButton?.onClick.RemoveListener(SelectedQuestion);
		questionButton?.onClick.AddListener(SelectedQuestion);

		originalColors = questionButton.colors;
	}

	public void SetMultipleChoiceData(string text, bool state)
	{
		questionText.text = text;
		isCorrectItem = state;
		SetupButton();
	}

	private void SelectedQuestion()
	{
		isSelectedQuestion = !isSelectedQuestion;

		if(stayInSelectedColor)
		{
			StartCoroutine(ChangeAfterDelay());
		}
	}

	private IEnumerator ChangeAfterDelay()
	{
		yield return new WaitForEndOfFrame();

		questionButton.colors = isSelectedQuestion ? selectedColors : originalColors;

		//yield return new WaitForEndOfFrame();

		//LayoutRebuilder.MarkLayoutForRebuild(this.transform.parent as RectTransform);
		//LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform.parent as RectTransform);
	}
}
