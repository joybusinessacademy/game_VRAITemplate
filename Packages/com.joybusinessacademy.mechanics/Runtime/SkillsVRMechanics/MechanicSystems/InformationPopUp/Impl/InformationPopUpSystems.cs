using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using SkillsVR.UnityExtenstion;

public class InformationPopUpSystems : AbstractMechanicSystemBehivour<InfoPopUpDatas>, IInformationPopUpSystem
{
	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}

	[Header("Info PopUp References")]
	public GameObject visualGameobject;
	public List<TextMeshProUGUI> popupInformationText = new List<TextMeshProUGUI>();
	public Button nextButton;

	public List<InfoPopUpVisual> infoPopUpVisuals = new List<InfoPopUpVisual>();

	public Material lineRendererMaterial;

	//Multimedia Information
	[Header("Info PopUp Multi Media References")]
	public List<TextMeshProUGUI> popupMultiMediaInformationText = new List<TextMeshProUGUI>();
	public Image multimediaImage;

	internal bool isTimedMechanic = false;
	internal bool mechanicFinished = false;
	internal float timeUntilTurnedOff = 0;

	public MechanicLineLinker lineLinker;
	public Transform linkPoint;
	internal MechanicLineLinker spawnedLineLinker;

	[Header("Marker System")]
	public GameObject marker;

	public List<ITelemetry> telemetries = new List<ITelemetry>();

	SelectionTelemetry selectionTelemetry = new SelectionTelemetry();
	TimeElpasedTelemetry timeElpasedTelemetry;
	TimedTelemetry timedTelemetry;

	MechanicTweenSystem tweenSystem = new MechanicTweenSystem();

	private Coroutine tweeningCoroutine;
	private Coroutine fadingCoroutine;

	[Header("Animation Items")]
	public float speedOfTween = 0.3f;
	public float fadeObjectsTime = 0.3f;

	public List<GameObject> soundBiteObjects = new List<GameObject>();

	protected override void Start()
	{
		base.Start();

		timedTelemetry = this.transform.GetOrAddComponent<TimedTelemetry>();
		timeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();

		telemetries.Add(timedTelemetry);
		telemetries.Add(timeElpasedTelemetry);
		telemetries.Add(selectionTelemetry);

		string mechID = "Information Slide: " + this.gameObject.GetInstanceID().ToString();
		timedTelemetry.id = mechID;
		timeElpasedTelemetry.id = mechID;
		selectionTelemetry.id = mechID;
	}

	public override void Reset()
	{
		base.Reset();
		mechanicFinished = false;
		foreach (var telemetry in telemetries)
		{
			if (null == telemetry)
			{
				continue;
			}
			telemetry.isCompleted = false;
		}
	}

	public void SetInfoPopUpVisualState(int stateToSet)
	{
		infoPopUpVisuals.ForEach(x => x.visual.SetActive(false));
		foreach (var item in infoPopUpVisuals)
		{
			if (item.ID == stateToSet)
				item.visual.SetActive(true);
		}
	}

	//protected override object OnReceiveMechanicSystemMessage(string message, object[] args)
	//{
	//	base.OnReceiveMechanicSystemMessage(message, args);
	//	switch (message)
	//	{
	//		case nameof(SetVisualState): SetVisualState((bool)args[0]); return null;
	//		case nameof(SetCustomTime): SetCustomTime((float)args[0]); return null;
	//		default:
	//			ThrowMessageNotMatchException(message, args);
	//			break;
	//	}
	//	return null;
	//}

	private bool currentVisualState = false;

	public override void SetVisualState(bool state)
	{
		base.SetVisualState(state);

		if (currentVisualState == state)
			return;
		else
			currentVisualState = state;

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

		if (state && !mechanicData.useMarker)
			visualGameobject?.SetActive(state);

		if (this.gameObject.activeInHierarchy)
		{
			tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(visualGameobject.transform, speedOfTween, state));
			fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(visualGameobject.GetOrAddComponent<CanvasGroup>(), state, fadeObjectsTime, OnFinishedFadingCanvas));
		}

	}

	public void PlaySoundBiteClip()
	{
		if(mechanicData.soundBiteClip != null)
			this.transform.GetComponent<AudioSource>().PlayOneShot(mechanicData.soundBiteClip);		
	}

	private void OnFinishedFadingCanvas(bool direction)
	{
		CanvasGroup canvasGroup = visualGameobject.GetComponent<CanvasGroup>();
		canvasGroup.interactable = canvasGroup.blocksRaycasts = direction;

		LayoutRebuilder.ForceRebuildLayoutImmediate(visualGameobject.transform as RectTransform);
	}

	public void OnNextButton()
	{
		TriggerEvent(InformationPopUpEvent.OnButtonClick);
		FinishedPopUp();
	}

	protected override void Update()
	{
		base.Update();

		//TIMED MECHANIC LOGIC
		if (!mechanicFinished && isTimedMechanic)
		{
			timeUntilTurnedOff -= Time.deltaTime;

			if (timeUntilTurnedOff <= 0)
			{
				TriggerEvent(InformationPopUpEvent.OnTimeOut);
				FinishedPopUp();
			}
				
		}

		foreach (var telemetry in telemetries)
		{
			if (!telemetry.isCompleted && telemetry.IsValidated())
				telemetry.SendEvents();
		}
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		mechanicData = base.mechanicData as InfoPopUpDatas;

		if (mechanicData == null)
		{
			Debug.LogError("Missing Mechanic Data");
			return;
		}

		//Marker System
		marker.SetActive(mechanicData.useMarker);

		if (mechanicData.useMarker)
		{
			marker.GetComponent<Button>()?.onClick.RemoveListener(OnMarkerButtonClicked);
			marker.GetComponent<Button>()?.onClick.AddListener(OnMarkerButtonClicked);
		}
			
		//VISUAL STATE
		visualGameobject.SetActive(mechanicData.isVisualOnSpawn);

		//TEXT FROM DATA
		popupInformationText.ForEach(x => x.text = mechanicData.information);

		//NEXT BUTTON STATE
		if (mechanicData.hasNextButton)
		{
			nextButton.gameObject.SetActive(true);
			nextButton.onClick.RemoveListener(OnNextButton);
			nextButton.onClick.AddListener(OnNextButton);
			SetButtonText(mechanicData.buttonText);
		}
		else //IS TIMED MECHANIC
		{
			nextButton.gameObject.SetActive(false);
			isTimedMechanic = true;
			timeUntilTurnedOff = mechanicData.timeUntilDisappear;
		}

		selectionTelemetry.itemSelectedData = mechanicData.information;

		//Set Multimedia Information
		//MULTI MEDIA DATA
		bool hasMultimediaText = false;
		bool hasCustomSprite = false;

		if (mechanicData.multiMediaInformation != string.Empty && 
  			// filtered strings
  			mechanicData.multiMediaInformation != "New dialog" && 
     			mechanicData.multiMediaInformation != "MultiMedia Information")
		{
			hasMultimediaText = true;
			popupMultiMediaInformationText.ForEach(y => y.text = mechanicData.multiMediaInformation);
		}
		if(mechanicData.imageToShow != null && mechanicData.showCustomImage)
		{
			hasCustomSprite = true;
			multimediaImage.sprite = mechanicData.imageToShow;
		}

		if (hasMultimediaText && hasCustomSprite)
		{
			SetInfoPopUpVisualState(2);
		}
		else if (hasMultimediaText)
		{
			SetInfoPopUpVisualState(1);
		}

		if(mechanicData.soundBiteClip != null)
		{
			soundBiteObjects.ForEach(x =>
			{
				x.SetActive(true);
				x.GetComponent<Button>().onClick.RemoveAllListeners();
				x.GetComponent<Button>().onClick.AddListener(PlaySoundBiteClip);
			});
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(visualGameobject.transform as RectTransform);
	}

	private void OnMarkerButtonClicked()
	{
		TriggerEvent(InformationPopUpEvent.OnMarkerButtonClick);
		mechanicData.useMarker = false;
		visualGameobject?.SetActive(true);

		if (this.gameObject.activeInHierarchy)
		{
			tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(visualGameobject.transform, speedOfTween, true));
			fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(visualGameobject.GetOrAddComponent<CanvasGroup>(), true, fadeObjectsTime, OnFinishedFadingCanvas));
		}
	}

	public void SetCustomTime(float time)
	{
		timeUntilTurnedOff = time;
		TriggerEvent(InformationPopUpEvent.OnSetCustomTime, time);
	}

	private void FinishedPopUp()
	{
		mechanicFinished = true;
		SetVisualState(false);

		if (spawnedLineLinker != null)
			spawnedLineLinker.gameObject.SetActive(false);

		TriggerEvent(InformationPopUpEvent.OnPopupFinished);
		StopMechanic();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (spawnedLineLinker?.gameObject != null)
		{
#if UNITY_EDITOR
			DestroyImmediate(spawnedLineLinker.gameObject);
#else
			Destroy(spawnedLineLinker.gameObject);
#endif
		}

		marker?.GetComponent<Button>()?.onClick.RemoveAllListeners();
		nextButton?.onClick.RemoveAllListeners();
	}

	public void SetButtonText(string textToSet)
	{
		nextButton.GetComponentInChildren<TextMeshProUGUI>().text = textToSet;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

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
	}
}

public enum InfoPopUpStyle
{
	TEXTBASEDSLIDE,
	MULTIMEDIASLIDE
}

[Serializable]
public class InfoPopUpVisual
{
	public int ID;
	public GameObject visual;
}
