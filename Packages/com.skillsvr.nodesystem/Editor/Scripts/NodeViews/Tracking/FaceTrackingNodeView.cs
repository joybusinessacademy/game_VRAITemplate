using GraphProcessor;
using SkillsVR;
using SkillsVRNodes.ScriptableObjects;
using SkillsVRNodes.Scripts.Nodes.Nodes;
using UnityEditor;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(FaceTrackingNode))]
	public class FaceTrackingNodeView : BaseNodeView
	{
		public FaceTrackingNode AttachedNode => AttachedNode<FaceTrackingNode>();
        VisualElement totalBody =  new VisualElement();

        public override VisualElement GetNodeVisualElement()
        {
            return null;
        }

		public override VisualElement GetInspectorVisualElement()
		{
            totalBody = new VisualElement();
            totalBody.Add(SetNodeVersion());

            if (!AttachedNode.isStopNode)
            {
                var elemt = SetNodeDetails();
                elemt.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode());
                totalBody.Add(elemt);
            }
            return totalBody;     
        }

        private VisualElement SetNodeVersion()
        {
            var container = new VisualElement();
           var toggle = AttachedNode.CustomToggle(nameof(AttachedNode.isStopNode));
            toggle.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode() );
            container.Add(toggle);
        
            return container;
        }

        
        private VisualElement SetNodeDetails()
        {
            var container = new VisualElement();


            container.Add(new ScriptableObjectDropdown<EmotionAsset>("Emotion", AttachedNode.activeEmotionAsset, (newValue) => AttachedNode.activeEmotionAsset = newValue, () => { }));
            container.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.targetEmotionDuration)));
            container.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.nodeDuration)));


            //DEBUGING, might be permanant
            container.Add(new ScriptableObjectDropdown<FloatSO>("Change: ", AttachedNode.alteredVariableSO,
                evt => AttachedNode<FaceTrackingNode>().alteredVariableSO = evt,
                () => EditorGUIUtility.PingObject(AttachedNode.alteredVariableSO)));
          
            return container;
        }
    }

}
