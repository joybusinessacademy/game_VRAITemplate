using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(MainStartNode))]
	public class MainStartNodeViewValidation : AbstractNodeViewValidation<BaseNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<MainStartNode>();
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				default: return null;
			}
		}
	}
}
