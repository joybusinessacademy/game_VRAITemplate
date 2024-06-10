using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public GameObject parentMechanic = null;

	public bool IsValidated()
	{
		return parentMechanic != null;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "LinkedTelemetry";
		telemetryData.data.Add(string.Format("Telemetry Is Linked To {0}", parentMechanic.name), parentMechanic.name);

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}
}