using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
    [Serializable]
    public class WorldToBodySpaceRangeDetector : AbstractGestureStepDetector<WorldToBodySpaceRangeComparisionData>
    {
        protected override bool OnDetect(WorldToBodySpaceRangeComparisionData detectorData, IGestureBody one, IGestureBody other, bool defaultResult)
        {
            var target = other.GetBodyPart(detectorData.targetId);
            if (null == target)
            {
                return defaultResult;
            }
            var head = other.GetBodyPart(nameof(GestureBodyPartType.Head));
            var bodySpaceTargetPos = null == head ? target.GetPosition() : head.WorldToLocalPosition(target.GetPosition());

            if (!IsInXRanges(detectorData.anchorSpaceRanges, head, bodySpaceTargetPos.x))
            {
                return false;
            }
            if (!IsInYRanges(detectorData.anchorSpaceRanges, head, bodySpaceTargetPos.y))
            {
                return false;
            }
            if (!IsInZRanges(detectorData.anchorSpaceRanges, head, bodySpaceTargetPos.z))
            {
                return false;
            }

            return true;
        }
        protected bool IsInXRanges(List<AxisRange> allRanges, IGestureBodyPart localSpaceOwner, float value)
        {
            var ranges = allRanges.Where(x => null != x && x.axis.HasFlag(Axis.X));
            if (0 == ranges.Count())
            {
                return true;
            }
            foreach (var range in ranges)
            {
                var min = null == localSpaceOwner ? range.min : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(range.min, 0.0f)).x;
                var max = null == localSpaceOwner ? range.max : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(range.max, 0.0f)).x;
                if (value > max || value < min)
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        protected bool IsInYRanges(List<AxisRange> allRanges, IGestureBodyPart localSpaceOwner, float value)
        {
            var ranges = allRanges.Where(x => null != x && x.axis.HasFlag(Axis.Y));
            if (0 == ranges.Count())
            {
                return true;
            }
            foreach (var range in ranges)
            {
                var min = null == localSpaceOwner ? range.min : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(0.0f, range.min, 0.0f)).y;
                var max = null == localSpaceOwner ? range.max : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(0.0f, range.max, 0.0f)).y; 
                if (value > max || value < min)
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        protected bool IsInZRanges(List<AxisRange> allRanges, IGestureBodyPart localSpaceOwner, float value)
        {
            var ranges = allRanges.Where(x => null != x && x.axis.HasFlag(Axis.Z));
            if (0 == ranges.Count())
            {
                return true;
            }
            foreach (var range in ranges)
            {
                var min = null == localSpaceOwner ? range.min : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(0.0f, 0.0f, range.min)).z;
                var max = null == localSpaceOwner ? range.max : localSpaceOwner.WorldToLocalPosition(new UnityEngine.Vector3(0.0f, 0.0f, range.max)).z;
                if (value > max || value < min)
                {
                    continue;
                }
                return true;
            }
            return false;
        }
    }
}