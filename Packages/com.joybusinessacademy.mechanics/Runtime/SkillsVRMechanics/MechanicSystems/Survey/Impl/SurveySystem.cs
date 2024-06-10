using DialogExporter;
using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.UnityExtenstion;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurveySystem : AbstractMechanicSystemBehivour<SurveyData>, ISurveySystem
{
	public GameObject mechanicVisual;

	public Slider slider;
	public AudioSource surveyAudioSource;
	public TextMeshProUGUI learnMoreHelperText;
	private string defaultLearnMoreText = "Select to learn more";
	private string selectedLearnMoreText = "Select again to return";

	private string preFix = "";
	private string postFix = " %";

	public Button learnMoreButton;

	public TextMeshProUGUI statementText;
	public TextMeshProUGUI sliderText;
	public TextMeshProUGUI instructionText;

	private List<SurveyLikertData> likertChoices = new List<SurveyLikertData>();

	private int currentStatement = 0;
	private int defaultSliderValue = -1;
	private bool firstTimeOnStatement = false;

	//Pagenation
	public GameObject paginationMarker;
	public Transform paginationParent;
	private bool alreadyGeneratedPagination = false;
	private Color whitePagination = Color.white;
	private Color grayPagination = Color.gray;
	private List<GameObject> paginationMarkersGenerated = new List<GameObject>();

	public Button nextButton;
	public Button backButton;

	//Finished
	public Button completeButton;

	//Slider Data
	private Dictionary<int, int> currentStatemenToSliderValue = new Dictionary<int, int>();

	//Data
	private Dictionary<string, string> statementToLikert = new Dictionary<string, string>();

	//Animation
	[Header("Animation Items")]
	public float speedOfTween = 0.3f;
	public float fadeObjectsTime = 0.3f;

	private MechanicTweenSystem tweenSystem = new MechanicTweenSystem();
	private Coroutine tweeningCoroutine;
	private Coroutine fadingCoroutine;

	private List<CanvasGroup> scalingObjects = new List<CanvasGroup>();

	//Telemetry
	private TimedTelemetry timedTelemetry;

	public List<ITelemetry> telemetries = new List<ITelemetry>();

	TimeElpasedTelemetry timeElpasedTelemetry;
	SurveyTelemetry surveyTelemetry = new SurveyTelemetry();

	//Just for Telemetry
	private Dictionary<string, bool> statementToClickedLearnMore = new Dictionary<string, bool>();

	private void SetUpTelemetry()
	{
		timedTelemetry = this.transform.GetOrAddComponent<TimedTelemetry>();
		timeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();

		telemetries.Add(timedTelemetry);
		telemetries.Add(timeElpasedTelemetry);
		telemetries.Add(surveyTelemetry);

		string mechID = "Slider Qyestion: " + this.gameObject.GetInstanceID().ToString();
		timedTelemetry.id = mechID;
		timeElpasedTelemetry.id = mechID;
		surveyTelemetry.id = mechID;
	}

	private void SendTelemetryData()
	{
		surveyTelemetry.statementToLearnMore = statementToClickedLearnMore;
		surveyTelemetry.statementToLikert = statementToLikert;

		surveyTelemetry.finishedSliderMechanic = true;
	}

	private void SetSliderText()
	{
		int sliderValue = -1;

		if (firstTimeOnStatement)
		{
			sliderValue = defaultSliderValue;
			firstTimeOnStatement = false;
		}
		else
			sliderValue = currentStatemenToSliderValue[currentStatement];

		if (sliderValue >= likertChoices.Count || sliderValue == -1)
		{
			Debug.LogWarning("Choices out of range for Likert");
			return;
		}

		slider.SetValueWithoutNotify(sliderValue);
		sliderText.text = likertChoices[sliderValue].likertString;
	}

	private void UpdateSliderText()
	{
		int sliderValue = (int)slider.value;

		if (sliderValue >= likertChoices.Count || sliderValue == -1)
		{
			Debug.LogWarning("Choices out of range for Likert");
			return;
		}

		sliderText.text = likertChoices[sliderValue].likertString;
	}

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
		switch (systemEvent.eventKey)
		{
			case MechSysEvent.OnStart: OnMechanicStart(); break;
			case MechSysEvent.OnStop: OnMechanicStop(); break;
		}
	}

	private void OnMechanicStart()
	{
		SetUpTelemetry();

		AddButtonListeners();
	}

	private void OnMechanicStop()
	{
		SendTelemetryData();

		RemoveButtonListeners();
	}

	public override void Reset()
	{
		base.Reset();

		currentStatement = 0;
		statementToLikert.Clear();
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		mechanicData = base.mechanicData as SurveyData;

		if (mechanicData == null)
		{
			Debug.LogError("Missing Mechanic Data");
			return;
		}

		//Reset Data
		currentStatement = 0;
		statementToLikert.Clear();
		currentStatemenToSliderValue.Clear();
		statementToClickedLearnMore.Clear();

		SetPrePostFix();

		SetSliderType(mechanicData.surveyScaleType, (mechanicData.likertLabels.Count - 1));

		likertChoices = mechanicData.likertLabels;

		//Pagination Markers
		if (!alreadyGeneratedPagination)
			GeneratePaginationMarkers();

		//Multiple Times
		UpdateSurveyItems();

		//One Offs
		SetCompleteButtonText();
	}

	private void SetPrePostFix()
	{
		preFix = mechanicData.prefixLabel;
		postFix = mechanicData.postFixLabel;
	}

	private void SetCompleteButtonText()
	{
		completeButton.GetComponentInChildren<TextMeshProUGUI>().text = mechanicData.finishedButtonLabel;
	}

	private void AddButtonListeners()
	{
		LearnMoreButtonSetup();
		backButton.onClick.AddListener(OnBackButtonClicked);
		nextButton.onClick.AddListener(OnNextButtonClicked);
		completeButton.onClick.AddListener(OnCompleteButtonClicked);
	}

	private void LearnMoreButtonSetup()
	{
		if (!string.IsNullOrEmpty(mechanicData.statementPrompts[currentStatement].learnMoreString))
		{
			learnMoreButton.onClick.AddListener(OnLearnMoreButtonClicked);
			learnMoreButton.interactable = true;
		}
		else
			learnMoreButton.interactable = false;
	}

	private void OnLearnMoreButtonClicked()
	{
		if (learnMoreHelperText.text == defaultLearnMoreText)
		{
			statementText.text = mechanicData.statementPrompts[currentStatement].learnMoreString;
			learnMoreHelperText.text = selectedLearnMoreText;

			if (mechanicData.statementPrompts[currentStatement].learnMoreAudio != null)
				surveyAudioSource.PlayOneShot(mechanicData.statementPrompts[currentStatement].learnMoreAudio);
		}
		else
		{
			statementText.text = mechanicData.statementPrompts[currentStatement].statementString;
			learnMoreHelperText.text = defaultLearnMoreText;
		}

		UpdateLearnMoreData();
	}

	private void UpdateLearnMoreData()
	{
		string statementKey = mechanicData.statementPrompts[currentStatement].statementString;
		if (statementToClickedLearnMore.ContainsKey(statementKey))
			statementToClickedLearnMore[statementKey] = true;
	}

	private void OnCompleteButtonClicked()
	{
		currentStatemenToSliderValue[currentStatement] = (int)slider.value;
		SetLikertDataStatement();

		ParseOutLikertToStatements();

		StopMechanic();
	}

	private void CheckStatementAndSlider(bool isPostCheck = false)
	{
		if (!currentStatemenToSliderValue.ContainsKey(currentStatement))
		{
			currentStatemenToSliderValue.Add(currentStatement, (int)slider.value);
			firstTimeOnStatement = true;
		}
		else
		{
			if (isPostCheck)
				return;

			currentStatemenToSliderValue[currentStatement] = (int)slider.value;
		}
	}

	private void SetLearnMoreDataCheck()
	{
		if (!statementToClickedLearnMore.ContainsKey(statementText.text))
			statementToClickedLearnMore.Add(statementText.text, false);
		
	}

	private void SetLikertDataStatement()
	{
		if (!statementToLikert.ContainsKey(statementText.text))
			statementToLikert.Add(statementText.text, sliderText.text);
		else
			statementToLikert[statementText.text] = sliderText.text;
	}

	private void OnNextButtonClicked()
	{
		//Adding Data Based on User Choice
		SetLikertDataStatement();
		SetLearnMoreDataCheck();

		//Updating Current
		CheckStatementAndSlider();

		currentStatement++;

		//Checking for Next
		CheckStatementAndSlider(true);
		

		if (currentStatement >= mechanicData.statementPrompts.Count)
			currentStatement = (mechanicData.statementPrompts.Count - 1);

		UpdateSurveyItems();
	}

	private void OnBackButtonClicked()
	{
		//Only do this on last page
		if (currentStatement == (mechanicData.statementPrompts.Count - 1))
			currentStatemenToSliderValue[currentStatement] = (int)slider.value;

		currentStatement--;

		if (currentStatement < 0)
			currentStatement = 0;

		UpdateSurveyItems();
	}

	private void RemoveButtonListeners()
	{
		backButton.onClick.RemoveListener(OnBackButtonClicked);
		nextButton.onClick.RemoveListener(OnNextButtonClicked);
		completeButton.onClick.RemoveListener(OnCompleteButtonClicked);
	}

	private void UpdateSurveyItems()
	{
		if (mechanicData.surveyScaleType == SurveyScaleType.LIKERT)
			SetSliderText();
		else
			SetSliderNumber();

		UpdateInstructionText();
		UpdateStatementText();
		UpdatePaginationButtons();
		UpdatePaginationVisuals();
		SetLearnMoreDataCheck();
	}

	private void GeneratePaginationMarkers()
	{
		if (mechanicData.statementPrompts.Count > 0)
		{
			for (int i = 0; i < mechanicData.statementPrompts.Count; i++)
			{
				GameObject pagination = Instantiate(paginationMarker, paginationParent);
				paginationMarkersGenerated.Add(pagination);
			}
		}
	}

	private void UpdateInstructionText()
	{
		string instructionTextConverted = mechanicData.instructionText;

		if (!string.IsNullOrEmpty(instructionTextConverted))
		{
			instructionText.text = instructionTextConverted;
			instructionText.gameObject.SetActive(true);
		}
		else
		{
			instructionText.text = "";
			instructionText.gameObject.SetActive(false);
		}
	}

	private void UpdateStatementText()
	{
		statementText.text = mechanicData.statementPrompts[currentStatement].statementString;
	}

	private void SetSliderType(SurveyScaleType surveyScaleType, int numberOfLikerts)
	{
		if (surveyScaleType == SurveyScaleType.LIKERT)
		{
			slider.maxValue = numberOfLikerts;
			defaultSliderValue = (numberOfLikerts / 2);
			slider.value = defaultSliderValue;

			CheckStatementAndSlider();

			slider.onValueChanged.AddListener(delegate { UpdateSliderText(); });
		}
		else if (surveyScaleType == SurveyScaleType.PERCENTAGE)
		{
			slider.maxValue = 100;
			defaultSliderValue = 50;
			slider.value = defaultSliderValue;

			CheckStatementAndSlider();

			slider.onValueChanged.AddListener(delegate { UpdateSliderNumber(); });
		}
		else
			Debug.LogWarning("Missing Format for Slider Type");
	}

	private void SetSliderNumber()
	{
		int sliderValue = -1;

		if (firstTimeOnStatement)
		{
			sliderValue = defaultSliderValue;
			firstTimeOnStatement = false;
		}
		else
			sliderValue = currentStatemenToSliderValue[currentStatement];

		slider.SetValueWithoutNotify(sliderValue);
		sliderText.text = preFix + slider.value.ToString() + postFix;
	}

	private void UpdateSliderNumber()
	{
		sliderText.text = preFix + slider.value.ToString() + postFix;
	}

	private void UpdatePaginationButtons()
	{
		//Only 1 Page
		if (currentStatement == 0 && mechanicData.statementPrompts.Count <= 1)
		{
			backButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(false);
			completeButton.gameObject.SetActive(true);
			return;
		}

		//More Than One Page - But current on first page
		if (currentStatement == 0 && mechanicData.statementPrompts.Count > 1)
		{
			backButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(true);
			completeButton.gameObject.SetActive(false);
		}

		//Not on First Page
		//Not On Last
		if (currentStatement != 0 && currentStatement != (mechanicData.statementPrompts.Count - 1))
		{
			backButton.gameObject.SetActive(true);
			nextButton.gameObject.SetActive(true);
		}

		//On Last Page
		if (currentStatement == (mechanicData.statementPrompts.Count - 1))
		{
			nextButton.gameObject.SetActive(false);
			completeButton.gameObject.SetActive(true);
		}
		else
			completeButton.gameObject.SetActive(false);
	}

	private void UpdatePaginationVisuals()
	{
		for (int i = 0; i < paginationMarkersGenerated.Count; i++)
		{
			if(currentStatement >= i)
				paginationMarkersGenerated[i].GetComponent<Image>().color = whitePagination;
			else
				paginationMarkersGenerated[i].GetComponent<Image>().color = grayPagination;
		}
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
			scalingObjects.ForEach(y => y.alpha = 0);
			mechanicVisual?.SetActive(state);
		}

		if (!this.gameObject.activeInHierarchy)
			return;

		tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(mechanicVisual.transform, speedOfTween, state));
		fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(mechanicVisual.GetOrAddComponent<CanvasGroup>(), state, fadeObjectsTime));
	}

	private void ParseOutLikertToStatements()
	{
		//Do Something with the Data
		//statementToLikert

#if NODE_DEVELOPMENT
		foreach (var item in statementToLikert)
		{
			Debug.Log("Statement: " + item.Key + " : " + "Choice: " + item.Value);
		}
#endif
	}
}

