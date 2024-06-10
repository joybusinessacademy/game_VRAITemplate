using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    public class ComponentProviderGetComponent : IComponentProvider
    {
        [SerializeReference]
        [ClassPicker]
        public IGameObjectProvider gameObjectProvider;

        [SerializeField]
        [SerializableTypePicker(typeof(Component))]
        protected SerializableType componentType;

        public bool enableCache = true;
        protected Component cache;

        public Type filterType 
        {
            get => componentType; 
            set
            {
                componentType = (null != value && typeof(Component).IsAssignableFrom(value)) ? value : null;
            }
        }

        public Component GetTypedUnityObject(object caller = null)
        {
            if (enableCache && null != cache)
            {
                return cache;
            }

            if (null == componentType)
            {
                Debug.LogError("Component Type must be set before providing component from GetComponent.");
                return null;
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
            cache = go.GetComponent(componentType);
            return cache;
            
        }
        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        public object Clone()
        {
            var clone = new ComponentProviderGetComponent();
            clone.gameObjectProvider = null == this.gameObjectProvider ? null : this.gameObjectProvider.Clone() as IGameObjectProvider;
            clone.componentType = this.componentType;
            clone.enableCache = this.enableCache;
            return clone;
        }
    }
}