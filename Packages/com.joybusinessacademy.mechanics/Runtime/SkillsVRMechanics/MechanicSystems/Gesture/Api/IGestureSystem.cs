using SkillsVR.Mechanic.Core;
using System.Collections.Generic;
using VRMechanics.Mechanics.GestureDetection;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
    [RegisterMechanicEvent(enumEventType = typeof(DetectionEvents))]
    [RegisterMechanicEventParameterType(DetectionEvents.OnDetect, typeof(bool))]
    [RegisterMechanicEventParameterType(DetectionEvents.StateChanged, typeof(bool))]
    public interface IGestureSystem : IMechanicSystem<GestureDetectionData>
    {
        IEnumerable<GestureDetectStepData> GetAllDetectStepData();
        IGestureBody GetRuntimePose();
        IGestureBody GetTemplate();
    }
}
