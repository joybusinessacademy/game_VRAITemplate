using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(WaitNode))]
	public class WaitNodeView : BaseNodeView
	{
		private WaitNode Node => AttachedNode<WaitNode>();

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new VisualElement();
			visualElement.Add(Node.CustomFloatField(nameof(Node.waitTime), "Seconds"));

			return visualElement;
		}
	}
}