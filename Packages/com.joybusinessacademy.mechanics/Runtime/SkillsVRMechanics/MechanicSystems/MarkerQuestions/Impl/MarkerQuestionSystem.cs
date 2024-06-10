using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.UnityExtenstion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MarkerQuestionFeedback
{
	NOFEEDBACK,
	SELECTEDANSWERONLY,
	SELECTEDPLUSCORRECTIVE
}

public class MarkerQuestionSystem : AbstractMechanicSystemBehivour<MarkerQuestionsData>, IMarkerQuestionSystem
{
	

	public Marker markerPrefab;
	public GameObject continueButton;

	[Header("Colors")]
	public Color correctColor;
	public Color incorrectColor;

	[Header("Sounds")]
	public AudioSource audioSource;
	public AudioClip incorrectSound;
	public AudioClip correctSound;
	public AudioClip neutralSound;

	private List<Marker> markers = new List<Marker>();

	private GameObject spawnedContinueButton;

	public List<ITelemetry> telemetries = new List<ITelemetry>();

	AnswerTelemetry answerTelemetry = new AnswerTelemetry();
	TimeElpasedTelemetry timeElpasedTelemetry;
	TimedTelemetry timedTelemetry;
	MarkerTelemetry markerTelemetry = new MarkerTelemetry();

	MechanicTweenSystem tweenSystem = new MechanicTweenSystem();
	
	private Coroutine tweeningCoroutine;
	[Header("Animation Items")]
	public float speedOfTween = 0.3f;
	public float fadeObjectsTime = 0.3f;

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}

	protected override void Start()
	{
		base.Start();

		timedTelemetry = this.transform.GetOrAddComponent<TimedTelemetry>();
		timeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();

		telemetries.Add(timedTelemetry);
		telemetries.Add(timeElpasedTelemetry);
		telemetries.Add(answerTelemetry);
		telemetries.Add(markerTelemetry);

		string mechID = "Marker Question: " + this.gameObject.GetInstanceID().ToString();
		timedTelemetry.id = mechID;
		timeElpasedTelemetry.id = mechID;
		answerTelemetry.id = mechID;
		markerTelemetry.id = mechID;
	}

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
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		mechanicData = base.mechanicData as MarkerQuestionsData;

		if (mechanicData == null)
		{
			Debug.LogError("Missing Mechanic Data");
			return;
		}

		OnMechanicDataReady();
	}

	protected void OnMechanicDataReady()
	{
		GenerateAndSetMarkerPositions();

		if (!mechanicData.checkStraightAway)
		{
			GenerateLockInButton();
		}

		SetMarkersVisualStates(mechanicData.isVisualOnStart);
	}

	private void SetMarkersVisualStates(bool state)
	{
		foreach (var marker in markers)
		{
			if(marker.alreadySelectedOnce)
				marker.gameObject.SetActive(false);
			else
				marker.gameObject.SetActive(state);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearMarkers();
		ClearLockInButton();
	}

	public override void SetVisualState(bool state)
	{
		base.SetVisualState(state);

		if (tweeningCoroutine != null)
		{
			StopCoroutine(tweeningCoroutine);
			tweeningCoroutine = null;
		}

		if (state)
			SetMarkersVisualStates(state);	

		List<GameObject> scalingObjects = new List<GameObject>();
		markers.ForEach(y => scalingObjects.Add(y.gameObject));

		if (!this.gameObject.activeInHierarchy)
			return;

		//tweeningCoroutine = StartCoroutine(tweenSystem.ScaleObjects(scalingObjects, state, fadeObjectsTime));
	}

	private Dictionary<Marker, MarkerData> markerVsData = new Dictionary<Marker, MarkerData>();

	private void GenerateAndSetMarkerPositions()
	{
		markerVsData.Clear();

		foreach (MarkerData item in mechanicData.markerDatas)
		{
			Marker markerObject = Instantiate(markerPrefab, this.transform);

			markerVsData.Add(markerObject, item);

			if (item.spawnPoint != null)
			{
				markerObject.transform.position = item.spawnPoint.transform.position;
				markerObject.transform.rotation = item.spawnPoint.transform.rotation;
				markerObject.transform.localScale = item.spawnPoint.transform.localScale;
			}

			markerObject.isCorrectMarker = item.isCorrectMarker;
			markerObject.markerSelected += OnMarkerSelected;

			if (item.changeMarkerSprite != null && item.useCustomSprite)
			{
				markerObject.markerImage.sprite = item.changeMarkerSprite;
				markerObject.SetNormalColor(Color.white);
			}

			if (item.customTextForMarker != string.Empty)
				markerObject.SetText(item.customTextForMarker);

			markers.Add(markerObject);

			if (mechanicData.hideMarkerOnReset)
			{
				if (!item.markerIsVisible)
				{
					markerObject.alreadySelectedOnce = true;
					markerObject.gameObject.SetActive(false);
				}
			}
		}
	}

	private void ClearMarkers()
	{
		if (null == markers)
		{
			return;
		}
		foreach (Marker marker in markers)
		{
			if (null != marker && null != marker.gameObject)
			{
				Destroy(marker.gameObject);
			}
		}
		markers.Clear();
	}

	private void ClearLockInButton()
	{
		if (null != spawnedContinueButton && null != spawnedContinueButton.gameObject)
		{
			Destroy(spawnedContinueButton.gameObject);
		}
	}
	private void GenerateLockInButton()
	{
		
		spawnedContinueButton = Instantiate(continueButton, transform);
		if (mechanicData.lockInButtonLocation != null)
		{
			spawnedContinueButton.transform.position = mechanicData.lockInButtonLocation.position;
			spawnedContinueButton.transform.rotation = mechanicData.lockInButtonLocation.rotation;
		}
		spawnedContinueButton.GetComponent<Button>().onClick.RemoveListener(CheckMarkerQuestions);
		spawnedContinueButton.GetComponent<Button>().onClick.AddListener(CheckMarkerQuestions);
		spawnedContinueButton.GetComponent<Button>().interactable = false;
	}

	private void FeedbackType()
	{
		switch (mechanicData.markerQuestionFeedback)
		{
			case MarkerQuestionFeedback.NOFEEDBACK:
				ShowNeutralFeedback();
				break;
			case MarkerQuestionFeedback.SELECTEDANSWERONLY:
				ShowSelectedAnswerOnly();
				break;
			case MarkerQuestionFeedback.SELECTEDPLUSCORRECTIVE:
				ShowSelectedPlusCorrective();
				break;
			default:
				break;
		}
	}

	private void ShowNeutralFeedback()
	{
		bool allCorrect = true;

		// if we have marked a correct item therefore it needs correct / incorrect event
		bool isAnyMarkedAsCorrect = markers.Find(i => i.isCorrectMarker);

		if (isAnyMarkedAsCorrect)
		{
			foreach (Marker marker in markers)
			{
				if (marker.isCurrentlyToggeled)
				{
					if (!marker.isCorrectMarker)
					{
						allCorrect = false;
					}

					markerTelemetry.answerResult = marker.isCorrectMarker;
				}
				else
				{
					if (marker.isCorrectMarker)
					{
						allCorrect = false;
					}
				}
			}

			TriggerEvent(allCorrect ? MarkerEvent.MarkerFinishedCorrect : MarkerEvent.MarkerFinishedIncorrect);
		}
		else
		{

			foreach (var marker in markers)
			{
				marker.markerButton.interactable = false;
			}

			TriggerEvent(MarkerEvent.MarkerFinished);
		}

		Marker selected = markers.Find(choice => choice.isCurrentlyToggeled);

		if (mechanicData.hideMarkerOnReset)
		{
			if(selected != null && markerVsData.ContainsKey(selected))
			{
				markerVsData[selected].markerIsVisible = false;
				selected.alreadySelectedOnce = true;
			}
		}

		TriggerEvent(MarkerEvent.OnChoiceSelected, markers.FindIndex(choice => choice.isCurrentlyToggeled));

		//Mechanic Is Finished
		StartCoroutine(DelayedFinished());
	}

	private void ShowSelectedAnswer()
	{
		bool allCorrect = true;

		foreach (Marker marker in markers)
		{
			if (marker.isCurrentlyToggeled)
			{
				ColorBlock cb = marker.markerButton.colors;
				cb.disabledColor = marker.isCorrectMarker ? correctColor : incorrectColor;
				marker.markerButton.colors = cb;

				if (!marker.isCorrectMarker)
				{
					allCorrect = false;
				}

				markerTelemetry.answerResult = marker.isCorrectMarker;
			}
			else
			{
				if (marker.isCorrectMarker)
				{
					allCorrect = false;
				}
			}
		}

		TriggerEvent(allCorrect ? MarkerEvent.MarkerFinishedCorrect : MarkerEvent.MarkerFinishedIncorrect);

		TriggerEvent(MarkerEvent.OnChoiceSelected, markers.FindIndex(choice => choice.isCurrentlyToggeled));
	}

	private void ShowSelectedAnswerOnly()
	{
		ShowSelectedAnswer();

		DisableAllMarkers();

		//Mechanic Is Finished
		
		StartCoroutine(DelayedFinished());
	}

	private void DisableAllMarkers()
	{
		markers.ForEach(x => x.markerButton.interactable = false);
	}

	private void ShowSelectedPlusCorrective()
	{
		bool allCorrect = true;

		foreach (var marker in markers)
		{
			ColorBlock cbl = marker.markerButton.colors;
			cbl.disabledColor = marker.isCorrectMarker ? correctColor : incorrectColor;
			marker.markerButton.colors = cbl;

			if(marker.isCurrentlyToggeled && !marker.isCorrectMarker)
				allCorrect = false;
		}

		TriggerEvent(allCorrect ? MarkerEvent.MarkerFinishedCorrect : MarkerEvent.MarkerFinishedIncorrect);

		DisableAllMarkers();

		TriggerEvent(MarkerEvent.OnChoiceSelected, markers.FindIndex(choice => choice.isCurrentlyToggeled));

		//Mechanic Is Finished
		StartCoroutine(DelayedFinished());
	}

	private void OnMarkerSelected(Marker markerSelected)
	{
		markerTelemetry.attemeptsMade++;

		if (mechanicData.checkStraightAway)
		{
			foreach (Marker marker in markers)
			{
				marker.SetMechanicDone();
			}
			
			FeedbackType();

			return;
		}

		if (mechanicData.untoggleOthers)
		{
			foreach (Marker marker in markers)
			{
				if (marker != markerSelected && marker.isCurrentlyToggeled)
					marker.SetToggledState(false);
			}
		}

		if (!mechanicData.checkStraightAway && spawnedContinueButton != null)
			spawnedContinueButton.GetComponent<Button>().interactable = IsAMarkerSelected();
	}

	private bool IsAMarkerSelected()
	{
		bool isAMarkerSelected = false;

		foreach (Marker marker in markers.Where(marker => marker.isCurrentlyToggeled))
		{
			isAMarkerSelected = true;
		}

		return isAMarkerSelected;
	}

	private IEnumerator DelayedFinished()
	{
		yield return new WaitForSeconds(1);

		FinishedMechanic();
	}

	public void CheckMarkerQuestions()
	{
		foreach (Marker marker in markers)
		{
			marker.SetMechanicDone();
		}
		
		FeedbackType();

		//Mechanic Is Finished
		StartCoroutine(DelayedFinished());
	}

	public void FinishedMechanic()
	{

		if (!mechanicData.checkStraightAway && spawnedContinueButton != null)
		{
			spawnedContinueButton.SetActive(false);
		}

		markerTelemetry.finishedMarkerMechanic = true;

		SetMarkersVisualStates(false);

		StopMechanic();
	}
}
