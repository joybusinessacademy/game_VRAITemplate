using System;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LookAtAsset : PlayableAsset, ITimelineClipAsset
{

	public LookAtAsset()
	{
	}
	
	public LookAtAsset(PropGUID<IPropLookAt> propTransform)
	{
		template = new LookAtPlayable
		{
			lookAtPoint = propTransform
		};
		name = propTransform.GetPropName();
	}
	
	[NotKeyable] // NotKeyable used to prevent Timeline from making fields available for animation.
	public LookAtPlayable template = new();

	public ClipCaps clipCaps => ClipCaps.Blending;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<LookAtPlayable>.Create(graph, template);
	}
}