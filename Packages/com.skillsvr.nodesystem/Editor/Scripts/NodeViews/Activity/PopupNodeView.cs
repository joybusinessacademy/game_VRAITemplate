using GraphProcessor;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections.Generic;
using Props.PropInterfaces;
using UnityEngine.UIElements;
using VisualElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PopupNode))]
	public class PopupNodeView : SpawnerNodeView<SpawnerInformationPopUp, IInformationPopUpSystem, InfoPopUpDatas>
	{
		public PopupNode AttachedNode => nodeTarget as PopupNode;

		
		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
			visualElement.Add(new Divider());
			visualElement.Add(AttachedNode.MechanicData.information.LocField("Information/Title:"));
			visualElement.Add(AttachedNode.MechanicData.multiMediaInformation.LocField("Body Text (Optional):"));
			visualElement.Add(CreateNextButtonElementBlock());
			
			Toggle useImageToggle = AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.showCustomImage));


			if (AttachedNode.MechanicData.showCustomImage)
			{
				ObjectField imageToShow = new("Rank Item Sprite: ");
				imageToShow.objectType = typeof(Sprite);
				imageToShow.value = AttachedNode.MechanicData.imageToShow;
				imageToShow.RegisterValueChangedCallback(evt =>
				{
					AttachedNode.MechanicData.imageToShow = evt.newValue as Sprite;
				});
				visualElement.Add(imageToShow);
			}
			
			//AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.isVisualOnSpawn)),
			visualElement.Add(AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.useMarker)));
			visualElement.Add(useImageToggle);
			
			useImageToggle.RegisterCallback<ChangeEvent<bool>>(_ =>
			{
				RefreshNode();
			});

			return visualElement;
		}

		public override VisualElement GetNodeVisualElement()
		{


			return null;
		}

		private ConditionalElementBlock CreateNextButtonElementBlock()
        {
			PopupNode spawnerNode = AttachedNode;

			Action a = RefreshNode;

			ConditionalElementBlock nextVisualElement = new(
				new List<VisualElement>() {
					spawnerNode.MechanicData.buttonText.LocField()
				},
				new List<VisualElement>() {
					spawnerNode.MechanicData.CustomFloatField(nameof(spawnerNode.MechanicData.timeUntilDisappear))
				},
				spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.hasNextButton), a , null));
			nextVisualElement.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode());
			return nextVisualElement;
        }
	}
}