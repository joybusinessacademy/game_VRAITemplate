using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
	public interface IGestureDetectStep
    {
        bool Detect(GestureDetectStepData data, IGestureBody one, IGestureBody other);

        CustomGestureDetectStepData CreateCustomData();
    }
}