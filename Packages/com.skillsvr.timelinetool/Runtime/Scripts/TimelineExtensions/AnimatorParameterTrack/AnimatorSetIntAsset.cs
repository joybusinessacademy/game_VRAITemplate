using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [Serializable]
	public class AnimatorSetIntAsset : PlayableAsset
	{
		public string id;
		public int value;
		public bool resetValueOnEnd;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorSetIntBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.id = id;
			behaviour.value = value;
			behaviour.resetValueOnEnd = resetValueOnEnd;
			return playable;
		}

		public string GetClipDisplayName()
		{
			return "Set Int " + value.ToString() + "\r\n" + (string.IsNullOrWhiteSpace(id) ? "" : id);
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
