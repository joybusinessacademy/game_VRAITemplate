using Newtonsoft.Json;
using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveyTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public bool answerResult = false;
	public bool finishedSliderMechanic = false;
	public int attemeptsMade = 0;

	public Dictionary<string, string> statementToLikert = new Dictionary<string, string>();
	public Dictionary<string, bool> statementToLearnMore = new Dictionary<string, bool>();

	public bool IsValidated()
	{
		return finishedSliderMechanic;
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

		TelemetryData surveyData = new TelemetryData();
		attemptData.mechanicID = id;
		attemptData.id = "SliderTelemetry";
		string json = JsonConvert.SerializeObject(statementToLikert, Formatting.Indented);
		attemptData.data.Add("Slider Answer",json);

		TelemetryData learnMoreData = new TelemetryData();
		attemptData.mechanicID = id;
		attemptData.id = "SliderTelemetry - Learn More";
		string jsonLearnMore = JsonConvert.SerializeObject(statementToLearnMore, Formatting.Indented);
		attemptData.data.Add("Learn More Clicked", jsonLearnMore);

		telemetryDatas.Add(surveyData);
		telemetryDatas.Add(learnMoreData);
		telemetryDatas.Add(answerData);
		telemetryDatas.Add(attemptData);

		GlobalMessenger.Instance?.Broadcast("TelemetryDataList", telemetryDatas);

		isCompleted = true;
	}
}
