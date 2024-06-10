using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [Serializable]
	public class AnimatorSetIntBehaviour : PlayableBehaviour
	{
		public string id;
		public int value;

		public bool resetValueOnEnd;

		protected Animator animator;
		protected int initValue;
		protected Action<object> oneTimeProcessFrame;

		protected void Reset()
		{
			initValue = 0;
			animator = null;
			oneTimeProcessFrame = null;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			base.OnBehaviourPlay(playable, info);
			Reset();
			oneTimeProcessFrame = Init;
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (resetValueOnEnd && null != animator)
			{
				animator.SetInteger(id, initValue);
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

		protected void Init(object playerData)
		{
			animator = playerData as Animator;
			if (null == animator || string.IsNullOrWhiteSpace(id))
			{
				resetValueOnEnd = false;
				return;
			}
			initValue = animator.GetInteger(id);
			animator.SetInteger(id, value);
		}
	}
}
