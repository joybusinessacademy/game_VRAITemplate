using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SkillsVR.UnityExtenstion;
using System;

public enum MultipleChoiceFeedback
{
	NOFEEDBACK,
	SELECTEDANSWERONLY,
	SELECTEDPLUSCORRECTIVE
}

public class MultipleChoiceSystem : AbstractMechanicSystemBehivour<MultipleChoiceQuestionScriptable>, IMultipleChoiceQuestionSystem
{
	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}

	[Header("Fields")]
	public GameObject mechanicVisualHolder;
	public GameObject titleVisual;
	public GameObject multiSelectionTitle;

	public CanvasGroup canvasGroup;

	public TextMeshProUGUI questionText;
	public TextMeshProUGUI multiQuestionText;

	public MultipleChoiceQuestionItem multipleChoiceQuestionItem;

	public Transform questionButtonContainer;

	public AudioSource audioSource;

	public MultipleChoiceFeedback choiceFeedback;

	[SerializeField]
	[Header("Question")]
	private bool endOnIncorrectSelection = false;

	[SerializeField]
	private List<MultipleChoiceQuestionItem> multipleChoiceQuestionsSpawned = new List<MultipleChoiceQuestionItem>();

	[Header("Colors")]
	public Color correctColor;
	public Color incorrectColor;

	[Header("Sounds")]
	public AudioClip incorrectSound;
	public AudioClip correctSound;
	public AudioClip neutralSound;

	public List<ITelemetry> telemetries = new List<ITelemetry>();

	SelectionTelemetry selectionTelemetry = new SelectionTelemetry();
	AnswerTelemetry answerTelemetry = new AnswerTelemetry();
	TimeElpasedTelemetry timeElpasedTelemetry;
	TimedTelemetry timedTelemetry;

	//private GameObject mechanicSpawner => base.spawner;

	internal MultipleChoiceQuestionScriptable mcqScriptable;

	MechanicTweenSystem mechanicTweenSystem = new MechanicTweenSystem();

	private Coroutine tweeningCoroutine;
	private Coroutine fadingCoroutine;

	[Header("Animation Items")]
	public float speedOfTween = 0.3f;
	public float fadeObjectsTime = 0.3f;

	private float timeUntilTurnedOfXSeconds = 5f;

	public int currentSelectedChoicesAmount = 0;
	private Dictionary<string,bool> allSelectedCorrect = new Dictionary<string,bool>();
	public GameObject submitButton;

	protected override void Awake()
	{
		base.Awake();

	}

	protected override void Start()
	{
		base.Start();

		timedTelemetry = this.transform.GetOrAddComponent<TimedTelemetry>();
		timeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();

		telemetries.Add(timedTelemetry);
		telemetries.Add(selectionTelemetry);
		telemetries.Add(timeElpasedTelemetry);
		telemetries.Add(answerTelemetry);

		string mechID = "Multiple Choice Question: " + this.gameObject.GetInstanceID().ToString();
		timedTelemetry.id = mechID;
		timeElpasedTelemetry.id = mechID;
		answerTelemetry.id = mechID;
		selectionTelemetry.id = mechID;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		multipleChoiceQuestionsSpawned.ForEach(choice => choice.isSelectedQuestion = false);
	}

	public override void Reset()
	{
		base.Reset();
		foreach (var telemetry in telemetries)
		{
			if (null == telemetry)
			{
				continue;
			}
			telemetry.isCompleted = false;
		}
		multipleChoiceQuestionsSpawned.ForEach(choice => choice.isSelectedQuestion = false);
		allSelectedCorrect.Clear();
		scalingQuestionItems.Clear();
		currentSelectedChoicesAmount = 0;

		foreach (var item in multipleChoiceQuestionsSpawned)
		{
			if (null != item && null != item.gameObject)
			{
				item.transform.SetParent(null);
				item.gameObject.SetActive(false);
				GameObject.Destroy(item.gameObject);
			}
		}
		multipleChoiceQuestionsSpawned.Clear();
		canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
	}

	public override void SetVisualState(bool state)
	{
		base.SetVisualState(state);

		if (tweeningCoroutine != null)
		{
			StopCoroutine(tweeningCoroutine);
			tweeningCoroutine = null;
		}

		if (fadingCoroutine != null)
		{
			StopCoroutine(fadingCoroutine);
			fadingCoroutine = null;
		}

		if (state)
		{
			scalingQuestionItems.ForEach(x => x.alpha = 0);
			mechanicVisualHolder?.SetActive(state);
		}

		if (null != mechanicVisualHolder && this.gameObject.activeInHierarchy)
		{
			tweeningCoroutine = StartCoroutine(mechanicTweenSystem.ScaleUpOrDown(mechanicVisualHolder.transform, speedOfTween, state, FinishedScaling));
			fadingCoroutine = StartCoroutine(mechanicTweenSystem.FadeCanvas(mechanicVisualHolder.GetComponent<CanvasGroup>(), state, fadeObjectsTime));
		}
	}



	private void FinishedScaling(bool direction)
	{
		mechanicVisualHolder?.SetActive(direction);

		if (direction)
		{
			StartCoroutine(mechanicTweenSystem.FadeCanvasObjects(scalingQuestionItems, direction, fadeObjectsTime,FinishedCanvasObjects));
		}
	}

	private void FinishedCanvasObjects(bool arg1)
	{
		submitButton.SetActive(mcqScriptable.allowMultipleSelection);
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		mcqScriptable = mechanicData as MultipleChoiceQuestionScriptable;

		if (mcqScriptable == null)
		{
			Debug.LogError("Missing Data");
			return;
		}

		//Visual State
		mechanicVisualHolder?.SetActive(mcqScriptable.isVisualOnSpawn);

		//Test Information
		endOnIncorrectSelection = mcqScriptable.endOnIncorrectAnswer;

		titleVisual.SetActive(mcqScriptable.questionTitle != string.Empty);
		multiQuestionText.text = mcqScriptable.questionTitle;
		questionText.text = mcqScriptable.questionTitle;
		choiceFeedback = mcqScriptable.multipleChoiceFeedback;

		int count = 1;
		foreach (var answerData in mcqScriptable.questions)
		{
			MultipleChoiceQuestionItem questionItem = Instantiate(multipleChoiceQuestionItem, questionButtonContainer);
			questionItem.SetMultipleChoiceData(answerData.answerText, answerData.isCorrectAnswer);
			questionItem.questionButton.onClick.AddListener(() => SetMechanicInteractable(false));
			questionItem.questionButton.onClick.AddListener(() => OnItemSelectionStateChanged(questionItem));
			questionItem.questionButton.onClick.AddListener(() => StartCoroutine(SetFeedbackBasedOnType()));
			questionItem.gameObject.name = "Button: " + count;
			multipleChoiceQuestionsSpawned.Add(questionItem);
			scalingQuestionItems.Add(questionItem.GetOrAddComponent<CanvasGroup>());

			if (mcqScriptable.allowMultipleSelection)
			{
				questionItem.stayInSelectedColor = true;
				questionItem.questionButton.onClick.AddListener(() => MultipleSelectionButtonSelected(questionItem));
			}

			count++;
		}

		if (mcqScriptable.shuffleAnswers)
		{
			questionButtonContainer.ShuffleChildren();
			scalingQuestionItems.Clear();
			foreach (Transform child in questionButtonContainer)
			{
				scalingQuestionItems.Add(child.GetOrAddComponent<CanvasGroup>());
			}
		}

		timeUntilTurnedOfXSeconds = mcqScriptable.delayTimeUntilTurnedOff;

		//Setting Title
		titleVisual.SetActive(!mcqScriptable.allowMultipleSelection);
		multiSelectionTitle.SetActive(mcqScriptable.allowMultipleSelection);

		if (submitButton != null)
		{
			submitButton.GetComponent<Button>().interactable = false;
			submitButton.GetComponent<Button>().onClick.RemoveListener(OnSubmitButtonClicked);
			submitButton.GetComponent<Button>().onClick.AddListener(OnSubmitButtonClicked);
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(mechanicVisualHolder.transform as RectTransform);
	}

	private List<CanvasGroup> scalingQuestionItems = new List<CanvasGroup>();

	protected override void Update()
	{
		base.Update();
		foreach (var telemetry in telemetries)
		{
			if (!telemetry.isCompleted && telemetry.IsValidated())
				telemetry.SendEvents();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		multipleChoiceQuestionsSpawned?.ForEach(x => x.questionButton?.onClick.RemoveAllListeners());
	}

	private void MultipleSelectionButtonSelected(MultipleChoiceQuestionItem questionItem)
	{
		StartCoroutine(WaitForSelectionSwitch(questionItem));	
	}

	private IEnumerator WaitForSelectionSwitch(MultipleChoiceQuestionItem questionItem)
	{
		yield return new WaitForEndOfFrame();

		if (questionItem.isSelectedQuestion)
			currentSelectedChoicesAmount++;
		else
			currentSelectedChoicesAmount--;

		if (mcqScriptable.minSelectionAmount <= currentSelectedChoicesAmount)
			submitButton.GetComponent<Button>().interactable = true;
		else
			submitButton.GetComponent<Button>().interactable = false;
	}

	private void OnSubmitButtonClicked()
	{
		switch (choiceFeedback)
		{
			case MultipleChoiceFeedback.NOFEEDBACK:
				NoFeedback();
				break;
			case MultipleChoiceFeedback.SELECTEDANSWERONLY:
				SelectedAnswerOnly();
				break;
			case MultipleChoiceFeedback.SELECTEDPLUSCORRECTIVE:
				SelectedPlusCorrective();
				break;
			default:
				break;
		}

		canvasGroup.interactable = canvasGroup.blocksRaycasts = false;

		TriggerEvent(MCQEvent.FinishedMCQ);
		MCQFinishedMechanic();
	}

	private void OnItemSelectionStateChanged(MultipleChoiceQuestionItem questionItem)
	{
		if (null == questionItem)
		{
			return;
		}
		bool selected = questionItem.isSelectedQuestion;
		int index = multipleChoiceQuestionsSpawned.IndexOf(questionItem);
		var eventKey = selected ? MCQEvent.OnChoiceSelected : MCQEvent.OnChoiceUnselected;
		TriggerEvent(eventKey, index);
	}
	public IEnumerator SetFeedbackBasedOnType()
	{
		yield return new WaitForEndOfFrame();

		if (mcqScriptable.allowMultipleSelection)
			yield break;

		switch (choiceFeedback)
		{
			case MultipleChoiceFeedback.NOFEEDBACK:
				NoFeedback();
				break;
			case MultipleChoiceFeedback.SELECTEDANSWERONLY:
				SelectedAnswerOnly();
				break;
			case MultipleChoiceFeedback.SELECTEDPLUSCORRECTIVE:
				SelectedPlusCorrective();
				break;
			default:
				break;
		}
	}

	private void PlaySound(AudioClip clip)
	{
		if (audioSource && clip != null)
		{
			audioSource.clip = clip;
			audioSource.Play();
		}
	}

	private void NoFeedback()
	{
		PlaySound(neutralSound);
		TriggerEvent(MCQEvent.FinishedMCQ);

		foreach (var item in multipleChoiceQuestionsSpawned)
		{
			if (item.isSelectedQuestion)
			{
				ColorBlock cb = item.questionButton.colors;
				cb.disabledColor = cb.pressedColor;
				item.questionButton.colors = cb;

				if (item.isCorrectItem)
				{
					allSelectedCorrect.Add(item.name, true);
					TriggerEvent(MCQEvent.CorrectButton);
					CorrectAnswerSelected();
				}
				else
				{
					allSelectedCorrect.Add(item.name, false);
					TriggerEvent(MCQEvent.InCorrectButton);
					InCorrectAnswerSelected();
				}
			}
		}

		CheckMultipleSelectionAnswers();

		MCQFinishedMechanic();
	}

	private void CheckMultipleSelectionAnswers()
	{
		if (mcqScriptable.allowMultipleSelection)
		{
			if (allSelectedCorrect.ContainsValue(false))
				TriggerEvent(MCQEvent.MultipleSelectionIncorrect);
			else
				TriggerEvent(MCQEvent.MultipleSelectionCorrect);
		}
	}

	private bool AllSelectionsCorrect()
	{
		return !allSelectedCorrect.ContainsValue(false);
	}

	private void SelectedAnswerOnly()
	{
		foreach (var item in multipleChoiceQuestionsSpawned)
		{
			if (item.isSelectedQuestion)
			{
				ColorBlock cb = item.questionButton.colors;
				cb.disabledColor = item.isCorrectItem ? correctColor : incorrectColor;
				item.questionButton.colors = cb;

				PlaySound(item.isCorrectItem ? correctSound : incorrectSound);

				//Show Icon
				if (item.isCorrectItem)
					item.correctIcon.SetActive(true);
				else
					item.incorrectIcon.SetActive(true);

				//TELEMENTARY DATA
				selectionTelemetry.itemSelectedData = item.questionText.text;

				if (item.isCorrectItem)
				{
					allSelectedCorrect.Add(item.name, true);
					MCQFinishedMechanic();
					CorrectAnswerSelected();
					TriggerEvent(MCQEvent.CorrectButton);
				}
				else
				{
					allSelectedCorrect.Add(item.name, false);
					InCorrectAnswerSelected();
					TriggerEvent(MCQEvent.InCorrectButton);

					if (endOnIncorrectSelection)
					{
						canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
						MCQFinishedMechanic();
						TriggerEvent(MCQEvent.FinishedMCQ);
					}

				}
			}
		}
	}

	private void SelectedPlusCorrective()
	{
		bool answerIsCorrect = false;

		foreach (var item in multipleChoiceQuestionsSpawned)
		{
			if (item.isSelectedQuestion)
			{
				answerIsCorrect = item.isCorrectItem;

				ColorBlock cb = item.questionButton.colors;
				cb.disabledColor = item.isCorrectItem ? correctColor : incorrectColor;
				item.questionButton.colors = cb;

				PlaySound(answerIsCorrect ? correctSound : incorrectSound);

				//Show Icon
				if (item.isCorrectItem)
					item.correctIcon.SetActive(true);
				else
					item.incorrectIcon.SetActive(true);

				//TELEMENTARY DATA
				selectionTelemetry.itemSelectedData = item.questionText.text;

				if (item.isCorrectItem)
				{
					allSelectedCorrect.Add(item.name, true);
					CorrectAnswerSelected();
					TriggerEvent(MCQEvent.CorrectButton);

					if(!mcqScriptable.allowMultipleSelection)
						MCQFinishedMechanic();
				}
				else
				{
					allSelectedCorrect.Add(item.name, false);
					InCorrectAnswerSelected();
					TriggerEvent(MCQEvent.InCorrectButton);
				}

			}
		}

		CheckMultipleSelectionAnswers();

		if (!mcqScriptable.allowMultipleSelection)
		{
			if (answerIsCorrect)
				return;
		}
		else
		{
			if (AllSelectionsCorrect())
			{
				MCQFinishedMechanic();
				return;
			}
		}


		foreach (var item in multipleChoiceQuestionsSpawned)
		{
			if (item.isCorrectItem)
			{
				ColorBlock cb = item.questionButton.colors;
				cb.disabledColor = correctColor;
				item.questionButton.colors = cb;
			}
		}

		MCQFinishedMechanic();
	}

	public void SetMechanicInteractable(bool state)
	{
		if (!endOnIncorrectSelection)
			return;

		if (mcqScriptable.allowMultipleSelection)
			return;

		canvasGroup.interactable = canvasGroup.blocksRaycasts = state;

		if (state == false)
		{
			TriggerEvent(MCQEvent.FinishedMCQ);
			//SendMessageToSpawner("FinishedMCQ");
			MCQFinishedMechanic();
		}
	}

	public void CorrectAnswerSelected()
	{
		answerTelemetry.answerResult = answerTelemetry.answerSelected = true;
	}

	public void InCorrectAnswerSelected()
	{
		answerTelemetry.answerResult = false;
		answerTelemetry.answerSelected = true;
	}

	public void MCQFinishedMechanic()
	{
		Dictionary<int, bool> indexResults = new Dictionary<int, bool>();
		foreach(var item in multipleChoiceQuestionsSpawned)
		{
			if (!item.isSelectedQuestion)
			{
				continue;
			}
			int index = multipleChoiceQuestionsSpawned.IndexOf(item);
			bool result = item.isCorrectItem;
			indexResults.Add(index, result);
		}

		TriggerEvent(MCQEvent.OnSelectionResult, indexResults);
		timeElpasedTelemetry.checkTimeElapsed = true;

		StartCoroutine(TurnoffAfterXSeconds());
	}

	private IEnumerator TurnoffAfterXSeconds()
	{
		yield return new WaitForSeconds(timeUntilTurnedOfXSeconds);

		SetVisualState(false);

		StopMechanic();
	}
}