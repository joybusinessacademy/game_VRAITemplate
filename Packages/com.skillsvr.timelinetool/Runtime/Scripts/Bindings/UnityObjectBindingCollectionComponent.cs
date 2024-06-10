using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{
    [Serializable]
    [ExecuteInEditMode]
    public class UnityObjectBindingCollectionComponent : MonoBehaviour, IUnityObjectBindingCollection
    {
        public List<UnityObjectBinding> bindings = new List<UnityObjectBinding>();


        private void OnEnable()
        {
#if UNITY_EDITOR
            TestBindings();
#endif
        }
        public void TestBindings()
        {
            foreach(var binding in bindings)
            {
                binding?.Test(this);
            }
        }

        public UnityEngine.Object GetBindingValue(string id, object caller = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var bind = bindings.Find(x => null != x && id == x.id);
            if (null == bind)
            {
                return null;
            }
            caller = null == caller ? this : caller;
            return bind.GetUnityObject(caller);
        }
    }
}