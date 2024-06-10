using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{

    [Serializable]
	public class AnimatorSetBoolAsset : PlayableAsset
	{
		public string id;
		public bool value;
		public bool resetValueOnEnd;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorSetBoolBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.id = id;
			behaviour.value = value;
			behaviour.resetValueOnEnd = resetValueOnEnd;
			return playable;
		}

		public string GetClipDisplayName()
		{
			return "Set Bool " + value.ToString() + "\r\n" + (string.IsNullOrWhiteSpace(id) ? "<null>" : id);
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
