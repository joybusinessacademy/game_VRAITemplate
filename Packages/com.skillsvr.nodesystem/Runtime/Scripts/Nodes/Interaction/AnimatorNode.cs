using System;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{
	//[Serializable, NodeMenuItem("Interaction/Animator", typeof(SceneGraph)), NodeMenuItem("Interaction/Animator", typeof(SubGraph))]

	public class AnimatorNode : ExecutableNode
	{
		public PropGUID<IPropAnimator> animatorProp;
		
		public string stateNameHash = "Null";
		public int layer = -1;
		
		public bool waitForAnimationToFinish = false;
		public override string name => "Animator";
		public override string icon => "Unity";
		public override Color color => NodeColours.CharactersAndProps;
		public override int Width => MEDIUM_WIDTH;

		public AnimatorState animatorState = AnimatorState.None;
		
		public enum AnimatorState
		{
			None,
			SetBool,
			SetInt,
			SetFloat,
			RunTrigger,
			PlayClip,
		}

		protected override void OnStart()
		{
			Animator animator = PropManager.GetProp<IPropAnimator>(animatorProp)?.GetAnimator();

			if (animator == null)
			{
				Debug.LogWarning($"Could not find animator: {animatorProp}");
				CompleteNode();
				return;
			}
			
			RunAnimation(animator);
		}


		public void RunAnimation(Animator animator)
		{
			switch (animatorState)
			{
				case AnimatorState.None:
					Debug.LogWarning($"Could not find animator: {animatorProp}");
					break;
				case AnimatorState.SetBool:
					break;
				case AnimatorState.SetInt:
					break;
				case AnimatorState.SetFloat:
					break;
				case AnimatorState.RunTrigger:
					
					animator.SetTrigger(stateNameHash);
					CompleteNode();

					break;
				case AnimatorState.PlayClip:
					animator.Play(stateNameHash, layer);
					
					if (!waitForAnimationToFinish)
					{
						CompleteNode();
						break;
					}
			
					AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(Animator.StringToHash(stateNameHash));
					WaitMonoBehaviour.Process(state.length, CompleteNode);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}