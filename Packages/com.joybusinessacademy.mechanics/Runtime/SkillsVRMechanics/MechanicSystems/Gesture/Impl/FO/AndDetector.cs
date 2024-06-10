using System;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
    [Serializable]
    public class AndDetector : AbstractGestureStepDetector<ListedSubStepDetectionData>
    {
        protected override bool OnDetect(ListedSubStepDetectionData detectorData, IGestureBody one, IGestureBody other, bool defaultResult)
        {
            foreach (var subData in detectorData.subStepDataList)
            {
                if (null == subData)
                {
                    continue;
                }
                if (!ExecDetectFromData(subData, one, other))
                {
                    return false;
                }
            }
            return true;
        }
    }
}