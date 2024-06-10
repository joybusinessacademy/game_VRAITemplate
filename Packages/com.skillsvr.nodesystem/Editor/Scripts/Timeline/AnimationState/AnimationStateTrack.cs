using System;
using UnityEngine;
using UnityEngine.Timeline;

[Serializable]
[TrackBindingType(typeof(Animator))]
[TrackClipType(typeof(AnimationStateAsset))]
[TrackColor(0.1764706f, 0.4039216f, 0.6980392f)]
public class AnimationStateTrack : TrackAsset
{
	public TimelineClip CreateClip()
	{
		TimelineClip newClip = CreateDefaultClip();

		return newClip;
	}
}