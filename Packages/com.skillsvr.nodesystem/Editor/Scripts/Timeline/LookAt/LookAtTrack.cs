using System;
using CrazyMinnow.SALSA;
using Scripts.Timeline.LookAt;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]

[TrackBindingType(typeof(Eyes))]
[TrackClipType(typeof(LookAtAsset))]
[TrackColor(0.1764706f, 0.4039216f, 0.6980392f)]
public class LookAtTrack : TrackAsset
{
	public Transform lookAtPoint;

	// public TimelineClip CreateClip()
	// {
	// 	return CreateDefaultClip();
	// }

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<LookAtMixerPlayer>.Create(graph, inputCount);
	}
}