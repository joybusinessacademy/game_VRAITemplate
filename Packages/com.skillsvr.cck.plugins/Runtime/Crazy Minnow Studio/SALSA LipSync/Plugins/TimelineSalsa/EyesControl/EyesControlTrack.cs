using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CrazyMinnow.SALSA.Timeline
{
    [TrackColor(0.1764706f, 0.4039216f, 0.6980392f)]
    [TrackClipType(typeof(EyesControl))]
    [TrackBindingType(typeof(Eyes))]
    public class EyesControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in GetClips())
            {
                var clipAsset = clip.asset as EyesControl;
                if (clipAsset != null)
                    clipAsset.template.trackBinding = (Eyes) go.GetComponent<PlayableDirector>().GetGenericBinding(this); // provides the playable asset with reference to the salsa binding on the track.
            }
            return ScriptPlayable<EyesControlMixer>.Create (graph, inputCount);
        }
    }

    public class EyesControlMixer : PlayableBehaviour {}
}