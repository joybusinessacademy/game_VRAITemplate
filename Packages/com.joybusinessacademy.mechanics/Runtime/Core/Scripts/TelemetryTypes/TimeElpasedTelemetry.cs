using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeElpasedTelemetry : MonoBehaviour , ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public float timeElapsed;
	public bool startCheckingTime = false;
	public bool checkTimeElapsed = false;

	public bool IsValidated()
	{
		return checkTimeElapsed;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "TimeElpasedTelemetry";
		telemetryData.data.Add("Time Elapsed Mechanic Showen: ", timeElapsed.ToString());

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}

	private void Update()
	{
		if (!isCompleted && startCheckingTime)
			timeElapsed += Time.deltaTime;
	}
}
