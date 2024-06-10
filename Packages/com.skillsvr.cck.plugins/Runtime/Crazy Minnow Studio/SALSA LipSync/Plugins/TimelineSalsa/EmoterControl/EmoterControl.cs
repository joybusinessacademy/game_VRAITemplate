using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class EmoterControl : PlayableAsset, ITimelineClipAsset
    {
        public EmoterControlBehavior template = new EmoterControlBehavior();

        // Create the runtime version of the clip, by creating a copy of the template
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<EmoterControlBehavior>.Create (graph, template);
        }

        // Use this to tell the Timeline Editor what features this clip supports
        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }
    }
}
