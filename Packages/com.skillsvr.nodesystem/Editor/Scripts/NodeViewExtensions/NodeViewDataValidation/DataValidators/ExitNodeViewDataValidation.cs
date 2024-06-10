using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(ExitGroupNode))]
	public class ExitNodeViewDataValidation : AbstractNodeViewValidation<BaseNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			return null;
		}

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<ExitGroupNode>();
		}
	}
}
