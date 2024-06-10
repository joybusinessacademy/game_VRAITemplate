using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [TrackClipType(typeof(AnimatorSetSpeedAsset))]
    [TrackBindingType(typeof(Animator))]
    public class AnimatorSpeedTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<AnimatorSpeedTrackMixerBehaviour>.Create(graph, inputCount);
        }
    }
}