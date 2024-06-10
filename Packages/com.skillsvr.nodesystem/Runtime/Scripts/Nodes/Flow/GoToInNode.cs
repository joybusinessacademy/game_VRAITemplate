using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Flow/Go-To In")]
	public class GoToInNode : ExecutableNode
	{
		public enum GoToNodes
		{
			GoTo1,
			GoTo2,
			GoTo3,
			GoTo4,
		}
		
		[Input(name = "Send Connection")]
		public new ConditionalLink executed;

		[Output(name = " ")]
		private int Complete = new();
		
		public GoToNodes goToNodes;
		public override string name => "Relay Input";
		public override string icon => " ";
		public override Color color => NodeColours.GoTo;
		public override string layoutStyle => "GoToNode";
		public override bool SolidColor => true;

		protected override void OnStart()
		{
			nodeActive = false;
			List<GoToOutNode> allGoToStartNodes = nodeExecutor.GoToStartNodeList.Where(t => t.goToNodes == goToNodes).ToList();
			foreach (GoToOutNode allStartNodes in allGoToStartNodes)
			{
				allStartNodes.RunExecutedNodes(nodeExecutor);
			}
		}
	}
}