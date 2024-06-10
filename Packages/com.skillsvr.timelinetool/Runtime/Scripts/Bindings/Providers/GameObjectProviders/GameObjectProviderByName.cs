using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{

    [Serializable]
    public class GameObjectProviderByName : IGameObjectProvider
    {
        public Type filterType { get => null; set { } }

        public string name;

        public bool enableCache = true;

        protected GameObject cache;

        public GameObject GetTypedUnityObject(object caller = null)
        {
            if (enableCache && null != cache)
            {
                return cache;
            }

            if (!string.IsNullOrEmpty(name))
            {
                cache = GameObject.Find(name);
                return cache;
            }
            return null;
        }
        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        public object Clone()
        {
            var clone = new GameObjectProviderByName();
            clone.name = this.name;
            clone.enableCache = this.enableCache;
            return clone;
        }
    }
    
}