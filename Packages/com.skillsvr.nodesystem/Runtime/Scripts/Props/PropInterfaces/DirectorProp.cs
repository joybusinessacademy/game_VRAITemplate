using Props.PropInterfaces;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Props
{
	public class DirectorProp : PropType, IPropTimeline
	{
		public PlayableDirector Director;
		public PlayableDirector GetDirector => Director;
		
        public TimelineAsset GetTimelineAsset => Director.playableAsset as TimelineAsset;

        public override void AutoConfigProp()
		{
			Director = propComponent.GetComponent<PlayableDirector>();
			if (Director == null)
			{
				Director = propComponent.gameObject.AddComponent<PlayableDirector>();
			}

		}

		public DirectorProp(PropComponent propComponent) : base(propComponent)
		{
		}
	}
}