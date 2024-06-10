using GraphProcessor;
using SkillsVRNodes.Managers;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	//[System.Serializable, NodeMenuItem("Functions/SubGraph", typeof(SceneGraph)), NodeMenuItem("Functions/SubGraph", typeof(SubGraph))]
	public class SubGraphNode : ExecutableNode
	{
		public override string name => "SubGraph";
		public override string icon => "SubQuest";
		public override Color color => NodeColours.Other;

		public SubGraph subQuest;
		private NodeExecutor nodeExecutor;


		protected override void OnInitialise()
		{
			nodeExecutor = new NodeExecutor(subQuest);
			nodeExecutor.InitializeGraph();
		}

		protected override void OnStart()
		{
			nodeExecutor.Start();
			nodeExecutor.onEndAction += CompleteNode;
		}
	}
}
