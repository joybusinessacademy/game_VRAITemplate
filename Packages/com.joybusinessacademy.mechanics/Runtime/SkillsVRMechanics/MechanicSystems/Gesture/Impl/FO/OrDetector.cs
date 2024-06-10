using System;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
    [Serializable]
    public class OrDetector : AbstractGestureStepDetector<ListedSubStepDetectionData>
    {
        protected override bool OnDetect(ListedSubStepDetectionData detectorData, IGestureBody one, IGestureBody other, bool defaultResult)
        {
            foreach (var subData in detectorData.subStepDataList)
            {
                if (null == subData)
                {
                    continue;
                }
                if (ExecDetectFromData(subData, one, other))
                {
                    return true;
                }
            }
            return false;
        }
    }
}