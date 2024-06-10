using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(WaitNodeView))]
	public class WaitNodeViewValidation : AbstractNodeViewValidation<WaitNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<WaitNode>();
			string timerKey = "Time";
			ErrorIf(node.waitTime < 0.0f, timerKey, "Wait time must no less than 0. Current is " + node.waitTime);
		}
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "Time": return TargetNodeView.Q<FloatField>();
				default: return null;
			}
		}
	}
}
