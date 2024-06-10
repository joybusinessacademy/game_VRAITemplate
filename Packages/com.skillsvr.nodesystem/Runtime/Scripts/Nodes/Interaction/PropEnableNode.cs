using System;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Character & Props/Enable-Disable Prop", typeof(SceneGraph)), NodeMenuItem("Character & Props/Enable-Disable Prop", typeof(SubGraph))]
	public class PropEnableNode : ExecutableNode
	{
		public override string name => "Enable-Disable Prop";
		public override string icon => "Unity";
		public override Color color => NodeColours.CharactersAndProps;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#transition-node";

		public bool setStateToEnabled = true;
		public override int Width => MEDIUM_WIDTH;

		[FormerlySerializedAs("characterName")] public PropGUID<IBaseProp> propName;
		
		protected override void OnStart()
		{
			base.OnStart();
			IBaseProp sceneAnimation = PropManager.GetProp(propName);

			if (sceneAnimation != null)
			{
				sceneAnimation.GetPropComponent().gameObject.SetActive(setStateToEnabled);
			}

			CompleteNode();
		}
	}
}