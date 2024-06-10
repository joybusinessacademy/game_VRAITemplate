using System;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class AnimatorReverseBoolAsset : PlayableAsset
	{
		public string id;
		public bool resetValueOnEnd;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorReverseBoolBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.id = id;
			behaviour.resetValueOnEnd = resetValueOnEnd;
			return playable;
		}

		public string GetClipDisplayName()
		{
			return "Set Bool " + "\r\n" + (string.IsNullOrWhiteSpace(id) ? "<null>" : id);
		}

		public string GetToolTip()
		{
			return GetClipDisplayName();
		}

		public string GetErrorText()
		{
			return string.IsNullOrWhiteSpace(id) ? "Error: Id cannot be null or empty." : null;
		}
	}
}
