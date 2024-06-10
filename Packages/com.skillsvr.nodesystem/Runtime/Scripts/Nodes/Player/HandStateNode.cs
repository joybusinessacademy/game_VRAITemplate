using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("User Rig/Hand State", typeof(SceneGraph)), NodeMenuItem("User Rig/Hand State", typeof(SubGraph))]
	public class HandStateNode : ExecutableNode
	{
		public override string name => "Hand State";
		public override string icon => "Player";
		public override int Width => MEDIUM_WIDTH;

		[FormerlySerializedAs("isPointing")] public bool rayPointerVisible;

		public override Color color => NodeColours.UserRig;
		
		protected override void OnStart()
		{
			PlayerDistributer.LocalPlayer.SendMessage(name.Replace(" ", string.Empty), new object[] { rayPointerVisible }, SendMessageOptions.DontRequireReceiver);

			CompleteNode();
		}
	}
}