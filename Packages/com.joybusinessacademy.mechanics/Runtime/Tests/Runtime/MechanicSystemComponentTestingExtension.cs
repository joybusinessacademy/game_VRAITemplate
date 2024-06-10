using NUnit.Framework;
using SkillsVR.Mechanic.Core;
using UnityEngine;


namespace SkillsVR.Mechanic.Test.MechanicSystemTests
{
	public static class MechanicSystemComponentTestingExtension
    {
        public static GameObject AddChildGameObject(this Component parentComponent, string name = null, bool gameObjectActive = true)
        {
            Assert.IsNotNull(parentComponent);
            var childGameObject = new GameObject(name);
            childGameObject.transform.SetParent(parentComponent.transform);
            childGameObject.SetActive(gameObjectActive);
            childGameObject.transform.localPosition = Vector3.zero;
            childGameObject.transform.localRotation = Quaternion.identity;
            childGameObject.transform.localScale = Vector3.one;
            return childGameObject;
        }

        public static COMPONENT_TYPE AddComponentOnNewChildGameObject<COMPONENT_TYPE>(this Component parentComponent, bool gameObjectActive = true) where COMPONENT_TYPE : Component
        {
            Assert.IsNotNull(parentComponent);
            var go = parentComponent.AddChildGameObject(typeof(COMPONENT_TYPE).Name, gameObjectActive);
            return go.AddComponent<COMPONENT_TYPE>();
        }

        public static MechanicSystemEventDelegate AddTestEventCallback(this IMechanicSystem mechanicSystem, object eventKey, MechanicSystemEventDelegate eventCallback)
        {
            Assert.IsNotNull(mechanicSystem);
            Assert.IsNotNull(eventKey);
            MechanicSystemEventDelegate callback = (eventArgs) =>
                {
                    if (eventArgs.eventKey.Equals(eventKey))
                    {
                        eventCallback?.Invoke(eventArgs);
                    }
                };
            mechanicSystem.AddListerner(callback);
            return callback;
        }

        public static MechanicSystemEventDelegate AddOneTimeTestEventCallback(this IMechanicSystem mechanicSystem, object eventKey, MechanicSystemEventDelegate eventCallback)
        {
            Assert.IsNotNull(mechanicSystem);
            Assert.IsNotNull(eventKey);
            MechanicSystemEventDelegate removeableCallback = null;
            MechanicSystemEventDelegate callback = (eventArgs) =>
            {
                if (eventArgs.eventKey.Equals(eventKey))
                {
                    eventCallback?.Invoke(eventArgs);
                    mechanicSystem.RemoveListener(removeableCallback);
                }
            };
            removeableCallback = callback;
            mechanicSystem.AddListerner(removeableCallback);
            return removeableCallback;
        }
    }
}
