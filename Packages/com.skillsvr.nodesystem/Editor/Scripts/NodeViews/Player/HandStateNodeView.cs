using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(HandStateNode))]
	public class HandStateNodeView : BaseNodeView
	{
		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			var attachedNode = AttachedNode<HandStateNode>();
			visualElement.Add(attachedNode.CustomToggle(nameof(attachedNode.rayPointerVisible), "Ray Pointer Visible"));
			
			return visualElement;
		}
	}
}