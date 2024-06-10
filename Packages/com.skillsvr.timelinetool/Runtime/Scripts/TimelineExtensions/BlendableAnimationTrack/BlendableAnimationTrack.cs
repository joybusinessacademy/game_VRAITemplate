using SkillsVR.TimelineTool;
using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{
	[Serializable]
	[TrackBindingType(typeof(Animator))]
	[TrackClipType(typeof(BlendableAnimationAsset), false)]
	public class BlendableAnimationTrack : AnimationTrack
	{
		public float defaultEaseInDuration = 0.1f;
		public float defaultEaseOurDuration = 0.1f;
		public AnimationPlayableAsset.LoopMode defaultAnimLoopMode = AnimationPlayableAsset.LoopMode.On;

		protected override void OnCreateClip(TimelineClip clip)
		{
			base.OnCreateClip(clip);
			clip.SetAsAnimatorBlendable(defaultEaseInDuration, defaultEaseOurDuration, defaultAnimLoopMode);
		}
	}
    
}