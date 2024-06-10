using System;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class AnimatorSetTriggerAsset : PlayableAsset
	{
		public string trigger;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorSetTriggerBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.trigger = trigger;
			return playable;
		}
	}
}
