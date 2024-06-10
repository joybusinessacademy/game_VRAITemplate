using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    public class GameObjectProviderCustomValue : IGameObjectProvider
    {
        public Type filterType { get => null; set { } }

        public GameObject value;

        public GameObject GetTypedUnityObject(object caller = null)
        {
            return value;
        }

        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        public object Clone()
        {
            var clone = new GameObjectProviderCustomValue();
            clone.value = this.value;
            return clone;
        }
    }
}