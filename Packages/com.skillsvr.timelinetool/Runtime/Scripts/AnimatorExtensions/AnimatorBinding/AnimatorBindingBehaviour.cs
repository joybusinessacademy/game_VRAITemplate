using SkillsVR.TimelineTool.Bindings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace SkillsVR.TimelineTool.AnimatorTimeline
{
    public class AnimatorBindingBehaviour : StateMachineBehaviour, IUnityObjectBindingCollection
    {
        [SerializeReference]
        [ClassPicker(typeof(IBindingCollectionProvider))]
        public List<IBindingCollectionProvider> outSourceBindingProviders = new List<IBindingCollectionProvider>();
        public List<UnityObjectBinding> bindings = new List<UnityObjectBinding>();


        protected Animator parentAnimator;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            parentAnimator = animator;
                TestBindings();
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex, controller);
            parentAnimator = animator;
        }

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            base.OnStateMachineEnter(animator, stateMachinePathHash);
            parentAnimator = animator;
        }

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
        {
            base.OnStateMachineEnter(animator, stateMachinePathHash, controller);
            parentAnimator = animator;
        }

        public UnityEngine.Object GetBindingValue(string id, object caller = null)
        {
            var obj = FindInSelfBindings(id, null == parentAnimator ? caller : parentAnimator);
            if (null != obj)
            {
                return obj;
            }

            return FindInOutSourceBindings(id, null == parentAnimator ? caller : parentAnimator);
        }

        protected UnityEngine.Object FindInSelfBindings(string id, object caller = null)
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
            return bind.GetUnityObject(caller);
        }

        protected UnityEngine.Object FindInOutSourceBindings(string id, object caller = null)
        {
            foreach (var source in outSourceBindingProviders)
            {
                var obj = source.GetBindingValue(id, caller);
                if (null != obj)
                {
                    return obj;
                }
            }
            return null;
        }


        public void TestBindings()
        {
            foreach (var binding in bindings)
            {
                binding?.Test(parentAnimator);
            }

            int index = -1;
            foreach (var binding in outSourceBindingProviders)
            {
                ++index;
                if (null == binding)
                {
                    continue;
                }
                var obj = binding.GetTypedUnityObject(parentAnimator);
                if (null == obj)
                {
                    Debug.LogError("OutSourceBindingProviders " + index + ": Null Value.");
                }
            }
        }
    }
}