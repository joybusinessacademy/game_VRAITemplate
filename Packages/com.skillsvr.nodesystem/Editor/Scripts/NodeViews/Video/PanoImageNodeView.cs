using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.Mechanic.MechanicSystems.PanoImage;
using SkillsVR.VideoPackage;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PanoImageNode))]
	public class PanoImageNodeView : SpawnerNodeView<SpawnerPanoImage, IPanoImageSystem, PanoImageData>
	{
		public PanoImageNode AttachedNode => nodeTarget as PanoImageNode;

		private SliderInt directionSlider;
		private IntegerField directionField;

		public override VisualElement GetNodeVisualElement()
		{
			if (AttachedNode.MechanicData == null || AttachedNode.MechanicData.image == null)
			{
				return new TextLabel("Image file", null);
			}
			return new TextLabel("Image file", AttachedNode.MechanicData.image.name);
		}
		
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			//Location Panel
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
			visualElement.Add(new Divider());

			var spriteDropdown = new AssetDropdown<Texture2D>(evt => AttachedNode.MechanicData.image = evt, AttachedNode.MechanicData.image);
			visualElement.Add(spriteDropdown);
			visualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.imageDuration), "Duration"));
			
			//BuildDirectionSlider();

			return visualElement;
		}

		#region DirectionSlider
		//Left in incase we want to use a slider for direction instead of parent transform
		//private void BuildDirectionSlider()
		//{
		//	directionSlider = new SliderInt("Rotate Image: ", 0, 360);
		//	directionSlider.RegisterValueChangedCallback(OnDirectionSliderChanged);

		//	directionField = new IntegerField();
		//	directionField.style.minWidth = 55;
		//	directionField.style.paddingLeft = 5;

		//	directionSlider.Add(directionField);

		//	controlsContainer.Add(directionSlider);
		//}

		//private void OnDirectionSliderChanged(ChangeEvent<int> evt)
		//{
		//	AttachedNode.mechanicData.imageDirection = evt.newValue;
		//	UpdateDirectionFromNode();
		//}

		//private void UpdateDirectionFromNode()
		//{
		//	directionSlider.SetValueWithoutNotify(AttachedNode.mechanicData.imageDirection);
		//	directionField.SetValueWithoutNotify(directionSlider.value);
		//}

		#endregion
	}
}