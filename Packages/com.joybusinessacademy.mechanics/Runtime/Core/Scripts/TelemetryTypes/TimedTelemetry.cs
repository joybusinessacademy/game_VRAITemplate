using SkillsVR.Messeneger;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedTelemetry : MonoBehaviour , ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public float timeSinceSpawned;
	public float timeToValidateAgainst = 10f;

	public TimedTelemetry() { }

	public TimedTelemetry(float timeToValidateAgainst)
	{
		this.timeToValidateAgainst = timeToValidateAgainst;
	}

	public bool IsValidated()
	{
		return timeSinceSpawned > timeToValidateAgainst;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "TimedTelemetry";
		telemetryData.data.Add("Time Since Spawned", timeSinceSpawned.ToString());

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}

	private void Update()
	{
		if (!isCompleted)
			timeSinceSpawned += Time.deltaTime;
	}
}
