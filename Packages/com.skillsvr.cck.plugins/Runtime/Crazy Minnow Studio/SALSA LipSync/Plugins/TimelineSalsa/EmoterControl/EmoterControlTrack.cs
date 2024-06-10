using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CrazyMinnow.SALSA.Timeline
{
    [TrackColor(0.1764706f, 0.4039216f, 0.6980392f)]
    [TrackClipType(typeof(EmoterControl))]
    [TrackBindingType(typeof(Emoter))]
    public class EmoterControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                EmoterControl clipAsset = clip.asset as EmoterControl;
                if (clipAsset != null)
                {
                    clipAsset.template.trackBinding = (Emoter) go.GetComponent<PlayableDirector>().GetGenericBinding(this);
                }
            }

            return ScriptPlayable<EmoterControlMixer>.Create (graph, inputCount);
        }
    }
}