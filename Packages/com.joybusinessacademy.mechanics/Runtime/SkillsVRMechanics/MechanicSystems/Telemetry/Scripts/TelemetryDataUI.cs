using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TelemetryDataUI : MonoBehaviour
{
    public TelemetryItemUI telemetryItemPrefab;
    public Transform telemetryItemSlot;

    public TextMeshProUGUI typeOfTelemetryText;
    public Button expandTelmetryItemsButton;
    public GameObject expandListVisual;

	private void Awake()
	{
        expandTelmetryItemsButton.onClick.AddListener(ToggleExpandVisual);
    }

	private void ToggleExpandVisual()
	{
        expandListVisual.SetActive(!expandListVisual.activeInHierarchy);
    }

	internal void GenerateNewTelemetryData(TelemetryData telemetryData)
	{
        foreach (var item in telemetryData.data)
		{
            TelemetryItemUI telemetryItem = Instantiate(telemetryItemPrefab, telemetryItemSlot);
            telemetryItem.typeOfTelemetryText.text = item.Key;
            telemetryItem.telemetryDataText.text = item.Value as string;
        } 
       
    }

    private void OnDestroy()
    {
        expandTelmetryItemsButton.onClick.RemoveListener(ToggleExpandVisual);

    }
}
