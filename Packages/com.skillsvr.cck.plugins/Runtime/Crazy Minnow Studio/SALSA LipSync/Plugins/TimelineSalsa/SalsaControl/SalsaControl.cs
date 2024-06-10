using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class SalsaControl : PlayableAsset, ITimelineClipAsset
    {
        public SalsaControlBehavior template = new SalsaControlBehavior();

        // Create the runtime version of the clip, by creating a copy of the template
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<SalsaControlBehavior>.Create (graph, template);
        }

        // Use this to tell the Timeline Editor what features this clip supports
        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        // Adjust the default/reset computation of duration to match the length of the AudioClip
        public override double duration
        {
            get
            {
                if (template.audioClip == null)
                    return base.duration;

                return (double)template.audioClip.samples / (double)template.audioClip.frequency;
            }
        }
    }
}