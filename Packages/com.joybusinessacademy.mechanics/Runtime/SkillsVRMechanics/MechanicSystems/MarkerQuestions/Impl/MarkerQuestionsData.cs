using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using DialogExporter;

[Serializable]
public class MarkerQuestionsData //: AbstractMechanicData<MarkerQuestionsData> 
{
	[Header("References")]
	public List<MarkerData> markerDatas = new List<MarkerData>();
	public Transform lockInButtonLocation;

	[Header("Logic")]
	public MarkerQuestionFeedback markerQuestionFeedback;
	public bool isVisualOnStart = false;
	public bool checkStraightAway = true;
	public bool untoggleOthers = true;
	//public bool generateLockInButton = false;
	public bool hideMarkerOnReset = false;
}

[Serializable]
public class MarkerData
{
	public GameObject spawnPoint;
	public bool isCorrectMarker = false;
	public bool useCustomSprite = false;
	public Sprite changeMarkerSprite;
	public LocalisedString customTextForMarker = "";

	[NonSerialized]
	public bool markerIsVisible = true;
}