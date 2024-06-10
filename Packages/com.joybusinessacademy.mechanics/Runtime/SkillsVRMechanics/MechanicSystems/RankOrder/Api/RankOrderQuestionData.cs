using System;
using System.Collections.Generic;
using DialogExporter;
using UnityEngine;

[Serializable]
public class RankOrderSystemData
{

	[Header("TITLES")]
	public bool showTitle = false;

	public LocalisedString slotsTitle = new("Slots Title Text");
	public LocalisedString itemTitle = new("Items Title Text");
	public LocalisedString continueButtonTitle = new("Continue Button Text");

	internal string internalSlotsTitle = "SLOTS";
	internal string internalItemTitle = "ITEMS";
	internal string internalContinueButtonTitle = "SUBMIT";

	[Header("RANK ITEMS")]
	public List<RankOrderItemData> rankOrderItems = new List<RankOrderItemData>();
	public List<RankOrderSlotData> rankOrderSlots = new List<RankOrderSlotData>();

	public bool isVisualOnStart = false;
	public bool checkForAnswers = false;
	public bool continueOnFail = false;
	public bool stackItems = false;
	public bool slotOnSameID = false;
	public bool shuffleAnswers = false;
	public bool keepColorsShown = false;
	public float timeToShowAnswers = 2f;
}

[Serializable]
public class RankOrderItemData
{
	public LocalisedString title = new("Item Text");

	public bool hasCustomSprite;
	public Sprite customImageSprite;

	public int rankItemID;
}

[Serializable]
public class RankOrderSlotData
{
	public LocalisedString title = new("Slot Text");

	public int rankSlotID;

	public bool hasCustomTitle = false;
	public LocalisedString customTitle = new("Custom Title Text");
	
	public Color customTitleTextColor = Color.white;
}
