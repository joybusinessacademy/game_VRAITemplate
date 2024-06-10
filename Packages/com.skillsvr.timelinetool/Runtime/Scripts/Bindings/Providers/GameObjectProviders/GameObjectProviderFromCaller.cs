using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    public class GameObjectProviderFromCaller : IGameObjectProvider
    {
        public Type filterType { get => null; set { } }

        protected GameObject callerGameobj;
        public GameObject GetTypedUnityObject(object caller = null)
        {
            SetCaller(caller);
            if (null == callerGameobj)
            {
                Debug.LogError("SetCaller must be called before providing game object from GameObjectProviderFromCaller.");
            }
            return callerGameobj;
        }

        protected void SetCaller(object callerObj)
        {
            if (null == callerObj)
            {
                return;
            }
            
            var callerType = callerObj.GetType();
            if (typeof(GameObject) == callerType)
            {
                callerGameobj = (GameObject)callerObj;
                return;
            }

            if (typeof(Component).IsAssignableFrom(callerType))
            {
                callerGameobj = ((Component)callerObj).gameObject;
                return;
            }
        }

        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            return GetTypedUnityObject(caller);
        }

        public object Clone()
        {
            return new GameObjectProviderFromCaller();
        }
    }
}