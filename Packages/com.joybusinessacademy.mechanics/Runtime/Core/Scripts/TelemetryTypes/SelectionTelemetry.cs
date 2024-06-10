using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public string itemSelectedData = "";

	public bool IsValidated()
	{
		return itemSelectedData != string.Empty;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "SelectionTelemetry";
		telemetryData.data.Add("Item Select: ", itemSelectedData);

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}
}
