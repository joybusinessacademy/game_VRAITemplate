using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System;
using TMPro;
using UnityEngine;

public class TimerSystem : AbstractMechanicSystemBehivour<TimerData>, ITimerSystem
{
	public TextMeshProUGUI timerTextMesh;
	public CanvasGroup timerCanvasGroup;
	public GameObject timerVisual;
	private bool timerRunning = false;
	private float timerTime = 0;

	//Juice
	MechanicTweenSystem tweenSystem = new MechanicTweenSystem();

	private Coroutine tweeningCoroutine;
	private Coroutine fadingCoroutine;

	[Header("Animation Items")]
	public float speedOfTween = 0.2f;
	public float fadeObjectsTime = 0.2f;

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
		timerRunning = true;
	}

	private void OnMechanicStop()
	{
		timerRunning = false;
		timerTime = 0;
	}

	protected override void Update()
	{
		if (!timerRunning || mechanicData == null)
			return;


		if (mechanicData.countDownTime)
		{
			if (timerTime > 0)
			{
				timerTime -= Time.deltaTime;

				if (timerTime <= 0)
				{
					timerRunning = false;
					StopMechanic();
				}
			}
		}
		else
		{
			timerTime += Time.deltaTime;

			if (timerTime > mechanicData.amountOfTimeInSeconds)
			{
				timerRunning = false;
				StopMechanic();
			}
		}

		UpdateTimerText();

	}


	private void UpdateTimerText()
	{
		int hours = Mathf.FloorToInt(timerTime / 3600);
		int minutes = Mathf.FloorToInt((timerTime % 3600) / 60);
		int seconds = Mathf.FloorToInt(timerTime % 60);

		int hoursMax = Mathf.FloorToInt(mechanicData.amountOfTimeInSeconds / 3600);
		int minutesMax = Mathf.FloorToInt((mechanicData.amountOfTimeInSeconds % 3600) / 60);
		int secondsMax = Mathf.FloorToInt(mechanicData.amountOfTimeInSeconds % 60);

		// Format the time as "H:MM:SS"
		string timeText = "";

		if (mechanicData.countDownTime)
			timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
		else
			timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds) + " / " +
				string.Format("{0:D2}:{1:D2}:{2:D2}", hoursMax, minutesMax, secondsMax);


		timerTextMesh.text = "Timer: " + timeText;
	}


	public override void Reset()
	{
		base.Reset();

		timerRunning = false;
		timerTime = 0;
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		mechanicData = base.mechanicData as TimerData;

		if (mechanicData == null)
		{
			Debug.LogError("Missing Mechanic Data");
			return;
		}

		if (mechanicData.countDownTime)
			timerTime = mechanicData.amountOfTimeInSeconds;

		UpdateTimerText();
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
			timerVisual?.SetActive(state);

		tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(timerVisual.transform, speedOfTween, state, FinishedScaling));
		fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(timerCanvasGroup, state, fadeObjectsTime));
	}

	private void FinishedScaling(bool direction)
	{
		timerVisual?.SetActive(direction);
	}
}
