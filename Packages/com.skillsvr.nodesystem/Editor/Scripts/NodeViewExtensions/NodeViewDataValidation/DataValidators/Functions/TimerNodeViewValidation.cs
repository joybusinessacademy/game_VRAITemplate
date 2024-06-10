using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(TimerNode))]
	public class TimerNodeViewValidation : AbstractNodeViewValidation<TimerNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<TimerNode>();
			string timerKey = "Time";
			CheckSpawnPosition();
			ErrorIf(node.mechanicData.amountOfTimeInSeconds < 0.0f, timerKey, "Time must no less than 0. Current is " + node.mechanicData.amountOfTimeInSeconds);
		}
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "Time": return TargetNodeView.Q<FloatField>();
				default: return null;
			}
		}
	}
}
