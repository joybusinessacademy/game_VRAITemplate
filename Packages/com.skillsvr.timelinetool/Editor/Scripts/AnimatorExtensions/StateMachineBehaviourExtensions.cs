using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SkillsVR.TimelineTool.Editor.AnimatorExtensions
{
    public static class StateMachineBehaviourExtensions
    {
        public static StateMachineBehaviourInfo GetInfo(this StateMachineBehaviour behaviour)
        {
            StateMachineBehaviourInfo info = null;
            if (behaviour.TryGetInfo(out info))
            {
                return info;
            }
            return null;
        }

        public static bool TryGetInfo(this StateMachineBehaviour behaviour, out StateMachineBehaviourInfo info)
        {
            info = new StateMachineBehaviourInfo();
            var controller = behaviour.GetParentAnimatorController();
            if (null == controller)
            {
                return false;
            }
            info.animationControllerPath = AssetDatabase.GetAssetPath(controller);
            info.animationControllerName = controller.name;

            var layer = behaviour.GetParentAnimatorControllerLayer();
            if (null == layer)
            {
                return false;
            }
            info.layerIndex = layer.syncedLayerIndex;
            info.layerName = layer.name;

            var state = behaviour.GetParentAnimatorState();
            if (null == state)
            {
                return false;
            }
            info.stateName = state.name;
            info.stateHash = state.nameHash;
            return true;

        }

        public static AnimatorController GetParentAnimatorController(this StateMachineBehaviour behaviour)
        {
            var path = AssetDatabase.GetAssetPath(behaviour);
            var controller = AssetDatabase.LoadMainAssetAtPath(path) as AnimatorController;
            return controller;
        }

        public static AnimatorControllerLayer GetParentAnimatorControllerLayer(this StateMachineBehaviour behaviour)
        {
            var controller = behaviour.GetParentAnimatorController();
            if (null == controller)
            {
                return null;
            }

            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    foreach (var testBehaviour in state.state.behaviours)
                    {
                        if (testBehaviour == behaviour)
                        {
                            return layer;
                        }
                    }
                }
            }
            return null;
        }

        public static AnimatorState GetParentAnimatorState(this StateMachineBehaviour behaviour)
        {
            var layer = behaviour.GetParentAnimatorControllerLayer();
            if (null == layer)
            {
                return null;
            }
            foreach (var state in layer.stateMachine.states)
            {
                foreach (var testBehaviour in state.state.behaviours)
                {
                    if (testBehaviour == behaviour)
                    {
                        return state.state;
                    }
                }
            }
            return null;
        }
    }
}