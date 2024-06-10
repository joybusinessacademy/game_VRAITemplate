using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using DialogExporter;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SkillsVR.UnityExtenstion;

public class RankOrderSystem : AbstractMechanicSystemBehivour<RankOrderSystemData>, IRankOrderSystem
{
	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}

	public GameObject mechanicVisual;

	[Header("SLOT ITEMS")]
	public GameObject slotPrefab;
	public Transform slotContainer;

	[Header("RANK ITEMS")]
	public GameObject rankItemPrefab;
	public Transform rankItemContainer;

	[Header("INIT POSITION ITEMS")]
	public GameObject rankItemPlaceholderPrefab;
	public Transform rankItemInitPositionContainer;

	[Header("GENERAL ITEMS")]
	public Button continueButton;
	public GameObject slotsTitleVisual;
	public GameObject itemsTitleVisual;
	public TextMeshProUGUI slotsTitle;
	public TextMeshProUGUI itemsTitle;

	private List<CardSelectionUIItem> itemInitPosList = new List<CardSelectionUIItem>();
	private List<CardSelectionUIItem> itemList = new List<CardSelectionUIItem>();
	private List<SnapPoint> snapPoints = new List<SnapPoint>();

	[Header("Colors")]
	public Color correctColor;
	public Color incorrectColor;

	private int itemsStacked = 0;

	private Dictionary<int, List<RankOrderItemData>> slottedItems = new Dictionary<int, List<RankOrderItemData>>();
	private List<RankOrderItemData> itemsSlotted = new List<RankOrderItemData>();

	public List<ITelemetry> telemetries = new List<ITelemetry>();

	AnswerTelemetry answerTelemetry = new AnswerTelemetry();
	TimeElpasedTelemetry timeElpasedTelemetry;
	TimedTelemetry timedTelemetry;
	RankOrderTelemetry orderTelemetry = new RankOrderTelemetry();

	MechanicTweenSystem tweenSystem = new MechanicTweenSystem();
	
	private Coroutine tweeningCoroutine;
	private Coroutine fadingCoroutine; 

	[Header("Animation Items")]
	public float speedOfTween = 0.3f;
	public float fadeObjectsTime = 0.3f;

	List<CanvasGroup> scalingObjects = new List<CanvasGroup>();

	public List<PointDragUI> draggables = new List<PointDragUI>();
	public List<UIBoxColliderResize> draggablesColliders = new List<UIBoxColliderResize>();
	public RectTransform contentInitDefault;
	public RectTransform slotItemDefault;

	private bool unevenLists = false;
	private bool finishedSetUp = false;

	[Header("Sounds")]
	public AudioSource rankOrderAudioSource;
	public AudioClip rankItemCorrectSound;
	public AudioClip rankItemIncorrectSound;
	public AudioClip rankItemNeutralSound;
	public AudioClip rankItemGrabbedSound;
	public AudioClip rankItemDroppedSound;

	protected override void Start()
	{
		base.Start();

		timedTelemetry = this.transform.GetOrAddComponent<TimedTelemetry>();
		timeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();

		telemetries.Add(timedTelemetry);
		telemetries.Add(timeElpasedTelemetry);
		telemetries.Add(answerTelemetry);
		telemetries.Add(orderTelemetry);

		string mechID = "Rank Order Question: " + this.gameObject.GetInstanceID().ToString();
		timedTelemetry.id = mechID;
		timeElpasedTelemetry.id = mechID;
		answerTelemetry.id = mechID;
		orderTelemetry.id = mechID;
	}

	protected override void Update()
	{
		foreach (var telemetry in telemetries)
		{
			if (!telemetry.isCompleted && telemetry.IsValidated())
				telemetry.SendEvents();
		}

		if (unevenLists && finishedSetUp)
			ChangeItemSizeOnUpdate();
	}

	private void ChangeItemSizeOnUpdate()
	{
		for (int i = 0; i < draggables.Count; i++)
		{
			var draggable = draggables[i];
			var dragRectTransform = draggable.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)rankItemContainer, dragRectTransform.position))
				dragRectTransform.sizeDelta = Vector2.Lerp(dragRectTransform.sizeDelta, contentInitDefault.rect.size, Time.deltaTime * 30);

			if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)slotContainer, dragRectTransform.position))
				dragRectTransform.sizeDelta = Vector2.Lerp(dragRectTransform.sizeDelta, slotItemDefault.rect.size, Time.deltaTime * 30);
		}
	}

	private void PlayRankOrderSound(AudioClip clipToPlay)
	{
		if(rankOrderAudioSource != null && clipToPlay != null)
		{
			rankOrderAudioSource.clip = clipToPlay;
			rankOrderAudioSource.Play();
		}
	}

	private void SpawnRankItemsAndSlots()
	{
		foreach (var rankItem in rankOrderQuestionData.rankOrderItems)
		{
			GameObject cardSelectionUIItem = Instantiate(rankItemPrefab, rankItemContainer);
			CardSelectionUIItem cardSelection = cardSelectionUIItem.GetComponent<CardSelectionUIItem>();
			cardSelection.text.text = rankItem.title;
			cardSelection.slotNumberID = rankItem.rankItemID;

			if (rankItem.hasCustomSprite && rankItem.customImageSprite != null)
			{
				cardSelection.customImage.gameObject.SetActive(true);
				cardSelection.customImage.sprite = rankItem.customImageSprite;
			}
			else
				cardSelection.customImage.gameObject.SetActive(false);

			cardSelectionUIItem.GetComponent<PointDragUI>().onBeginDragEvent.AddListener(()=> PlayRankOrderSound(rankItemGrabbedSound));

			draggables.Add(cardSelectionUIItem.GetComponent<PointDragUI>());
			draggablesColliders.Add(cardSelectionUIItem.GetComponent<UIBoxColliderResize>());

			itemList.Add(cardSelection);

			GameObject placeholderItem = Instantiate(rankItemPlaceholderPrefab, rankItemInitPositionContainer);
			itemInitPosList.Add(placeholderItem.GetComponent<CardSelectionUIItem>());
		}

		if (rankOrderQuestionData.shuffleAnswers)
		{
			itemList.Shuffle();
		}

		foreach (var rankSlot in rankOrderQuestionData.rankOrderSlots)
		{
			GameObject snapItem = Instantiate(slotPrefab, slotContainer);
			CardSelectionUIItem cardSelection = snapItem.GetComponent<CardSelectionUIItem>();
			cardSelection.text.text = rankSlot.title;

			if (rankSlot.hasCustomTitle)
			{
				cardSelection.customTitleText.gameObject.SetActive(true);
				cardSelection.customTitleText.color = rankSlot.customTitleTextColor;
				cardSelection.customTitleText.text = rankSlot.customTitle;
			}
			else
			{
				cardSelection.customTitleText.gameObject.SetActive(false);
			}


			SnapPoint snapPoint = snapItem.GetComponent<SnapPoint>();
			
			snapPoint.snapPointID = rankSlot.rankSlotID;
			snapPoint.requireSameID = rankOrderQuestionData.slotOnSameID;
			snapPoint.onAnyItemSlotted.AddListener(() => PlayRankOrderSound(rankItemDroppedSound));
			
			snapPoints.Add(snapPoint);
		}

		StartCoroutine(Setup());
	}

	internal RankOrderSystemData rankOrderQuestionData;

	public override void Reset()
	{
		base.Reset();
		ClearItems();
		ClearPlaceHolders();
		ClearSlots();

		slottedItems.Clear();

		itemsStacked = 0;
		unevenLists = false;
		finishedSetUp = false;

		foreach (var telemetry in telemetries)
		{
			if (null == telemetry)
			{
				continue;
			}
			telemetry.isCompleted = false;
		}


		if (null != rankItemContainer && null != rankItemContainer.GetComponent<LayoutGroup>())
		{
			rankItemContainer.GetComponent<LayoutGroup>().enabled = true;
		}
		if (null != rankItemInitPositionContainer && null != rankItemInitPositionContainer.GetComponent<LayoutGroup>())
		{
			rankItemInitPositionContainer.GetComponent<LayoutGroup>().enabled = true;
		}
	}

	private void ClearSlots()
	{
		foreach (var item in snapPoints)
		{
			if (null == item || null == item.gameObject)
			{
				continue;
			}
			Destroy(item.gameObject);
		}
		snapPoints.Clear();
	}

	private void ClearItems()
	{
		foreach(var item in itemList)
		{
			if (null == item || null == item.gameObject)
			{
				continue;
			}
			Destroy(item.gameObject);
		}
		itemList.Clear();
		draggables.Clear();
		draggablesColliders.Clear();
	}

	private void ClearPlaceHolders()
	{
		foreach (var item in itemInitPosList)
		{
			if (null == item || null == item.gameObject)
			{
				continue;
			}
			Destroy(item.gameObject);
		}
		itemInitPosList.Clear();
	}

	public override void SetMechanicData()
	{
		base.SetMechanicData();

		rankOrderQuestionData = mechanicData;

		if (rankOrderQuestionData == null)
		{
			Debug.LogError("Missing Mechanic Data");
			return;
		}

		SpawnRankItemsAndSlots();

		if (rankOrderQuestionData.showTitle)
		{
			string slotTitleTerm = rankOrderQuestionData.slotsTitle;
			string itemTitleTerm = rankOrderQuestionData.itemTitle;

			slotsTitle.text = slotTitleTerm == null ? rankOrderQuestionData.internalSlotsTitle : slotTitleTerm;
			itemsTitle.text = itemTitleTerm == null ? rankOrderQuestionData.internalItemTitle : itemTitleTerm;
		}
		else
		{
			slotsTitleVisual.SetActive(false);
			itemsTitleVisual.SetActive(false);
			slotsTitle.gameObject.SetActive(false);
			itemsTitle.gameObject.SetActive(false);
		}


		if(continueButton.GetComponentInChildren<TextMeshProUGUI>().text != null)
		{
			if (mechanicData.continueButtonTitle != "Continue Button Text")
				continueButton.GetComponentInChildren<TextMeshProUGUI>().text = mechanicData.continueButtonTitle;
			else if (mechanicData.continueButtonTitle == string.Empty)
				continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "NEXT";
		}

		continueButton.onClick.RemoveListener(OnContinueButtonPressed);
		continueButton.onClick.AddListener(OnContinueButtonPressed);
		continueButton.interactable = false;
	}

	public override void SetVisualState(bool state)
	{
		base.SetVisualState(state);

		if (tweeningCoroutine != null)
		{
			StopCoroutine(tweeningCoroutine);
			tweeningCoroutine = null;
		}

		if (fadingCoroutine != null)
		{
			StopCoroutine(fadingCoroutine);
			fadingCoroutine = null;
		}

		if (state)
		{
			scalingObjects.ForEach(y => y.alpha = 0);
			mechanicVisual?.SetActive(state);
		}

		if (!this.gameObject.activeInHierarchy)
			return;

		tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(mechanicVisual.transform, speedOfTween, state, FinishedScaling));
		fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(mechanicVisual.GetOrAddComponent<CanvasGroup>(), state, fadeObjectsTime));
	}

	private void FinishedScaling(bool direction)
	{
		mechanicVisual?.SetActive(direction);
		
		if (direction)
			StartCoroutine(tweenSystem.FadeCanvasObjects(scalingObjects, direction, fadeObjectsTime));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		foreach (var item in snapPoints)
		{
			item.onItemAttach.RemoveListener(CheckIfAllSlotted);
			item.onItemDetach.RemoveListener(ItemBeenDetached);
		}

		continueButton.onClick.RemoveListener(OnContinueButtonPressed);
	}

	public void SetPanelEnable(bool show)
	{
		foreach (Transform child in this.transform)
		{
			child.gameObject.SetActive(show);
		}
	}

	private void SetupSnapPoints()
	{
		bool enableAutoSnap = true;
		int itemCount = itemList.Count;
		for (int i = 0; i < itemCount; i++)
		{
			var snapItem = itemList[i].GetComponent<SnapPointItem>();
			var snapPoint = itemInitPosList[i].GetComponent<SnapPoint>();
			if (null != snapItem)
			{
				snapItem.enableAutoSnap = enableAutoSnap;
				snapItem.SetOriginSnapPoint(snapPoint);
			}
		}
	}

	public IEnumerator Setup()
	{
		yield return new WaitForEndOfFrame();

		if (rankOrderQuestionData.slotOnSameID)
			CheckIfInCorrectSlotted();

		SnapPointListeners();
		SetupSnapPoints();
		SnapAllItems();

		SetStackingStatus();

		yield return new WaitForEndOfFrame();

		ResizeColliders();

		yield return new WaitForEndOfFrame();

		scalingObjects.Clear();
		snapPoints.ForEach(y => scalingObjects.Add(y.transform.GetOrAddComponent<CanvasGroup>()));
		itemList.ForEach(x => scalingObjects.Add(x.transform.GetOrAddComponent<CanvasGroup>()));

		slotItemDefault = snapPoints[0].transform as RectTransform;
		contentInitDefault = itemInitPosList[0].transform as RectTransform;

		if (snapPoints.Count != itemList.Count)
			unevenLists = true;

		rankItemContainer.GetComponent<LayoutGroup>().enabled = false;

		delayForShowingAnswers = rankOrderQuestionData.timeToShowAnswers;

		SetVisualState(rankOrderQuestionData.isVisualOnStart);

		string continueTitleTerm = mechanicData.continueButtonTitle;
		continueButton.GetComponentInChildren<TextMeshProUGUI>().text = continueTitleTerm == null ? mechanicData.internalContinueButtonTitle : continueTitleTerm;
		continueButton.onClick.AddListener(OnContinueButtonPressed);
		continueButton.interactable = false;

		finishedSetUp = true;
	}

	private void ResizeColliders()
	{
		foreach (var item in snapPoints)
		{
			item.GetComponent<UIBoxColliderResize>()?.SetNewBoxColliderSize();
		}
		foreach (var itemList in itemList)
		{
			itemList.GetComponent<UIBoxColliderResize>()?.SetNewBoxColliderSize();
		}
	}

	private void SetStackingStatus()
	{
		if (rankOrderQuestionData.stackItems)
		{
			rankItemContainer.GetComponent<LayoutGroup>().enabled = false;
			rankItemInitPositionContainer.GetComponent<LayoutGroup>().enabled = false;
			itemList.ForEach(x => x.transform.localPosition = Vector3.zero);
			itemInitPosList.ForEach(x => x.transform.localPosition = Vector3.zero);

			RectTransform slotTransformSize = snapPoints[0].transform as RectTransform;

			foreach (var item in itemList)
			{
				RectTransform itemRect = item.transform as RectTransform;
				itemRect.sizeDelta = slotTransformSize.sizeDelta;
			}
		}
	}

	private void SnapPointListeners()
	{
		foreach (var item in snapPoints)
		{
			item.onItemSlotted.RemoveListener(OnItemSlotted);
			item.onItemAttach.RemoveListener(CheckIfAllSlotted);
			item.onItemDetach.RemoveListener(ItemBeenDetached);

			item.onItemSlotted.AddListener(OnItemSlotted);
			item.onItemAttach.AddListener(CheckIfAllSlotted);
			item.onItemDetach.AddListener(ItemBeenDetached);
		}
	}

	private void CheckIfInCorrectSlotted()
	{
		foreach (var item in snapPoints)
		{
			item.incorrectSlotted += IncorrectItemIDSlotted;
		}
	}

	public void IncorrectItemIDSlotted()
	{
		//Incorrect Item has been slotted when checking for IDs
		TriggerEvent(RankOrderSystemEvent.OnIncorrectSlotted);
	}

	public void OnItemSlotted(GameObject item)
	{
		//Incorrect Item has been slotted when checking for IDs
		TriggerEvent(RankOrderSystemEvent.OnItemSlotted, item);
	}

	private void ItemBeenDetached(GameObject attached)
	{
		continueButton.interactable = false;
	}

	private void GetSlottedItems()
	{
		for (int i = 0; i < snapPoints.Count; i++)
		{
			if (snapPoints[i]?.currentItem?.GetComponent<CardSelectionUIItem>()?.slotNumberID == null || 
			    snapPoints[i]?.currentItem?.GetComponent<CardSelectionUIItem>()?.text == null)
				continue;
			RankOrderItemData rankOrderItemData = new RankOrderItemData();
			rankOrderItemData.rankItemID = snapPoints[i].currentItem.GetComponent<CardSelectionUIItem>().slotNumberID;
			rankOrderItemData.title = snapPoints[i].currentItem.GetComponent<CardSelectionUIItem>().text.text;
			if (!slottedItems.ContainsKey(i))
				slottedItems.Add(i, new List<RankOrderItemData> { rankOrderItemData });
			slottedItems[i] = new List<RankOrderItemData> { rankOrderItemData };
		}

		TriggerEvent(RankOrderSystemEvent.OnGetSlottedItems, slottedItems);
	}

	private void GetStackedItems()
	{
		for (int i = 0; i < snapPoints.Count; i++)
		{
			List<RankOrderItemData> rankOrderItemData = new List<RankOrderItemData>();

			foreach (var item in snapPoints[i].stackedObjects)
			{
				RankOrderItemData itemData = new RankOrderItemData
				{
					rankItemID = item.GetComponent<CardSelectionUIItem>().slotNumberID,
					title = item.GetComponent<CardSelectionUIItem>().text.text
				};
				rankOrderItemData.Add(itemData);
			}

			if (!slottedItems.ContainsKey(i))
				slottedItems.Add(i, rankOrderItemData);
		}

		TriggerEvent(RankOrderSystemEvent.OnGetSlottedItems, slottedItems);
	}

	private void CheckIfAllSlotted(GameObject attached)
	{
		bool state = true;

		foreach (var item in snapPoints)
		{
			if (item.currentItem == null)
				state = false;
		}

		if (rankOrderQuestionData.stackItems)
		{
			itemsStacked++;
			attached.SetActive(false);

			if (itemsStacked == itemList.Count)
				continueButton.interactable = true;

			snapPoints.ForEach(x => x.GetComponent<RankOrderVisualStates>().SetSelectState(false));

		}
		else
			continueButton.interactable = state;

	}

	private void SnapAllItems()
	{
		int itemCount = itemList.Count;
		for (int i = 0; i < itemCount; i++)
		{
			var snapItem = itemList[i].GetComponent<SnapPointItem>();
			if (null != snapItem)
			{
				snapItem.Snap();
				snapItem.DelaySnap(0.05f);
			}
		}
	}

	public void OnContinueButtonPressed()
	{
		continueButton.interactable = false;

		orderTelemetry.attemeptsMade++;

		if (rankOrderQuestionData.stackItems)
		{
			//SetVisualState(false);
			//mechanicVisual.SetActive(false);
			PlayRankOrderSound(rankItemNeutralSound);
			TriggerEvent(RankOrderSystemEvent.OnFinished);
			FinishedMechanic();
		}
		else if (rankOrderQuestionData.checkForAnswers)
		{
			StartCoroutine(ShowAnswers());
		}
		else
		{
			//SetVisualState(false);
			//mechanicVisual.SetActive(false);
			PlayRankOrderSound(rankItemNeutralSound);
			TriggerEvent(RankOrderSystemEvent.OnFinished);
			FinishedMechanic();
		}
	}

	protected void FinishedMechanic()
	{

		if (!rankOrderQuestionData.stackItems)
			GetSlottedItems();
		else
			GetStackedItems();

		orderTelemetry.slottedItems = slottedItems;

		orderTelemetry.finishedRankorderMechanic = true;

		StopMechanic();
	}

	private float delayForShowingAnswers = 2f;

	private IEnumerator ShowAnswers()
	{

		bool allCorrect = true;
		Color cb = Color.white;

		foreach (var item in snapPoints)
		{
			bool itemIsCorrect = item.currentItem.GetComponent<CardSelectionUIItem>().slotNumberID == item.snapPointID;
			cb = item.currentItem.GetComponent<CardSelectionUIItem>().itemImage.color;
			item.currentItem.GetComponent<CardSelectionUIItem>().itemImage.color = itemIsCorrect ? correctColor : incorrectColor;

			if (allCorrect && !itemIsCorrect)
				allCorrect = false;
		}

		if (allCorrect)
			PlayRankOrderSound(rankItemCorrectSound);
		else
			PlayRankOrderSound(rankItemIncorrectSound);

		yield return new WaitForSeconds(delayForShowingAnswers);

		if (allCorrect)
		{
			answerTelemetry.answerSelected = true;
			TriggerEvent(RankOrderSystemEvent.OnCorrectFinished);
			FinishedMechanic();
		}
		else if(rankOrderQuestionData.continueOnFail)
		{
			TriggerEvent(RankOrderSystemEvent.OnIncorrectFinished);
			FinishedMechanic();
		}
		else
		{
			foreach (var item in snapPoints)
			{
				bool itemIsCorrect = item.currentItem.GetComponent<CardSelectionUIItem>().slotNumberID == item.snapPointID;

				if (!itemIsCorrect)
				{
					if(!rankOrderQuestionData.keepColorsShown)
						item.currentItem.GetComponent<CardSelectionUIItem>().itemImage.color = cb;
					
					item.currentItem.SetAttachTo(null);
					item.currentItem.Snap();
				}
			}

			
		}
	}
}
