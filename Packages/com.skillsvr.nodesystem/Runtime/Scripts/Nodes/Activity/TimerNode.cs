using GraphProcessor;
using SkillsVR.Mechanic.Core;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Timer", typeof(SceneGraph)), NodeMenuItem("Learning/Timer", typeof(SubGraph))]
	public class TimerNode : SpawnerNode<SpawnerTimer, ITimerSystem, TimerData>
	{
		public override string name => "Timer";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#timer";
		public override int Width => MEDIUM_WIDTH;

	}
}
