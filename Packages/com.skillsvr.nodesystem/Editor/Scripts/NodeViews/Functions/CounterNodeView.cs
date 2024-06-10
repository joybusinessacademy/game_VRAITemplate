using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(CounterNode))]
	public class CounterNodeView : BaseNodeView
	{
		private CounterNode Node => AttachedNode<CounterNode>();

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new VisualElement();
			visualElement.Add(Node.CustomUIntField(nameof(Node.inputsBeforeOutput), "Inputs"));
			visualElement.Add(Node.CustomToggle(nameof(Node.limitOutputs), "Limit Outputs"));
			visualElement.Add(Node.CustomUIntField(nameof(Node.outputsAmount), "Outputs"));

			return visualElement;
		}
	}
}