using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankOrderTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	internal Dictionary<int, List<RankOrderItemData>> slottedItems = new Dictionary<int, List<RankOrderItemData>>();
	public bool answerResult = false;
	public bool finishedRankorderMechanic = false;
	public int attemeptsMade = 0;

	public bool IsValidated()
	{
		return finishedRankorderMechanic;
	}

	public void SendEvents()
	{
		List<TelemetryData> telemetryDatas = new List<TelemetryData>();

		TelemetryData answerData = new TelemetryData();
		answerData.mechanicID = id;
		answerData.id = "AnswerTelemetry";
		answerData.data.Add(string.Format("Answered {0}", answerResult ? "Correctly" : "Incorrectly"), answerResult.ToString());

		TelemetryData slottedData = new TelemetryData();
		slottedData.mechanicID = id;
		slottedData.id = "SlottedRankOrderItems";
		string slottedItemData = JsonUtility.ToJson(slottedItems);
		slottedData.data.Add("Slotted Item Data",slottedItemData);

		TelemetryData attemptData = new TelemetryData();
		attemptData.mechanicID = id;
		attemptData.id = "AttemptsTelemetry";
		attemptData.data.Add(string.Format("Attemepts Made"), attemeptsMade.ToString());

		telemetryDatas.Add(answerData);
		telemetryDatas.Add(slottedData);
		telemetryDatas.Add(attemptData);

		GlobalMessenger.Instance?.Broadcast("TelemetryDataList", telemetryDatas);

		isCompleted = true;
	}
}
