using GraphProcessor;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GPTNode))]
	public class GPTNodeView : BaseNodeView
	{
		private GPTNode AttachedNode => AttachedNode<GPTNode>();

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new VisualElement();
			visualElement.Add(new PropDropdown<IPropAudioSource>("Character: ", AttachedNode.dialoguePosition,
			elementName => AttachedNode.dialoguePosition = elementName, false));

			return visualElement;
		}
	}


}