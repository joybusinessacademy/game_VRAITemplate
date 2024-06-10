using SkillsVR.Messeneger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelemetryController : MonoBehaviour
{
	internal List<TelemetryData> telemetryDatas = new List<TelemetryData>();

	public delegate void TelemetryAdded(TelemetryData telemetryData);
	public TelemetryAdded telemetryAdded;

	private void Awake()
	{
		GlobalMessenger.Instance?.AddListener<TelemetryData>("TelemetryData", OnTelemetryDataReceieved);
		GlobalMessenger.Instance?.AddListener<List<TelemetryData>>("TelemetryDataList", OnTelemetryDataReceieved);
	}

	private void OnTelemetryDataReceieved(TelemetryData telemetryData)
	{
		int countOffset = 0;

		if (telemetryDatas.Contains(telemetryData))
		{
			countOffset = telemetryDatas.Count(n => n == telemetryData);
			telemetryData.id = telemetryData.id + countOffset.ToString();
		}

		telemetryDatas.Add(telemetryData);

		telemetryAdded?.Invoke(telemetryData);
	}

	private void OnTelemetryDataReceieved(List<TelemetryData> telemetryData)
	{
		telemetryData.ForEach(x => telemetryDatas.Add(x));
	}

	private void SendData()
	{
		string jsonData = JsonUtility.ToJson(telemetryDatas);

		//Send Json Data for All Telemetrys
	}

	private void OnDestroy()
	{
		GlobalMessenger.Instance?.RemoveListener<TelemetryData>("TelemetryData", OnTelemetryDataReceieved);
		GlobalMessenger.Instance?.RemoveListener<List<TelemetryData>>("TelemetryDataList", OnTelemetryDataReceieved);
	}
}
