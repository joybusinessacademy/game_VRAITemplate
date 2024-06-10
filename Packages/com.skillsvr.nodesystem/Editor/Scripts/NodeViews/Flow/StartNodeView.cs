using GraphProcessor;
using SkillsVR;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(StartNode))]
	public class StartNodeView : BaseNodeView
	{
		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			
			if (owner.graph is MainGraph)
			{
				// If the graph is a scene graph, we don't want to show the fade settings
				return visualElement;
			}

			var attachedNode = AttachedNode<StartNode>();
			visualElement.Add(attachedNode.CustomToggle(nameof(attachedNode.autoFade), "Auto Fade"));
			visualElement.Add(attachedNode.CustomFloatField(nameof(attachedNode.fadeDuration), "Fade Duration"));
			
			return visualElement;
		}
	}
}