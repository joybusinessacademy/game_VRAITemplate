using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using Samples.Editor.General;
using SkillsVR.VisualElements;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using VisualElements;
using System;
using CrazyMinnow.SALSA.Timeline;
using CrazyMinnow.SALSA;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers;
using System.Data;
using CCKManagers;
using SkillsVRNodes.Scripts.Hierarchy;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(TimelineNode))]
    public class TimelineNodeView : BaseNodeView
    {
        public IPropTimeline GetPropTimeline => AttachedNode?.director?.GetProp();

        public TimelineNode AttachedNode => nodeTarget as TimelineNode;

        public override VisualElement GetNodeVisualElement()
        {
            VisualElement visualElement = new();

            Button editTimelineButton = new(EditTimeline)
            {
                text = "Edit Timeline"
            };

            PropDropdown<IPropTimeline> propDrop = new("Trackset", AttachedNode.director, evt =>
            {
                AttachedNode.director = evt;

                editTimelineButton?.SetEnabled(GetCanEditTimeline());
            }, true, typeof(DirectorProp));

            visualElement.Add(propDrop);
            visualElement.Add(editTimelineButton);

            editTimelineButton?.SetEnabled(GetCanEditTimeline());
            return visualElement;
        }


        public override VisualElement GetInspectorVisualElement()
        {
            return GetNodeVisualElement();
        }

        private void EditTimeline()
        {
            //TODO: Show Error for Node
            if (!GetCanEditTimeline())
            {
                return;
            }


            if (AttachedNode.director.GetProp().GetDirector != null)
            {
                if (AttachedNode.director.GetProp().GetDirector.playableAsset == null)
                {
                    AttachedNode.director.GetProp().GetDirector.playableAsset = ScriptableObjectManager.CreateScriptableObject<TimelineAsset>
                                (ScriptableObjectManager.Path() + "/TimelineAsset/", AttachedNode.director.GetPropName());
                }
            }

            AttachedNode.timeline = AttachedNode.director.GetProp().GetTimelineAsset;


            AttachedNode.UpdateAnimationTracksToSceneOffset();
            PlayableDirector propObjectDirector = AttachedNode.director.GetProp().GetDirector;

            EditorGUIUtility.PingObject(propObjectDirector);
            Selection.objects = new UnityEngine.Object[] { propObjectDirector };
            EditorWindow.GetWindow<SceneView>().FrameSelected();
            AssetDatabase.OpenAsset(propObjectDirector.playableAsset);

            EditorWindow.GetWindow<TimelineAssistant>(typeof(TimelineEditorWindow));
            TimelineAssistant.currentDirector = propObjectDirector;
            TimelineAssistant.currentAsset = AttachedNode.timeline;
            PropsHierarchyWindow.highlightedName = propObjectDirector.gameObject.name;
            TimelineAssistant.Refresh();
            TimelineAssistant.TryLockTimelineWindow();

            LayoutManager.ResetTimelineLayout();
        }

        private bool GetCanEditTimeline()
        {
            return AttachedNode.director != null && AttachedNode.director.GetProp()!=null && AttachedNode.director.GetProp().GetDirector != null;
        }
    }
}