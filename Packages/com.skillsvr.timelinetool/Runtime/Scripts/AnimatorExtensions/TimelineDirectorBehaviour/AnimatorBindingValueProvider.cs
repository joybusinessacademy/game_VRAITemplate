using SkillsVR.TimelineTool.Bindings;
using System;
using UnityEngine;

namespace SkillsVR.TimelineTool.AnimatorTimeline
{
    [Serializable]
    public class AnimatorBindingValueProvider : IUnityObjectProvider
    {
        public string id;

        public UnityEngine.Object GetUnityObject(object caller = null)
        {
            var animator = caller as Animator;
            if (null == animator)
            {
                return null;
            }

            var sourceList = animator.GetBehaviours<AnimatorBindingBehaviour>();
            foreach(var bind in sourceList)
            {
                var obj = bind.GetBindingValue(id, caller);
                if (null != obj)
                {
                    return obj;
                }
            }
            return null;
        }

        public object Clone()
        {
            var clone = new AnimatorBindingValueProvider();
            clone.id = this.id;
            return clone;
        }
    }
}