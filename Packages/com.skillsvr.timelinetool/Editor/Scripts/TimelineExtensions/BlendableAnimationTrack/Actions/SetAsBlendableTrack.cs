using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [MenuEntry("Set as Blendable/Only Track Settings")]
    class SetAsBlendableTrack : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            tracks.ToList().ForEach((t) => {
                AnimationTrack animationTrack = t as AnimationTrack;
                if (null == animationTrack)
                {
                    return;
                }
                animationTrack.trackOffset = TrackOffset.ApplySceneOffsets;
            });
            return true;
        }

        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            bool canDisplay = tracks.Any(t => t.GetType() == typeof(AnimationTrack) || typeof(AnimationTrack).IsAssignableFrom(t.GetType()));
            return canDisplay ? ActionValidity.Valid : ActionValidity.NotApplicable;
        }
    }
}
