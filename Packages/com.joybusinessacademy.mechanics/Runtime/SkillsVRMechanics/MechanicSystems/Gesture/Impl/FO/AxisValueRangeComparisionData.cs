using System;
using System.Collections.Generic;
using UnityEngine.Animations;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
	[Serializable]
    public class AxisValueRangeComparisionData : CustomGestureDetectStepData
    {
        [Serializable]
        public class AxisValue
        {
            public Axis axis;
            public float value;
        }
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string spaceOwnerId;
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string targetId;

        public List<AxisValue> values = new List<AxisValue>();
        [StringEnumValueDropdown(enumType = typeof(ComparisonType))]
        public string comparisonName;

        public ComparisonType comparisonType
        {
            get
            {

                ComparisonType t = ComparisonType.None;
                if (Enum.TryParse<ComparisonType>(comparisonName, out t))
                {
                    return t;
                }
                return ComparisonType.None;
            }
            set
            {
                comparisonName = value.ToString();
            }
        }
    }
}