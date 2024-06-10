using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Marker : MonoBehaviour
{
	public bool isCurrentlyToggeled;

	internal bool isCorrectMarker = false;

	public Image markerImage;
	public Button markerButton;
	public Outline markerOutline;

	public TextMeshProUGUI textMesh;

	public delegate void OnMarkerSelected(Marker marker);
	public OnMarkerSelected markerSelected;

	public bool alreadySelectedOnce = false;

	private bool mechanicDone = false;

	public void SetMechanicDone()
	{
		mechanicDone = true;

		markerButton.interactable = false;
	}
	
	private void Awake()
	{
		isCurrentlyToggeled = false;
		markerOutline.enabled = false;
		mechanicDone = false;
		markerButton.interactable = true;

		markerButton.onClick.AddListener(OnMarkerButton);
	}

	private void OnMarkerButton()
	{
		if (mechanicDone)
		{
			  return;
		}
		
		ChangeMarkerState();

		markerSelected?.Invoke(this);
	}

	public void SetText(string text)
	{
		textMesh.text = text;
	}

	private void ChangeMarkerState()
	{
		isCurrentlyToggeled = !isCurrentlyToggeled;
		markerOutline.enabled = isCurrentlyToggeled;
	}

	public void SetToggledState(bool state)
	{
		isCurrentlyToggeled = state;
		markerOutline.enabled = state;
	}

	public void SetNormalColor(Color color)
	{
		var c = markerButton.colors;
		c.normalColor = color;
		markerButton.colors = c;
	}

	public void SetHighlightedColor(Color color)
	{
		var c = markerButton.colors;
		c.highlightedColor = color;
		markerButton.colors = c;
	}

	public void SetPressedColor(Color color)
	{
		var c = markerButton.colors;
		c.pressedColor = color;
		markerButton.colors = c;
	}

	public void SetDisabledColor(Color color)
	{
		var c = markerButton.colors;
		c.disabledColor = color;
		markerButton.colors = c;
	}
}
