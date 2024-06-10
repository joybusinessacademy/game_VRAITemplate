using GraphProcessor;
using SkillsVRNodes.Managers;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Flow/Go-To Out")]
	public class GoToOutNode : BaseNode
	{
		[Output(name = "Receive Connection")]
		public ConditionalLink executes;
		public GoToInNode.GoToNodes goToNodes;

		public override string name => "Relay Output";
		public override string icon => " ";
		public override Color color => NodeColours.GoTo;
		public override string layoutStyle => "GoToNode";
		public override bool SolidColor => true;

		public void RunExecutedNodes(NodeExecutor runNodeComponent)
		{
			runNodeComponent.RunConnectedNodes(this, nameof(executes));
		}
	}
}
