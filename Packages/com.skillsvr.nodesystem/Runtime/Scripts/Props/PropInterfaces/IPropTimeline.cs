using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Props.PropInterfaces
{
	public interface IPropTimeline : IBaseProp
	{
		public PlayableDirector GetDirector { get; }
		public TimelineAsset GetTimelineAsset { get; }
	}
}