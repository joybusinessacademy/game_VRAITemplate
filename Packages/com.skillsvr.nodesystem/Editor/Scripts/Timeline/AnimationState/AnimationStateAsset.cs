using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[CreateAssetMenu(fileName = "AnimationState", menuName = "SKILLS ANIMATOR/AnimationState", order = 1)]
public class AnimationStateAsset : PlayableAsset, ITimelineClipAsset
{
	[NotKeyable] // NotKeyable used to prevent Timeline from making fields available for animation.
	public AnimationStatePlayable template = new();
	
	public ClipCaps clipCaps => ClipCaps.Blending;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<AnimationStatePlayable>.Create(graph, template);
	}
}