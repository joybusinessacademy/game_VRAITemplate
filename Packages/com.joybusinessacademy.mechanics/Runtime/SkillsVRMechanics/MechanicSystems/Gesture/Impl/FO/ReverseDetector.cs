using System;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
    [Serializable]
    public class ReversedStepDetectionData : CustomGestureDetectStepData
    {
        public GestureDetectStepData reversedTarget;
    }

    [Serializable]
    public class ReverseDetector : AbstractGestureStepDetector<ReversedStepDetectionData>
    {
        protected override bool OnDetect(ReversedStepDetectionData detectorData, IGestureBody one, IGestureBody other, bool defaultResult)
        {
            if (null == detectorData.reversedTarget)
            {
                return defaultResult;
            }
            return !ExecDetectFromData(detectorData.reversedTarget, one, other);
        }
    }
}