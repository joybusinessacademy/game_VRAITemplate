using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{

    public interface IGestureVisualizer : IMechanicSystem<NO_DATA>
    {
        SpawnerGestureDetection targetGesture { get; set; }
    }
}
