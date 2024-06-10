using GraphProcessor;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	public class InvalidNodeViewDataValidation : AbstractNodeViewValidation<BaseNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			return null;
		}

		public override void OnValidate()
		{
			ErrorIf(true, "this", "No data validation found for " + TargetNodeView.GetType().Name);
		}
	}
}

