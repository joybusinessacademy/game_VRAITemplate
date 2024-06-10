using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEngine.Timeline
{
    // A track that allows the user to change Text parameters from a Timeline.
    // It demonstrates the following
    //  * How to support blending of timeline clips.
    //  * How to change data over time on Components that is not supported by Animation.
    //  * Putting properties into preview mode.
    //  * Reacting to changes on the clip from the Timeline Editor.
    // Note: This track requires the TextMeshPro package to be installed in the project.
    [TrackColor(0.1394896f, 0.4411765f, 0.3413077f)]
    [TrackClipType(typeof(LegacyTextPlayableAsset))]
    [TrackBindingType(typeof(Text))]
    public class LegacyTextTrack : TrackAsset
    {
        // Creates a runtime instance of the track, represented by a PlayableBehaviour.
        // The runtime instance performs mixing on the timeline clips.
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<LegacyTextTrackMixerBehaviour>.Create(graph, inputCount);
        }

        
    }
}
