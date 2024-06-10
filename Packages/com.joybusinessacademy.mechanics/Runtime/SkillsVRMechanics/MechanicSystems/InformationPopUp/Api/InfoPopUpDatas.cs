using UnityEngine;
using System;
using DialogExporter;


[Serializable]
public class InfoPopUpDatas
{
	[Header("Defaults")]
	public bool isVisualOnSpawn;

	public LocalisedString buttonText = "Next";
	
	public LocalisedString information= new LocalisedString("Information Text");
	public InfoPopUpStyle infoPopUpStyle;

	[Header("InfoPopup State")]
	public bool hasNextButton;
	public float timeUntilDisappear = 5f;

	public bool useMarker = false;

	[Header("MultiMedia")]
	public LocalisedString multiMediaInformation = new LocalisedString("MultiMedia Information");

	[Header("Custom Image")]
	public bool showCustomImage = false;
	public Sprite imageToShow;

	public AudioClip soundBiteClip;
}
