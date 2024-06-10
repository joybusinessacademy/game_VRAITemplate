using System;
using Props.PropInterfaces;
using SkillsVR.UnityExtenstion;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Props
{
	[Serializable]
	public class CharacterProp : PropType, IPropNPCAnimator, IPropLookAt
	{
		public override string name => "Character Prop";

		public Animator animator;
		public AudioSource audioSource;
		//public PlayableDirector playableDirector;
		public Transform lookAtPoint;

		public CharacterProp(PropComponent propComponent) : base(propComponent)
		{
		}
		
		public AudioSource GetAudioSource()
		{
			return audioSource;
		}

		public Animator GetAnimator()
		{
			return animator;
		}

        public override void AutoConfigProp()
		{
			if (animator == null)
			{
				animator = propComponent.GetOrAddComponent<Animator>(true);
			}
			if (audioSource == null)
			{
				audioSource = propComponent.GetOrAddComponent<AudioSource>(true);
			}
			
			lookAtPoint = RecursiveFindChild(propComponent.transform, "head");
			lookAtPoint ??= RecursiveFindChild(propComponent.transform, "head.x");
			lookAtPoint ??= propComponent.transform;
		}

		Transform RecursiveFindChild(Transform parent, string childName)
		{
			foreach (Transform child in parent)
			{
				if(child.name == childName)
				{
					return child;
				}

				Transform found = RecursiveFindChild(child, childName);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}
		
		public Transform GetLookAtTransform()
		{
			return lookAtPoint;
		}
	}
}