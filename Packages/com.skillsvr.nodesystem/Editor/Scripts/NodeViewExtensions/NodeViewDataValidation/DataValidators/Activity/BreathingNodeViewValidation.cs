using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(BreathingNodeView))]
	public class BreathingNodeViewValidation : AbstractNodeViewValidation<BreathingNodeView>
	{
		

		public override void OnValidate()
		{
			

			var node = TargetNodeView.AttachedNode<BreathingNode>();
			string style1DurationKey = "Style1Duration";

			string style2InTimeKey = "Style2InTime";
			string style2OutTimeKey = "Style2OutTime";
			string style2TimeoutKey = "Style2Timeout";

			CheckSpawnPosition();

			switch(node.mechanicData.style)
			{
				case 0:
					var duration = node.mechanicData.duration;
					ErrorIf(duration <= 0.0f, style1DurationKey, "Duration must no less than 0. Current is " + duration);
					break;
					
				case 1:
					ErrorIf(node.mechanicData.breathIn <= 0.0f, style2InTimeKey, "Breath in time must no less than 0. Current is " + node.mechanicData.breathIn);
					ErrorIf(node.mechanicData.breatheOut <= 0.0f, style2OutTimeKey, "Breath out time must no less than 0. Current is " + node.mechanicData.breatheOut);
					ErrorIf(node.mechanicData.style2Timeout <= 0.0f, style2TimeoutKey, "Timeout must no less than 0. Current is " + node.mechanicData.style2Timeout);
					break;
			}
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "Style1Duration": return TargetNodeView.Q("duration-floatfield");
				case "Style2InTime": return TargetNodeView.Q("breath-in-floatfield");
				case "Style2OutTime": return TargetNodeView.Q("breathe-out-floatfield");
				case "Style2Timeout": return TargetNodeView.Q("style-2-timeout-floatfield");
				default: return null;
			}
		}
	}
}
