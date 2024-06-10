using System;
using UnityEngine.Animations;

namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
	[Serializable]
    public class AxisRange
    {
        public Axis axis;
        public float min;
        public float max;
    }
}