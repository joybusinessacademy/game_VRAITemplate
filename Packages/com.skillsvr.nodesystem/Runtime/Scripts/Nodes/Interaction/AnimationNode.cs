using System;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVR.UnityExtenstion;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Character & Props/Animation", typeof(SceneGraph)), NodeMenuItem("Character & Props/Animation", typeof(SubGraph))]

	public class AnimationNode : ExecutableNode
	{
		public PropGUID<IPropAnimator> animationProp;
	
		public bool waitForAnimationToFinish = false;
		public override string name => "Animation";
		public override string icon => "Unity";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#2d-panel-video-node";

		public override Color color => NodeColours.CharactersAndProps;
		public override int Width => MEDIUM_WIDTH;

		public AnimationClip animation;

		protected override void OnStart()
		{
			base.OnStart();
			IPropAnimator sceneAnimation = PropManager.GetProp(animationProp);

			// This is temp fix for null runtimeAnimatorController as no controller setup in character prefabs.
			// TODO: Fix null runtimeAnimatorController issue and play animation without Animation component.
			if (null != sceneAnimation.GetAnimator().runtimeAnimatorController)
			{
				sceneAnimation.GetAnimator().runtimeAnimatorController.animationClips.Add(animation);
				sceneAnimation.GetAnimator().Play(animation.name);
				WaitMonoBehaviour.Process(animation.length, CompleteNode);
			}
			else
			{
				var animComp = sceneAnimation.GetPropComponent().GetOrAddComponent<Animation>();
				animComp.AddClip(animation, animation.name);
				animComp.clip = animation;
				animComp.Play();
				WaitMonoBehaviour.Process(animation.length, CompleteNode);
			}
			

		}
	}
}