using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public bool answerResult = false;
	public bool answerSelected = false;

	public bool IsValidated()
	{
		return answerSelected;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "AnswerTelemetry";
		telemetryData.data.Add(string.Format("Answered {0}", answerResult ? "Correctly" : "Incorrectly"), "Bool State:" + answerResult.ToString());

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}
}
