using SkillsVR.TimelineTool;
using SkillsVR.TimelineTool.Editor.TimelineExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [MenuEntry("Set as Blendable Animation")]
    class SetAsBlendableAnimationClip : ClipAction
    {
        public override ActionValidity Validate(IEnumerable<TimelineClip> clips)
        {
            bool canDisplay = clips.Any(c=> null != c.GetAssetAs<AnimationPlayableAsset>());
            return canDisplay ? ActionValidity.Valid : ActionValidity.NotApplicable;
        }

        public override bool Execute(IEnumerable<TimelineClip> clips)
        {
            clips.Where(c => null != c.GetAssetAs<AnimationPlayableAsset>())
                .ToList()
                .ForEach((clip) => {
                    var track = clip.GetParentTrack() as BlendableAnimationTrack;

                    float easeIn = null == track ? TimelineBlenderPreferences.instance.defaultEaseInDuration : track.defaultEaseInDuration;
                    float easeOut = null == track ? TimelineBlenderPreferences.instance.defaultEaseOurDuration : track.defaultEaseOurDuration;
                    var loopMode = null == track ? TimelineBlenderPreferences.instance.defaultAnimLoopMode : track.defaultAnimLoopMode;
                    clip.SetAsAnimatorBlendable(easeIn, easeOut, loopMode);
                });

            return true;
        }
    }
}
