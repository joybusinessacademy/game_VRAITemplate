using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{
    [Serializable]
	public class AnimatorSetFloatAsset : PlayableAsset
	{
		public string id;
		public float value;
		public bool resetValueOnEnd;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorSetFloatBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.id = id;
			behaviour.value = value;
			behaviour.resetValueOnEnd = resetValueOnEnd;
			return playable;
		}

		public string GetClipDisplayName()
		{
			return "Set float " + value.ToString() + "\r\n" + (string.IsNullOrWhiteSpace(id) ? "" : id);
		}

		public string GetToolTip()
		{
			return GetClipDisplayName();
		}

		public string GetErrorText()
		{
			return string.IsNullOrWhiteSpace(id) ? "Null id" : null;
		}
	}
}
