using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(EndNodeView))]
	public class EndNodeViewValidation : AbstractNodeViewValidation<EndNodeView>
	{
		

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<EndNode>();
			string fadeDurationName = "FadeDuration";

			if (node.fade)
			{
				ErrorIf(node.fadeDuration < 0.0f, fadeDurationName, "Fade duration must no less than 0. Current is " + node.fadeDuration);
				WarningIf(node.fadeDuration > 20.0f, fadeDurationName, "Fade time may too long. Current is " + node.fadeDuration);
			}
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "FadeDuration": return TargetNodeView.Q("fade-duration-floatfield");
				default: return null;
			}
		}
	}
}
