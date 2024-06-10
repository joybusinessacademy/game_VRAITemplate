using SkillsVR.TimelineTool;
using UnityEditor.Timeline;
using UnityEngine.Timeline;


namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [CustomTimelineEditor(typeof(BlendableAnimationAsset))]
	class BlendableAnimationAssetEditor : ClipEditor
	{
		public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
		{
			base.OnCreate(clip, track, clonedFrom);
			SetAsBlendableAnimationClip setupAction = new SetAsBlendableAnimationClip();
			setupAction.Execute(new TimelineClip[] { clip });
		}
	}
}