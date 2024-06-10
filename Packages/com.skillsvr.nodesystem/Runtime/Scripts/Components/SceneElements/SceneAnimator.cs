using UnityEngine;

namespace SkillsVRNodes
{
	[RequireComponent(typeof(Animator))]
	public class SceneAnimator : SceneElement
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