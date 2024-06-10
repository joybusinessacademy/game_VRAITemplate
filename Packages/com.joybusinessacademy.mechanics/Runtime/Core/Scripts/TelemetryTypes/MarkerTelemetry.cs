using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public bool answerResult = false;
	public bool finishedMarkerMechanic = false;
	public int attemeptsMade = 0;

	public bool IsValidated()
	{
		return finishedMarkerMechanic;
	}

	public void SendEvents()
	{
		List<TelemetryData> telemetryDatas = new List<TelemetryData>();

		TelemetryData answerData = new TelemetryData();
		answerData.mechanicID = id;
		answerData.id = "AnswerTelemetry";
		answerData.data.Add(string.Format("Answered {0}", answerResult ? "Correctly" : "Incorrectly"), answerResult.ToString());

		TelemetryData attemptData = new TelemetryData();
		attemptData.mechanicID = id;
		attemptData.id = "AttemptsTelemetry";
		attemptData.data.Add(string.Format("Attemepts Made"), attemeptsMade.ToString());

		telemetryDatas.Add(attemptData);
		telemetryDatas.Add(answerData);

		GlobalMessenger.Instance?.Broadcast("TelemetryDataList", telemetryDatas);

		isCompleted = true;
	}
}
