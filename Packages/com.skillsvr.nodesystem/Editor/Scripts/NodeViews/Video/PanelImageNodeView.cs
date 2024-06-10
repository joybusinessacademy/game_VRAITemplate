using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.Mechanic.MechanicSystems.PanelImage;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PanelImageNode))]
	public class PanelImageNodeView: SpawnerNodeView<SpawnerPanelImage, IPanelImageSystem, PanelImageData>
	{
		public PanelImageNode AttachedNode => nodeTarget as PanelImageNode;

		public override VisualElement GetNodeVisualElement()
		{
			return new TextLabel("Image file", AttachedNode.MechanicData.image != null ? AttachedNode.MechanicData.image.name : null);
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
			visualElement.Add(AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.showNextButton), "Next Button"));
			visualElement.Add(AttachedNode.MechanicData.CustomTextField(nameof(AttachedNode.MechanicData.nextButtonText), "Next Button Text"));

			return visualElement;
		}
	}
}