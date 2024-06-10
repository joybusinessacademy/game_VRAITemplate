using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCQTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public bool finishedMCQ = false;
	private string mcqData = "";

	public bool IsValidated()
	{
		return finishedMCQ;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "MCQTelemetryData";
		telemetryData.data.Add("MCQ Telemetry Data", mcqData);

		GlobalMessenger.Instance?.Broadcast("MCQTelemetry", telemetryData);

		isCompleted = true;
	}
}
