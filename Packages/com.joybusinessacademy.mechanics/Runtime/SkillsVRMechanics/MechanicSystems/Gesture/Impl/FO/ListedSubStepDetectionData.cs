using System;
using System.Collections.Generic;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
	[Serializable]
    public class ListedSubStepDetectionData : CustomGestureDetectStepData
    {
        public List<GestureDetectStepData> subStepDataList = new List<GestureDetectStepData>();
    }
}