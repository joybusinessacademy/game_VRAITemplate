using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

	[Serializable]
	public class AnimatorSetSpeedAsset : PlayableAsset
	{
		public float speed;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<AnimatorSetSpeedBehaviour>.Create(graph);
			var behaviour = playable.GetBehaviour();
			behaviour.speed = speed;
			return playable;
		}

		public string GetClipDisplayName()
		{
			return "Set speed ";
		}

		public string GetToolTip()
		{
			return GetClipDisplayName();
		}

		public string GetErrorText()
		{
			return "";
		}
	}
}
