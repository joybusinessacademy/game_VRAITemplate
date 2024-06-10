using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[Serializable]
public class AnimationStatePlayable : PlayableBehaviour
{
	public RuntimeAnimatorController animatorController;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		Animator animator = playerData as Animator;
		if (animator == null || !Application.isPlaying)
		{
			return;
		}
		animator.runtimeAnimatorController = animatorController;
	}
}