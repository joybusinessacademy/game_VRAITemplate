using System;
using System.Collections.Generic;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{

    [Serializable]
    public class AxisRangeComparisonData : CustomGestureDetectStepData
    {
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string spaceOwnerId;
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string targetId;
        public List<AxisRange> ranges = new List<AxisRange>();
    }
}