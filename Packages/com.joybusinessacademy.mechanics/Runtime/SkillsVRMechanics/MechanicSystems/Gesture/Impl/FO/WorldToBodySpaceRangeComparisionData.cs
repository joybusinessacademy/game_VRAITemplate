using System;
using System.Collections.Generic;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
	[Serializable]
    public class WorldToBodySpaceRangeComparisionData : CustomGestureDetectStepData
    {
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string targetId;
        public List<AxisRange> anchorSpaceRanges = new List<AxisRange>();
    }
}