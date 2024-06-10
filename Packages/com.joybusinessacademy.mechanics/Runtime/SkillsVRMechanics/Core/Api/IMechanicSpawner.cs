using System;

namespace SkillsVR.Mechanic.Core
{
    public interface IMechanicSpawner : IGetFormatString
    {
        bool ready { get; }
    }
}
