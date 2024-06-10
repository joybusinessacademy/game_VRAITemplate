using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PlayerTransitionNode))]
	public class PlayerTransitionNodeView : BaseNodeView
	{
		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}
		
		public override VisualElement GetInspectorVisualElement()
		{
			var attachedNode = AttachedNode<PlayerTransitionNode>();
			VisualElement visualElement = new();
			visualElement.Add(attachedNode.CustomFloatField(nameof(attachedNode.transitionTime), "Duration"));


			var color = new ColorField
			{
				label = "Color",
				value = attachedNode.targetColor,
				showAlpha = true
			};
			color.RegisterValueChangedCallback(c =>
			{
				attachedNode.targetColor = c.newValue;
			});
			
			visualElement.Add(color);
				
			return visualElement;
		}
	}
}