using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Functions/Wait", typeof(SceneGraph)), NodeMenuItem("Functions/Wait", typeof(SubGraph))]
	//[NodeMenuItem("Functions/Delay", typeof(SceneGraph)), NodeMenuItem("Functions/Delay", typeof(SubGraph))]
	public class WaitNode : ExecutableNode
	{
		public override string name => "Wait";
		public override string icon => "Timer";
		public override string layoutStyle => "WaitNode";

		public override Color color => NodeColours.Other;

		
		public float waitTime = 1f;

		protected override void OnStart()
		{
			WaitMonoBehaviour.Process(waitTime, CompleteNode);
		}
	}
}