using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    public abstract class AbstractComponentProviderGetComponent<T> : IComponentProvider where T : Component
    {
        public abstract object Clone();

        [SerializeReference]
        [ClassPicker]
        public IGameObjectProvider gameObjectProvider;

        public bool enableCache = true;
        protected Component cache;

       

        public Type filterType
        {
            get => typeof(T);
            set { }
        }

        

        public Component GetTypedUnityObject(object caller = null)
        {
            if (enableCache && null != cache)
            {
                return cache;
            }


            if (null == gameObjectProvider)
            {
                Debug.LogError("Game object provider must be set before providing component from GetComponent.");
                return null;
            }
            var go = gameObjectProvider.GetTypedUnityObject(caller);
            if (null == go)
            {
                return null;
            }
            cache = go.GetComponent(filterType);
            return cache;

        }
        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        
    }
}