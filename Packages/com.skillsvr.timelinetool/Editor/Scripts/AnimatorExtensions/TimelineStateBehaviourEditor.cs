using SkillsVR.TimelineTool.AnimatorTimeline;
using SkillsVR.TimelineTool.Bindings;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.AnimatorTimeline
{
    [CustomEditor(typeof(TimelineDirectorBehaviour))]
    public class TimelineStateBehaviourEditor : UnityEditor.Editor
    {
        TimelineDirectorBehaviour smartTarget;

        private void OnEnable()
        {
            smartTarget = this.target as TimelineDirectorBehaviour;
            if (!Application.isPlaying)
            {
                if (null != smartTarget)
                {
                    smartTarget.OnValidate();
                    this.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            TimelineAsset prevTimeline = smartTarget.timelineAsset;
            base.OnInspectorGUI();
            bool timlineChanged = prevTimeline != smartTarget.timelineAsset;

            
            if (GUILayout.Button("Attach Timeline Bindings") || timlineChanged)
            {
                smartTarget.AttachTrackBindings();
                this.serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Clear Inactive Bindings") || timlineChanged)
            {
                smartTarget.ClearInactiveBindings();
                this.serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Auto build state nodes from selected timeline asset\r\n" +
                "  1. Lock current state inspector by clicking the lock icon on top right;\r\n" +
                "  2. Select 1 or more timeline assets in project view;\r\n" +
                "  3. Click this button, state nodes will be auto connected to current state node.\r\n", 
                MessageType.Info); 
            if (GUILayout.Button("Add Selection Timeline To Animator"))
            {
                AddSelectionTimelines();
            }
        }

        void AddSelectionTimelines()
        {
            if (Application.isPlaying)
            {
                return;
            }
            var path = AssetDatabase.GetAssetPath(smartTarget);
            var controller = AssetDatabase.LoadMainAssetAtPath(path) as AnimatorController;

            var timelines = Selection.objects.Where(x => null != x && x.GetType() == typeof(TimelineAsset)).Select(obj => (TimelineAsset)obj);

            if (timelines.Count() == 0)
            {
                return;
            }
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                   if (state.state.behaviours.Contains(smartTarget))
                    {
                        var fromState = state.state;
                        var pos = state.position;
                        foreach(var item in timelines)
                        {
                            pos += Vector3.right * 350;
                            var toState = layer.stateMachine.AddState(item.name, pos);
                            var addedBeh = toState.AddStateMachineBehaviour<TimelineDirectorBehaviour>();
                            
                            addedBeh.CopyFrom(smartTarget);
                            addedBeh.timelineAsset = item;
                            addedBeh.trackBindings.Clear();
                            addedBeh.AttachTrackBindings();
                            if (null != smartTarget.trackBindings)
                            {
                                foreach (var binding in addedBeh.trackBindings)
                                {
                                    if (null == binding.trackAsset)
                                    {
                                        continue;
                                    }
                                    var similarBinding = smartTarget.trackBindings.Find(x => 
                                            null != x.trackAsset 
                                               && x.trackAsset.name == binding.trackAsset.name
                                                && x.outputType == binding.outputType
                                                  && null != x.valueProvider);
                                    if (null != similarBinding)
                                    {
                                        binding.valueProvider = similarBinding.valueProvider.Clone() as IUnityObjectProvider;
                                    }
                                }
                            }
                            
                           
                            addedBeh.OnValidate();

                            var trans = fromState.AddTransition(toState, true);
                            trans.hasExitTime = true;
                            trans.exitTime = 0.1f;
                            trans.duration = 0.01f;

                            fromState = toState;
                        }

                        EditorUtility.CopySerialized(controller, controller);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        break;
                    }
                }
            }
        }

       
    }
}
