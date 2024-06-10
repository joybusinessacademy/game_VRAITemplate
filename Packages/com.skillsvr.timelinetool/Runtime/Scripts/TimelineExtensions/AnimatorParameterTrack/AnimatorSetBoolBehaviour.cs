using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class AnimatorSetBoolBehaviour : PlayableBehaviour
	{
		public string id;
		public bool value;

		public bool resetValueOnEnd;

		protected Animator animator;
		protected bool initValue;
		protected Action<object> oneTimeProcessFrame;

		protected void Reset()
		{
			initValue = false;
			animator = null;
			oneTimeProcessFrame = null;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			base.OnBehaviourPlay(playable, info);
			Reset();
			oneTimeProcessFrame = OnPlay;
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (resetValueOnEnd && null != animator)
			{
				animator.SetBool(id, initValue);
			}

			base.OnBehaviourPause(playable, info);
			Reset();

		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			base.ProcessFrame(playable, info, playerData);
			if (null != oneTimeProcessFrame)
			{
				oneTimeProcessFrame.Invoke(playerData);
				oneTimeProcessFrame = null;
			}
		}

		protected void OnPlay(object playerData)
		{
			animator = playerData as Animator;
			if (null == animator || string.IsNullOrWhiteSpace(id))
			{
				resetValueOnEnd = false;
				return;
			}
			initValue = animator.GetBool(id);
			animator.SetBool(id, value);
		}
	}
}
