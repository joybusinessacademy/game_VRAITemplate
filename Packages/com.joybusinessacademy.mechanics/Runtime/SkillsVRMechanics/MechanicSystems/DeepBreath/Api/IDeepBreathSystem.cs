using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath
{
    [RegisterMechanicEventParameterType(BreathStateChanged, typeof(bool))]
    public enum DeepBreathEvent
    {
        Idle,
        StartCheck,
        StopCheck,
        BreathIn,
        BreathInMax,
        BreathOut,
        BreathOutMax,
        BreathStateChanged,
        Timeout,
    }

    [RegisterMechanicEvent(enumEventType = typeof(DeepBreathEvent))]
    public interface IDeepBreathSystem : IMechanicSystem<DeepBreathData>
    {
        int fullBreathCount { get; }
        void ActiveBreath(bool active);

        void StartCheckBreath(float duration = 6, float timeout = -1, bool autoHideAfterSuccess = true, bool autoBreathOut = true);
        void StopCheckBreath();
    }
}
