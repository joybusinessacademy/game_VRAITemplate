using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GPTNode))]
	public class GPTNodeView : BaseNodeView
	{
		private GPTNode Node => AttachedNode<GPTNode>();

	}
}