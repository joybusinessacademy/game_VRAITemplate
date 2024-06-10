using SkillsVR.Messeneger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	public AudioClip audioClipData = null;

	public bool IsValidated()
	{
		return audioClipData != null;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "Audio Telemetry";
		telemetryData.data.Add("Audio Clip Data: ", audioClipData);

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}
}