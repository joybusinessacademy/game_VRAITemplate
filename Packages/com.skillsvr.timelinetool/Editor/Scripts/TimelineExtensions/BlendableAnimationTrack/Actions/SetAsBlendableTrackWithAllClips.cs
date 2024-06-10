﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [MenuEntry("Set as Blendable/Both Track Settings and Children Clips")]
    class SetAsBlendableTrackWithAllClips : TrackAction
    {
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            SetAsBlendableTrack trackSetupAction = new SetAsBlendableTrack();
            trackSetupAction.Execute(tracks);

            tracks.ToList().ForEach((t) => {
                SetAsBlendableAnimationClip clipSetupAction = new SetAsBlendableAnimationClip();
                clipSetupAction.Execute(t.GetClips());
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
