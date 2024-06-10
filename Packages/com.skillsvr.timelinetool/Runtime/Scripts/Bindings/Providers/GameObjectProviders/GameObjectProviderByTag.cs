using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    public class GameObjectProviderByTag : IGameObjectProvider
    {
        public Type filterType { get => null; set { } }

        public string tag;

        public bool enableCache = true;

        protected GameObject cache;

        public GameObject GetTypedUnityObject(object caller = null)
        {
            if (enableCache && null != cache)
            {
                return cache;
            }

            if (!string.IsNullOrEmpty(tag))
            {
                cache = GameObject.FindGameObjectWithTag(tag);
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
            var clone = new GameObjectProviderByTag();
            clone.tag = this.tag;
            clone.enableCache = this.enableCache;
            return clone;
        }
    }
    
}