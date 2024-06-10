using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class EyesControl : PlayableAsset, ITimelineClipAsset
    {
        public EyesControlBehavior template = new EyesControlBehavior();
        public ExposedReference<Transform> lookTarget;

        // Create the runtime version of the clip, by creating a copy of the template
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<EyesControlBehavior>.Create (graph, template);
            EyesControlBehavior eyesControlBehavior = playable.GetBehaviour();
            eyesControlBehavior.lookTarget = lookTarget.Resolve(graph.GetResolver());
            return playable;
        }

        // Use this to tell the Timeline Editor what features this clip supports
        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }
    }
}