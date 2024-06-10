using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(StartNode))]
	public class StartNodeViewValidation : AbstractNodeViewValidation<BaseNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<StartNode>();
			string fadeDurationName = "FadeDuration";
			ErrorIf(node.fadeDuration < 0.0f, fadeDurationName, "Fade duration must no less than 0. Current is " + node.fadeDuration);
			WarningIf(node.fadeDuration > 20.0f, fadeDurationName, "Fade time may too long. Current is " + node.fadeDuration);
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "FadeDuration": return TargetNodeView.Q<PropertyField>("fadeDuration");
				default: return null;
			}
		}
	}
}
