using System;

namespace SkillsVR.TimelineTool.Bindings
{
    public interface IUnityObjectProvider : ICloneable
    {
        UnityEngine.Object GetUnityObject(object caller = null);
    }
}