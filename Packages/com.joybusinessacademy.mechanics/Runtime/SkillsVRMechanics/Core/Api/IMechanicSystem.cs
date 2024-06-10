using UnityEngine;

namespace SkillsVR.Mechanic.Core
{
    public delegate void MechanicSystemEventDelegate(IMechanicSystemEvent eventArgs);


    [RegisterMechanicEvent(enumEventType = typeof(MechSysEvent))]
    public interface IMechanicSystem : IGetFormatString
    {
        Component component { get; }

        bool active { get; }
        bool visualState { get; }

        bool enableUpdateEvent { get; set; }

        void StartMechanic();
        void StopMechanic();
        void SetActive(bool isActive);
        void Reset();
        void SetVisualState(bool show);

        IMechanicSystemEvent CreateEventMessage(object eventType, object data = null, params object[] args);
        void TriggerEvent(object eventType, object data = null, params object[] args);
        void TriggerEvent(IMechanicSystemEvent systemEvent);
        void AddListerner(MechanicSystemEventDelegate listener);
        void RemoveListener(MechanicSystemEventDelegate listener);
    }

    public interface IMechanicSystem<MECHANIC_DATA_TYPE> : IMechanicSystem
    {
        MECHANIC_DATA_TYPE mechanicData { get; set; }
    }
}
