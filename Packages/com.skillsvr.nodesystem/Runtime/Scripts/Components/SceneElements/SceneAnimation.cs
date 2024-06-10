using System;
using UnityEngine;

namespace SkillsVRNodes
{
	[RequireComponent(typeof(Animator))]
	[Obsolete("All Scene elements and hotspots are now Props. Use Prop instead of SceneElement.")]
	public class SceneAnimation : SceneElement
	{
		private Animator animator;

		public Animator Animator
		{
			get
			{
				if (animator == null)
				{
					animator = GetComponent<Animator>();
				}

				return animator;
			}
		}

		public RuntimeAnimatorController AnimatorController { get; set; }
	}
}