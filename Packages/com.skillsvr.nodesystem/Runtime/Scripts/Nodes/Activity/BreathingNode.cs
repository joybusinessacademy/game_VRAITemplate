using GraphProcessor;
using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.MechanicSystems.DeepBreath;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Breathing", typeof(SceneGraph)), NodeMenuItem("Learning/Breathing", typeof(SubGraph))]
	public class BreathingNode : SpawnerNode<SpawnerDeepBreath, IDeepBreathSystem, DeepBreathData>
	{
		public override string name => "Breathing";
		public override string icon => "Breathing";

		public override Color color => NodeColours.Learning;
		public override int Width => MEDIUM_WIDTH;

		// update this via node view
		// && means both button must be pressed to trigger
		// the name are referenced on Unity Input Manager
		public string inputIds = "Axis9&&Axis10";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#breathing-node";

		public Dictionary<string, string> inputIdsRemap = new Dictionary<string, string>()
		{
			{ "Any Trigger", "Axis9||Axis10" },
			{ "Both Trigger", "Axis9&&Axis10" },
			{ "Any Grip", "Axis11||Axis12" },
			{ "Both Grip", "Axis11&&Axis12" },
		};

		protected override void OnStart()
		{
			base.OnStart();
			// registering to mediator
			PlayerDistributer.LocalPlayer.SendMessage(name.Replace(" ", string.Empty), new object[]{ mechanicSpawner.gameObject, inputIds}, SendMessageOptions.DontRequireReceiver);
		}

		protected override void OnComplete()
		{
			base.OnComplete();
			// unregistering to mediator
			PlayerDistributer.LocalPlayer.SendMessage(UnregisterMediatorId, mechanicSpawner.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}
