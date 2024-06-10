using System;
using System.Linq;

namespace SkillsVR.TimelineTool.Bindings
{
    public interface IUnityObjectProvider<T> : IUnityObjectProvider where T : UnityEngine.Object
    {
        Type filterType { get; set; }
        T GetTypedUnityObject(object caller = null);
    }
}