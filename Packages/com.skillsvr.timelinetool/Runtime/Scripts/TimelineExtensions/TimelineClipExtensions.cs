using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool
{
    public static class TimelineClipExtensions
    {
        public static T GetAssetAs<T>(this TimelineClip timelineClip) where T : class
        {
            if (null == timelineClip || null == timelineClip.asset)
            {
                return default(T);
            }
            T a = timelineClip.asset as T;
            return a;
        }

		public static void SetPostExtrapolationMode(this TimelineClip clip, TimelineClip.ClipExtrapolation clipExtrapolation)
		{
			SetExtrapolationMode(clip, nameof(TimelineClip.postExtrapolationMode), clipExtrapolation);
		}

		public static void SetPretExtrapolationMode(this TimelineClip clip, TimelineClip.ClipExtrapolation clipExtrapolation)
		{
			SetExtrapolationMode(clip, nameof(TimelineClip.preExtrapolationMode), clipExtrapolation);
		}

		private static void SetExtrapolationMode(this TimelineClip clip, string propertyName, TimelineClip.ClipExtrapolation clipExtrapolation)
		{
			try
			{
				clip.GetType()
					.GetProperty(propertyName)
					.GetSetMethod(true)
					.Invoke(clip, new object[] { clipExtrapolation });
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}


		public static void SetAsAnimatorBlendable(this TimelineClip clip, float easeIn, float easeOut, AnimationPlayableAsset.LoopMode loopMode)
		{
			AnimationPlayableAsset animationPlayableAsset = clip.GetAssetAs<AnimationPlayableAsset>();
			if (null == animationPlayableAsset)
			{
				return;
			}

			animationPlayableAsset.loop = loopMode;

			clip.easeInDuration = easeIn;
			clip.easeOutDuration = easeOut;
			clip.SetPostExtrapolationMode(TimelineClip.ClipExtrapolation.None);
			clip.SetPretExtrapolationMode(TimelineClip.ClipExtrapolation.None);
		}
	}
}