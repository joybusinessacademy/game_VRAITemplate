using GraphProcessor;
using SkillsVRNodes.Managers;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Flow/Start", typeof(MainGraph))]
	public class MainStartNode : BaseNode
	{
		[Output(name = "Start", allowMultiple = false)]
		public ConditionalLink executes = new();

		public override string name => "Start";
		public override string icon => "start";
		public override Color color => NodeColours.Start;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/base-nodes#start-node";
		public override string layoutStyle => "StartNode";
		public override bool SolidColor => true;

		protected NodeExecutor nodeExecutor;

		public void OnStart(NodeExecutor newRunNodeComponent)
		{
			nodeExecutor = newRunNodeComponent;
			CompleteNode();
		}

		public void CompleteNode()
		{
			nodeExecutor.RunConnectedNodes(this, nameof(executes));
		}
	}
}