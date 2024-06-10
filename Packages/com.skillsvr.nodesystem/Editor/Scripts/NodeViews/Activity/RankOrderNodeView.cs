using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(RankOrderNode))]
	public class RankOrderNodeView : SpawnerNodeView<SpawnerRankOrder, IRankOrderSystem, RankOrderSystemData>
	{
		public RankOrderNode AttachedNode => nodeTarget as RankOrderNode;

		public Dictionary<string, int> rankSortPresets = new Dictionary<string, int>()
		{
			{ "All Items Must Slot" ,0 },
			{ "Fail on First Attempt",1 },
			{ "Items are Stacked" , 2 },
			{ "Items And Slots Same ID" , 3 },
		};

		
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
			visualElement.Add(new Divider());

			visualElement.Add(ShowPresetItems());

			UpdateRankorderBasedOnType();

			visualElement.Add(new Divider());


			Toggle titleToggle = new("Show Titles");
			titleToggle.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.MechanicData.showTitle = evt.newValue;
				SkillsVRGraphWindow.RefreshGraph();
			});
			

			titleToggle.value = AttachedNode.MechanicData.showTitle;

			visualElement.Add(titleToggle);

			if (AttachedNode.MechanicData.showTitle)
			{
				visualElement.Add(AttachedNode.MechanicData.slotsTitle.LocField("Slots Title"));
			}

			var allSlotsContainer = new ListDropdown<RankOrderSlotData>("Slots", AttachedNode.MechanicData.rankOrderSlots, SlotBox);
			allSlotsContainer.onAddButtonClicked += RefreshNode;
			visualElement.Add(allSlotsContainer);

			if (AttachedNode.MechanicData.showTitle)
			{
				visualElement.Add(AttachedNode.MechanicData.itemTitle.LocField("Item Title"));
			}

			var allItemsContainer = new ListDropdown<RankOrderItemData>("Items", AttachedNode.MechanicData.rankOrderItems, ItemBox);
			visualElement.Add(allItemsContainer);

			visualElement.Add(AttachedNode.MechanicData.continueButtonTitle.LocField("Continue Button Title: "));

			visualElement.Add(new Divider());
			
			visualElement.Add(AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.shuffleAnswers), "Randomize answer order"));

			EditorCoroutineUtility.StartCoroutineOwnerless(DelayedUpdatePorts());

			
			RankOrderNode spawnerNode = AttachedNode;

			Toggle continueOnFail = spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.continueOnFail), "Only one answer attempt");
			Toggle showFeedback = spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.checkForAnswers), "Show Correct/Incorrect Feedback");


			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.showTitle), "Show Titles"));
			visualElement.Add(continueOnFail);
			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.stackItems), "Show one item at a time"));
			visualElement.Add(showFeedback);
			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.slotOnSameID), "Match Slot and Item ID"));
			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.keepColorsShown), "Keep Feedback Showing"));
			visualElement.Add(spawnerNode.MechanicData.CustomFloatField(nameof(spawnerNode.MechanicData.timeToShowAnswers), "Add Seconds for Answer Review"));

			

			continueOnFail.RegisterCallback<ChangeEvent<bool>>(evt => {
				UpdatePorts();

			});

			showFeedback.RegisterCallback<ChangeEvent<bool>>(evt => {
				UpdatePorts();

			});
			
			return visualElement;
		}
		public override VisualElement GetNodeVisualElement()
		{
			UpdatePorts();

			return null;
		}

		private VisualElement ShowPresetItems()
		{
			List<string> keyArray = rankSortPresets.Keys.ToList();

			DropdownField dropdown = new DropdownField("Select Rank/Sort Preset: ", keyArray, AttachedNode.currentSelectedRankType);
			int valueToFind = AttachedNode.currentSelectedRankType;

			foreach (var item in rankSortPresets)
			{
				if (item.Value == valueToFind)
				{
					dropdown.value = item.Key;
					break;
				}
			}

			dropdown.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.currentSelectedRankType = rankSortPresets[evt.newValue];

				ResetAllMechanicData();
				UpdateRankorderBasedOnType();
				RefreshNode();
			});

			return dropdown;
		}

		private void UpdateRankorderBasedOnType()
		{
			/*
{ "All Items Must Slot" ,0 },
{ "Fail on First Attempt",1 },
{ "Items are Stacked" , 2 },
*/
			switch (AttachedNode.currentSelectedRankType)
			{
				case 0:
					AttachedNode.MechanicData.checkForAnswers = true;
					AttachedNode.MechanicData.keepColorsShown = true;
					AttachedNode.MechanicData.continueOnFail = false;
					break;
				case 1:
					AttachedNode.MechanicData.checkForAnswers = true;
					AttachedNode.MechanicData.keepColorsShown = true;
					AttachedNode.MechanicData.continueOnFail = true;
					break;
				case 2:
					AttachedNode.MechanicData.checkForAnswers = true;
					AttachedNode.MechanicData.keepColorsShown = true;
					AttachedNode.MechanicData.continueOnFail = false;
					AttachedNode.MechanicData.stackItems = true;
					AttachedNode.MechanicData.slotOnSameID = true;
					break;
				case 3:
					AttachedNode.MechanicData.checkForAnswers = true;
					AttachedNode.MechanicData.keepColorsShown = true;
					AttachedNode.MechanicData.continueOnFail = false;
					AttachedNode.MechanicData.slotOnSameID = true;
					break;

				default:
					break;
			}
		}

		private void ResetAllMechanicData()
		{
			AttachedNode.MechanicData.stackItems = false;
			AttachedNode.MechanicData.continueOnFail = false;
			AttachedNode.MechanicData.checkForAnswers = false;
			AttachedNode.MechanicData.keepColorsShown = false;
			AttachedNode.MechanicData.slotOnSameID = false;
			AttachedNode.MechanicData.showTitle = false;
			AttachedNode.MechanicData.timeToShowAnswers = 2;

		}

		IEnumerator DelayedUpdatePorts()
		{
			yield return new EditorWaitForSeconds(0.05f);
			UpdatePorts();
		}

		private void UpdatePorts()
		{
			if (null == AttachedNode || null == AttachedNode.MechanicData)
			{
				return;
			}
			if (AttachedNode.MechanicData == null)
				return;

			bool showExPorts = AttachedNode.MechanicData.continueOnFail && AttachedNode.MechanicData.checkForAnswers;
			if (GetPortViewsFromFieldName("Correct") != null)
			{
				GetPortViewsFromFieldName("Correct").ForEach(k =>
				{
					k.SetEnabled(showExPorts);
					k.GetEdges().ForEach(edge => edge.style.display = showExPorts == false ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}
			if (GetPortViewsFromFieldName("Incorrect") != null)
			{
				GetPortViewsFromFieldName("Incorrect").ForEach(k =>
				{
					k.SetEnabled(showExPorts);
					k.GetEdges().ForEach(edge => edge.style.display = showExPorts == false ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}
		}
		
		private VisualElement ItemBox(RankOrderItemData data)
		{
			
			VisualElement questionContainer = new();

			questionContainer.Add(data.CustomIntField(nameof(data.rankItemID), "ID"));
			questionContainer.Add(data.title.LocField());

			var test = data.CustomToggle(nameof(data.hasCustomSprite), "Custom Image");
			test.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode());
			questionContainer.Add(test);

			
			var spriteField = new ObjectField("Rank Item Sprite: ");
			spriteField.objectType = typeof(Sprite);
			spriteField.value = data.customImageSprite;
			spriteField.RegisterValueChangedCallback(evt =>
			{
				data.customImageSprite = evt.newValue as Sprite;
				if (AttachedNode.rankItemCustomSprite.Any(x => x.data == data))
				{
					SpriteRankData matchingCustomSprite = AttachedNode.rankItemCustomSprite.FirstOrDefault(x => x.data == data);
				}
				else
				{
					SpriteRankData matchingCustomSprite = new SpriteRankData();
					matchingCustomSprite.data = data;
					AttachedNode.rankItemCustomSprite.Add(matchingCustomSprite);
				}
			});

			if (data.hasCustomSprite)
			{
				questionContainer.Add(spriteField);
			}

			IconButton delete = new IconButton("Close")
			{
				tooltip = "Delete",
			};
			
			delete.clicked += () =>
			{
				AttachedNode.MechanicData.rankOrderItems.Remove(data);
				RefreshNode();
			};
			questionContainer.Add(delete);
			return questionContainer;
		}
		
		private VisualElement SlotBox(RankOrderSlotData data)
		{

			VisualElement questionContainer = new();

			questionContainer.Add(data.CustomIntField(nameof(data.rankSlotID),"ID"));
			questionContainer.Add(data.title.LocField());

			Toggle test = data.CustomToggle(nameof(data.hasCustomTitle), "Custom Title");
			test.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode());
			questionContainer.Add(test);
			if (data.hasCustomTitle)
			{
				questionContainer.Add(data.customTitle.LocField());
			}

			IconButton delete = new IconButton("Close")
			{
				tooltip = "Delete",
			};
			
			delete.clicked += () =>
			{
				AttachedNode.MechanicData.rankOrderSlots.Remove(data);
				RefreshNode();
			};
			questionContainer.Add(delete);

			return questionContainer;
		}
	}
}
