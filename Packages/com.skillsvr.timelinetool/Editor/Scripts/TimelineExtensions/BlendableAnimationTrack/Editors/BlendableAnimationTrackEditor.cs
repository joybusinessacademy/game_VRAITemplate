using SkillsVR.TimelineTool.Editor.TimelineExtensions;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [CustomTimelineEditor(typeof(BlendableAnimationTrack))]
    public class BlendableAnimationTrackEditor : TrackEditor
    {
        public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
        {
            base.OnCreate(track, copiedFrom);
            var blendTrack = track as BlendableAnimationTrack;
            blendTrack.trackOffset = TimelineBlenderPreferences.instance.defaultTrackOffset;
            blendTrack.defaultEaseInDuration = TimelineBlenderPreferences.instance.defaultEaseInDuration;
            blendTrack.defaultEaseOurDuration = TimelineBlenderPreferences.instance.defaultEaseOurDuration;
            blendTrack.defaultAnimLoopMode = TimelineBlenderPreferences.instance.defaultAnimLoopMode;
        }
    }

}
