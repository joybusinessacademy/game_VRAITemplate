using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryVisualLog : MonoBehaviour
{
	private TelemetryController telemetryController;
		
	[Header("SCENE REFERENCES")]
	public Transform newItemSlot;

	[Header("VISUAL PREFABS")]
	public TelemetryDataUI telemetryVisualItem;

	private Dictionary<string, TelemetryDataUI> telemetryItemSlot = new Dictionary<string, TelemetryDataUI>();

	private void Start()
	{
		if (telemetryController == null)
			telemetryController = FindObjectOfType<TelemetryController>();

		if (telemetryController != null)
			telemetryController.telemetryAdded += TelemetryAdded;
	}

	private void TelemetryAdded(TelemetryData telemetryData)
	{
		if(AlreadyHaveTelemetryItem(telemetryData))
		{
			foreach (var item in telemetryItemSlot)
			{
				if (item.Key == telemetryData.mechanicID)
					item.Value.GenerateNewTelemetryData(telemetryData);
			}
		}
		else
		{
			TelemetryDataUI telemetryDataUI = Instantiate(telemetryVisualItem, newItemSlot);
			telemetryDataUI.typeOfTelemetryText.text = "TELEMETRY ID: " + telemetryData.mechanicID;
			telemetryDataUI.GenerateNewTelemetryData(telemetryData);

			telemetryItemSlot.Add(telemetryData.mechanicID, telemetryDataUI);
		}
	}

	private bool AlreadyHaveTelemetryItem(TelemetryData telemetryData)
	{
		return telemetryItemSlot.ContainsKey(telemetryData.mechanicID);
	}

	private void OnDestroy()
	{
		
	}

}
