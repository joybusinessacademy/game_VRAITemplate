using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Learning/GPT", typeof(SceneGraph)), NodeMenuItem("Learning/GPT", typeof(SubGraph))]
	public class GPTNode : ExecutableNode
	{
		public override string name => "GPT";
		public override string icon => "Dialogue";
		public override string layoutStyle => "GPTNode";

		public override Color color => NodeColours.CharactersAndProps;

		private GameObject instance;
		protected override void OnStart()
		{
			instance = GameObject.Instantiate(Resources.Load("GPTPrefab")) as GameObject;
		}
	}
}