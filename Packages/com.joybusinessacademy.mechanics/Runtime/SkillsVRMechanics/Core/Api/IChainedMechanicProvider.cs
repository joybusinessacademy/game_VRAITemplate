using System;
using System.Collections.Generic;

namespace SkillsVR.Mechanic.Core
{
    public interface IChainedMechanicProvider : IMechanicProvider
    {
        List<IMechanicProvider> providers { get; }
    }
}