using System;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
    public class RangeDetectionResult
    {
        public Vector3 targetPositionInSpace;
        public Vector3 targetPositionWorldSpace;
        public Vector3 rangeRootPositionWorldSpace;
        public bool inRange = false;
        public bool hasResult = false;
    }

    [Serializable]
    public class RangeDetectionData : CustomGestureDetectStepData
    {
        public string detectorId;
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string targetId;
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodySpace))]
        public string bodyPartSpaceId;
        public Vector3 rangeRootPositionInSpace;
        public float range;

        [NonSerialized]
        public RangeDetectionResult result = new RangeDetectionResult();
    }

    [Serializable]
    public class RangeDetector : AbstractGestureStepDetector<RangeDetectionData>
    {
        protected override bool OnDetect(RangeDetectionData detectorData, IGestureBody one, IGestureBody other, bool defaultResult)
        {
            if (null == detectorData.result)
            {
                detectorData.result = new RangeDetectionResult();
            }
            detectorData.result.inRange = defaultResult;

            var target = other.GetRawBodyPart(detectorData.targetId);
            if (null == target)
            {
                return defaultResult;
            }
            var spaceRoot = other.GetRawBodyPart(detectorData.bodyPartSpaceId);
            if (nameof(GestureBodySpace.Body) == detectorData.bodyPartSpaceId)
            {
                GestureBodyPart bodyRoot = new GestureBodyPart();
                var head = other.GetRawBodyPart(nameof(GestureBodyPartType.Head));
                var anchor = other.GetRawBodyPart(nameof(GestureBodyPartType.Anchor));
                if (null != head && null != anchor)
                {
                    bodyRoot.position = head.GetPosition();
                    bodyRoot.position.y = anchor.GetPosition().y;
                    Vector3 angle = head.GetRotation().eulerAngles;
                    angle.x = 0.0f;
                    angle.z = 0.0f;
                    bodyRoot.rotation = angle;
                }
                spaceRoot = bodyRoot;
            }

            if (null == spaceRoot)
            {
                return defaultResult;
            }

            var targetPositionInSpace = spaceRoot.WorldToLocalPosition(target.GetPosition());

            var targetToRootDir = targetPositionInSpace - detectorData.rangeRootPositionInSpace;
            detectorData.result.hasResult = true;
            detectorData.result.targetPositionInSpace = targetPositionInSpace;
            detectorData.result.inRange = targetToRootDir.sqrMagnitude <= detectorData.range * detectorData.range;
            detectorData.result.targetPositionWorldSpace = target.GetPosition();
            detectorData.result.rangeRootPositionWorldSpace = spaceRoot.LocalToWorldPosition(detectorData.rangeRootPositionInSpace);
            return detectorData.result.inRange;
        }
    }
}