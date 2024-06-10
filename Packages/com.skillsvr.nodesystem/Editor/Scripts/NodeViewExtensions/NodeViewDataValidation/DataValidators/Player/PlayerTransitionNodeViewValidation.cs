using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PlayerTransitionNodeView))]
	public class PlayerTransitionNodeViewValidation : AbstractNodeViewValidation<PlayerTransitionNodeView>
	{
		
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<PlayerTransitionNode>();

			string timeKey = "Time";

			ErrorIf(node.transitionTime < 0.0f, timeKey, "Transition time must no less than 0. Current is " + node.transitionTime);
			WarningIf(node.transitionTime > 20.0f, timeKey, "Transition time may too long. Current is " + node.transitionTime);
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "Time": return TargetNodeView.Q<FloatField>();
				default: return null;
			}
		}
	}
}
