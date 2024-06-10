using SkillsVR.Impl;
using System.Collections.Generic;

namespace SkillsVR.Mechanic.Core.Impl
{
    public class MechanicSystemEvent : Event<IMechanicSystem>, IMechanicSystemEvent
    {
        public MechanicSystemEvent(IMechanicSystem system, object eventType, object userData = null, params object[] userArgs)
            : base(system, eventType, userData, userArgs)
        {
        }
    }
}
