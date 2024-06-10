using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{

    [TrackClipType(typeof(AnimatorReverseBoolAsset))]
    [TrackClipType(typeof(AnimatorSetTriggerAsset))]
    [TrackClipType(typeof(AnimatorSetBoolAsset))]
    [TrackClipType(typeof(AnimatorSetFloatAsset))]
    [TrackClipType(typeof(AnimatorSetIntAsset))]
    [TrackBindingType(typeof(Animator))]
    public class AnimatorParameterTrack : MarkerTrack
    {
    }
}