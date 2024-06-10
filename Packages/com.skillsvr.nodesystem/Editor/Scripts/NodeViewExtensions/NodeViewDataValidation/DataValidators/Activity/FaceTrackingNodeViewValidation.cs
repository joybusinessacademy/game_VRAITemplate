using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using SkillsVRNodes.Scripts.Nodes.Nodes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    [CustomDataValidation(typeof(FaceTrackingNodeView))]
    public class FaceTrackingNodeViewValidation : AbstractNodeViewValidation<FaceTrackingNodeView>
    {
        private List<string> guidTracker = new List<string>();

        public override VisualElement OnGetVisualSourceFromPath(string path)
        {

            return null;
        }

        public override void OnValidate()
        {
            var node = TargetNodeView.AttachedNode<FaceTrackingNode>();

            WarningIf(node.targetEmotionDuration > node.nodeDuration, "Duration Warning", "Increase Face Tracking node duration to be greater than Target emotion duration");
            ErrorIf(node.activeEmotionAsset == null && !node.isStopNode, "Tracking Error", "Please select an Emotion to track");

            guidTracker = new List<string>();      
            ErrorIf(!node.isStopNode && !SeekFaceTrackStopNodeRegressive(node) && node.nodeDuration == 0 , "Tracking Error", "If not using auto stop, please attach a Face Tracking node with 'stop node' set to true to this node's complete");

        }


        private bool SeekFaceTrackStopNodeRegressive(BaseNode node)
        {
            if (guidTracker.Contains(node.GUID))
            {
                return false;
            }
            guidTracker.Add(node.GUID);


            var ports = node.GetAllPorts();
            foreach (var port in ports)
            {
                if (port.portData.displayName == "Complete")
                {
                    var edges = port.GetEdges();

                    foreach (var edge in edges)
                    {
                        if (edge.inputPort.portData.displayName == "Start")
                        {
                            BaseNode nextNode = edge.inputPort.owner;

                            if (nextNode as FaceTrackingNode != null)
                            {
                                if ((nextNode as FaceTrackingNode).isStopNode)
                                {
                                    return true;

                                }
                            }
                            SeekFaceTrackStopNodeRegressive(nextNode);
                        }
                    }
                }               
            }
            return false;
        }
  
    }
}
