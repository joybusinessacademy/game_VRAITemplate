using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    public class BindingCollectionComponentProvider :  IBindingCollectionProvider
    {
        public Type filterType { get => typeof(IUnityObjectBindingCollection); set { } }

        [SerializeReference]
        [ClassPicker]
        public IGameObjectProvider gameObjectProvider;
        public bool enableCache;

        protected IUnityObjectBindingCollection cache;

        public UnityEngine.Object GetBindingValue(string id, object caller)
        {
            var collection = GetTypedUnityObject() as IUnityObjectBindingCollection;
            if (null == collection)
            {
                return null;
            }
            return collection.GetBindingValue(id, caller);
        }

        public Component GetTypedUnityObject(object caller = null)
        {
            if (enableCache && null != cache)
            {
                return cache as Component;
            }

            if (null == gameObjectProvider)
            {
                Debug.LogError("Game object provider must be set before providing component from BindingCollectionComponentProvider.");
                return null;
            }
            var go = gameObjectProvider.GetTypedUnityObject(caller);
            if (null == go)
            {
                return null;
            }
            cache = go.GetComponent(filterType) as IUnityObjectBindingCollection;
            return cache as Component;

        }

        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        public object Clone()
        {
            var clone = new BindingCollectionComponentProvider();
            clone.gameObjectProvider = null == this.gameObjectProvider ? null : this.gameObjectProvider.Clone() as IGameObjectProvider;
            clone.enableCache = this.enableCache;
            return clone;
        }
    }
}